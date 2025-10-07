#!/usr/bin/env python3
"""
Verification Script for Document Cleanup Results
Analyzes the document structure after cleanup to verify success
"""

import requests
import json
from collections import defaultdict, Counter

class CleanupVerifier:
    def __init__(self, base_url: str = "https://rqmtmgmt.local"):
        self.base_url = base_url
        self.document_id = 11  # TestFlow Pro SRS Document
        self.session = requests.Session()
        
        # Expected final counts from original document
        self.expected_sections = 67
        self.expected_requirements = 176
    
    def setup_auth(self, token: str = None):
        """Setup authentication headers"""
        if token:
            self.session.headers.update({
                'Authorization': f'Bearer {token}',
                'Content-Type': 'application/json'
            })
    
    def get_document_sections(self):
        """Get all sections for the document"""
        try:
            response = self.session.get(
                f"{self.base_url}/api/DocumentSections/document/{self.document_id}",
                params={'includeChildren': True}
            )
            response.raise_for_status()
            return response.json()
        except requests.RequestException as e:
            print(f"Error fetching document sections: {e}")
            return []
    
    def get_document_requirements(self):
        """Get all requirements for the document"""
        try:
            response = self.session.get(
                f"{self.base_url}/api/Requirement/document/{self.document_id}"
            )
            response.raise_for_status()
            return response.json()
        except requests.RequestException as e:
            print(f"Error fetching document requirements: {e}")
            return []
    
    def analyze_duplicates(self, sections, requirements):
        """Analyze remaining duplicates"""
        # Check for duplicate section titles
        section_titles = defaultdict(list)
        for section in sections:
            title = section.get('title', '').strip()
            if title:
                section_titles[title].append(section['id'])
        
        duplicate_sections = {title: ids for title, ids in section_titles.items() if len(ids) > 1}
        
        # Check for duplicate requirement descriptions
        req_descriptions = defaultdict(list)
        for req in requirements:
            desc = req.get('description', '').strip()
            if desc:
                req_descriptions[desc].append(req['id'])
        
        duplicate_requirements = {desc: ids for desc, ids in req_descriptions.items() if len(ids) > 1}
        
        return duplicate_sections, duplicate_requirements
    
    def analyze_section_hierarchy(self, sections):
        """Analyze section hierarchy structure"""
        levels = Counter()
        parent_child = defaultdict(list)
        
        for section in sections:
            level = section.get('level', 0)
            levels[level] += 1
            
            parent_id = section.get('parentSectionId')
            if parent_id:
                parent_child[parent_id].append(section['id'])
        
        return levels, parent_child
    
    def verify_cleanup(self, token: str = None):
        """Run verification analysis"""
        if token:
            self.setup_auth(token)
        
        print("🔍 Document Cleanup Verification")
        print("=" * 50)
        
        # Get current document state
        sections = self.get_document_sections()
        requirements = self.get_document_requirements()
        
        print(f"📋 Document ID: {self.document_id}")
        print(f"📁 Current Sections: {len(sections)}")
        print(f"📄 Current Requirements: {len(requirements)}")
        print()
        
        # Compare with expected counts
        print("🎯 Target vs Actual Comparison:")
        print(f"   📁 Sections: Expected {self.expected_sections}, Actual {len(sections)}")
        print(f"   📄 Requirements: Expected {self.expected_requirements}, Actual {len(requirements)}")
        
        sections_diff = len(sections) - self.expected_sections
        requirements_diff = len(requirements) - self.expected_requirements
        
        if sections_diff == 0:
            print("   ✅ Section count matches expected!")
        else:
            print(f"   ⚠️  Section count differs by {sections_diff}")
        
        if requirements_diff == 0:
            print("   ✅ Requirements count matches expected!")
        else:
            print(f"   ⚠️  Requirements count differs by {requirements_diff}")
        
        print()
        
        # Check for remaining duplicates
        print("🔍 Duplicate Analysis:")
        duplicate_sections, duplicate_requirements = self.analyze_duplicates(sections, requirements)
        
        if duplicate_sections:
            print(f"   ❌ Found {len(duplicate_sections)} section groups with duplicates:")
            for title, ids in duplicate_sections.items():
                print(f"      '{title}': {len(ids)} instances (IDs: {', '.join(map(str, ids))})")
        else:
            print("   ✅ No duplicate sections found!")
        
        if duplicate_requirements:
            print(f"   ❌ Found {len(duplicate_requirements)} requirement groups with duplicates:")
            for desc, ids in list(duplicate_requirements.items())[:5]:  # Show first 5
                print(f"      '{desc[:50]}...': {len(ids)} instances")
            if len(duplicate_requirements) > 5:
                print(f"      ... and {len(duplicate_requirements) - 5} more")
        else:
            print("   ✅ No duplicate requirements found!")
        
        print()
        
        # Analyze hierarchy structure
        print("📊 Section Hierarchy Analysis:")
        levels, parent_child = self.analyze_section_hierarchy(sections)
        
        for level in sorted(levels.keys()):
            print(f"   Level {level}: {levels[level]} sections")
        
        # Count orphaned sections (no parent but not level 0)
        orphaned = [s for s in sections if s.get('level', 0) > 0 and not s.get('parentSectionId')]
        if orphaned:
            print(f"   ⚠️  Found {len(orphaned)} orphaned sections")
        else:
            print("   ✅ No orphaned sections found")
        
        print()
        
        # Overall assessment
        print("📈 Overall Assessment:")
        
        issues = []
        if sections_diff != 0:
            issues.append(f"Section count mismatch ({sections_diff:+d})")
        if requirements_diff != 0:
            issues.append(f"Requirements count mismatch ({requirements_diff:+d})")
        if duplicate_sections:
            issues.append(f"{len(duplicate_sections)} duplicate section groups")
        if duplicate_requirements:
            issues.append(f"{len(duplicate_requirements)} duplicate requirement groups")
        if orphaned:
            issues.append(f"{len(orphaned)} orphaned sections")
        
        if not issues:
            print("   ✅ SUCCESS: Document cleanup appears to be complete!")
            print("   📋 All targets met, no duplicates or structural issues found.")
        else:
            print("   ⚠️  Issues found:")
            for issue in issues:
                print(f"      • {issue}")
            print()
            print("   🔧 Recommended actions:")
            if duplicate_sections or duplicate_requirements:
                print("      • Re-run cleanup script to address remaining duplicates")
            if sections_diff != 0 or requirements_diff != 0:
                print("      • Manual review may be needed for count discrepancies")
            if orphaned:
                print("      • Fix section parent-child relationships")
        
        return len(issues) == 0


def main():
    import argparse
    
    parser = argparse.ArgumentParser(description='Verify document cleanup results')
    parser.add_argument('--token', type=str,
                       help='Authentication token for API access')
    parser.add_argument('--base-url', type=str, default='https://rqmtmgmt.local',
                       help='Base URL for the API')
    
    args = parser.parse_args()
    
    verifier = CleanupVerifier(base_url=args.base_url)
    
    try:
        success = verifier.verify_cleanup(token=args.token)
        exit(0 if success else 1)
    except Exception as e:
        print(f"❌ Verification failed: {e}")
        exit(1)


if __name__ == '__main__':
    main()