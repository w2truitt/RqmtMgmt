#!/usr/bin/env python3
"""
Document Duplicate Cleanup Script
Cleans up duplicate sections and requirements in Project 25 Document 11 (TestFlow Pro SRS)

Based on analysis from:
- COMPREHENSIVE_REVIEW_REPORT.md
- DOCUMENT_COMPARISON_REPORT.md  
- DETAILED_DOCUMENT_ANALYSIS.md

Issues to fix:
- 29 duplicate section groups
- 175 duplicate requirement groups (49% of all requirements)
- Document inflated from 67 sections/176 requirements to 96 sections/355 requirements
"""

import requests
import json
import sys
from typing import Dict, List, Set, Tuple
from collections import defaultdict
import time

class DocumentCleanup:
    def __init__(self, base_url: str = "https://rqmtmgmt.local"):
        self.base_url = base_url
        self.document_id = 11  # TestFlow Pro SRS Document
        self.session = requests.Session()
        
        # Statistics tracking
        self.stats = {
            'sections_removed': 0,
            'requirements_removed': 0,
            'sections_updated': 0,
            'requirements_updated': 0
        }
    
    def setup_auth(self, token: str = None):
        """Setup authentication headers"""
        if token:
            self.session.headers.update({
                'Authorization': f'Bearer {token}',
                'Content-Type': 'application/json'
            })
        else:
            print("Warning: No authentication token provided. API calls may fail.")
    
    def get_document_sections(self) -> List[Dict]:
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
    
    def get_document_requirements(self) -> List[Dict]:
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
    
    def find_duplicate_sections(self, sections: List[Dict]) -> Dict[str, List[Dict]]:
        """Find sections with identical titles"""
        duplicates = defaultdict(list)
        
        for section in sections:
            title = section.get('title', '').strip()
            if title:
                duplicates[title].append(section)
        
        # Filter to only actual duplicates (more than 1 section with same title)
        return {title: secs for title, secs in duplicates.items() if len(secs) > 1}
    
    def find_duplicate_requirements(self, requirements: List[Dict]) -> Dict[str, List[Dict]]:
        """Find requirements with identical descriptions"""
        duplicates = defaultdict(list)
        
        for req in requirements:
            # Use description as the key for finding duplicates
            desc = req.get('description', '').strip()
            if desc:
                duplicates[desc].append(req)
        
        # Filter to only actual duplicates
        return {desc: reqs for desc, reqs in duplicates.items() if len(reqs) > 1}
    
    def get_section_requirements(self, section_id: int) -> List[Dict]:
        """Get requirements for a specific section"""
        try:
            response = self.session.get(
                f"{self.base_url}/api/Requirement/section/{section_id}"
            )
            response.raise_for_status()
            return response.json()
        except requests.RequestException as e:
            print(f"Error fetching requirements for section {section_id}: {e}")
            return []
    
    def update_requirement_section(self, req_id: int, new_section_id: int) -> bool:
        """Update a requirement's section association"""
        try:
            # First get the current requirement
            response = self.session.get(f"{self.base_url}/api/Requirement/{req_id}")
            response.raise_for_status()
            requirement = response.json()
            
            # Update the section ID
            requirement['sectionId'] = new_section_id
            
            # Send the update
            response = self.session.put(
                f"{self.base_url}/api/Requirement/{req_id}",
                json=requirement
            )
            response.raise_for_status()
            self.stats['requirements_updated'] += 1
            return True
            
        except requests.RequestException as e:
            print(f"Error updating requirement {req_id}: {e}")
            return False
    
    def delete_section(self, section_id: int) -> bool:
        """Delete a section"""
        try:
            response = self.session.delete(f"{self.base_url}/api/DocumentSections/{section_id}")
            response.raise_for_status()
            self.stats['sections_removed'] += 1
            return True
        except requests.RequestException as e:
            print(f"Error deleting section {section_id}: {e}")
            return False
    
    def delete_requirement(self, req_id: int) -> bool:
        """Delete a requirement"""
        try:
            response = self.session.delete(f"{self.base_url}/api/Requirement/{req_id}")
            response.raise_for_status()
            self.stats['requirements_removed'] += 1
            return True
        except requests.RequestException as e:
            print(f"Error deleting requirement {req_id}: {e}")
            return False
    
    def consolidate_section_duplicates(self, duplicate_sections: Dict[str, List[Dict]]) -> Dict[int, int]:
        """
        Consolidate duplicate sections by keeping the first and removing others.
        Returns mapping of old_section_id -> new_section_id for requirement updates.
        """
        section_mapping = {}
        
        for title, sections in duplicate_sections.items():
            if len(sections) <= 1:
                continue
                
            print(f"\n📁 Processing duplicate sections for '{title}' ({len(sections)} instances)")
            
            # Sort by ID to keep the first created (lowest ID)
            sections.sort(key=lambda x: x['id'])
            primary_section = sections[0]
            duplicate_sections_to_remove = sections[1:]
            
            print(f"   ✅ Keeping primary section ID: {primary_section['id']}")
            
            # Process each duplicate section
            for dup_section in duplicate_sections_to_remove:
                dup_id = dup_section['id']
                print(f"   🔄 Processing duplicate section ID: {dup_id}")
                
                # Get requirements from the duplicate section
                dup_requirements = self.get_section_requirements(dup_id)
                
                # Move requirements to primary section
                for req in dup_requirements:
                    req_id = req['id']
                    if self.update_requirement_section(req_id, primary_section['id']):
                        print(f"      ➡️  Moved requirement {req_id} to primary section")
                    else:
                        print(f"      ❌ Failed to move requirement {req_id}")
                
                # Map this section to the primary section for any other references
                section_mapping[dup_id] = primary_section['id']
                
                # Delete the duplicate section (after moving its requirements)
                if self.delete_section(dup_id):
                    print(f"   🗑️  Deleted duplicate section ID: {dup_id}")
                else:
                    print(f"   ❌ Failed to delete section ID: {dup_id}")
                
                # Small delay to avoid overwhelming the API
                time.sleep(0.1)
        
        return section_mapping
    
    def consolidate_requirement_duplicates(self, duplicate_requirements: Dict[str, List[Dict]]):
        """
        Consolidate duplicate requirements by keeping the first and removing others.
        """
        for description, requirements in duplicate_requirements.items():
            if len(requirements) <= 1:
                continue
                
            print(f"\n📄 Processing duplicate requirements for '{description[:50]}...' ({len(requirements)} instances)")
            
            # Sort by ID to keep the first created (lowest ID)
            requirements.sort(key=lambda x: x['id'])
            primary_req = requirements[0]
            duplicate_reqs_to_remove = requirements[1:]
            
            print(f"   ✅ Keeping primary requirement ID: {primary_req['id']}")
            
            # Remove duplicate requirements
            for dup_req in duplicate_reqs_to_remove:
                dup_id = dup_req['id']
                if self.delete_requirement(dup_id):
                    print(f"   🗑️  Deleted duplicate requirement ID: {dup_id}")
                else:
                    print(f"   ❌ Failed to delete requirement ID: {dup_id}")
                
                # Small delay to avoid overwhelming the API
                time.sleep(0.1)
    
    def run_cleanup(self, dry_run: bool = False):
        """Run the complete cleanup process"""
        print("🚀 Starting Document Duplicate Cleanup")
        print(f"📋 Target Document ID: {self.document_id}")
        print(f"🔧 Dry Run Mode: {dry_run}")
        print("-" * 60)
        
        if dry_run:
            print("⚠️  DRY RUN MODE - No changes will be made")
            print("-" * 60)
        
        # Step 1: Get all sections and requirements
        print("📊 Step 1: Fetching document data...")
        sections = self.get_document_sections()
        requirements = self.get_document_requirements()
        
        print(f"   📁 Found {len(sections)} sections")
        print(f"   📄 Found {len(requirements)} requirements")
        
        # Step 2: Identify duplicates
        print("\n🔍 Step 2: Identifying duplicates...")
        duplicate_sections = self.find_duplicate_sections(sections)
        duplicate_requirements = self.find_duplicate_requirements(requirements)
        
        print(f"   📁 Found {len(duplicate_sections)} section groups with duplicates")
        print(f"   📄 Found {len(duplicate_requirements)} requirement groups with duplicates")
        
        # Show duplicate statistics
        total_duplicate_sections = sum(len(secs) - 1 for secs in duplicate_sections.values())
        total_duplicate_requirements = sum(len(reqs) - 1 for reqs in duplicate_requirements.values())
        
        print(f"   📁 Total duplicate sections to remove: {total_duplicate_sections}")
        print(f"   📄 Total duplicate requirements to remove: {total_duplicate_requirements}")
        
        if dry_run:
            print("\n📋 DRY RUN SUMMARY:")
            print("   Duplicate Sections:")
            for title, secs in duplicate_sections.items():
                if len(secs) > 1:
                    ids = [str(s['id']) for s in secs]
                    print(f"     '{title}': {len(secs)} instances (IDs: {', '.join(ids)})")
            
            print("   Duplicate Requirements:")
            for desc, reqs in duplicate_requirements.items():
                if len(reqs) > 1:
                    ids = [str(r['id']) for r in reqs]
                    print(f"     '{desc[:50]}...': {len(reqs)} instances (IDs: {', '.join(ids)})")
            return
        
        # Step 3: Consolidate sections (this will move requirements)
        print("\n🔧 Step 3: Consolidating duplicate sections...")
        section_mapping = self.consolidate_section_duplicates(duplicate_sections)
        
        # Step 4: Consolidate requirements
        print("\n🔧 Step 4: Consolidating duplicate requirements...")
        self.consolidate_requirement_duplicates(duplicate_requirements)
        
        # Step 5: Final statistics
        print("\n📈 Cleanup Complete!")
        print("-" * 60)
        print(f"📁 Sections removed: {self.stats['sections_removed']}")
        print(f"📄 Requirements removed: {self.stats['requirements_removed']}")
        print(f"📁 Sections updated: {self.stats['sections_updated']}")
        print(f"📄 Requirements updated: {self.stats['requirements_updated']}")
        
        # Calculate final counts
        final_sections = len(sections) - self.stats['sections_removed']
        final_requirements = len(requirements) - self.stats['requirements_removed']
        
        print(f"\n📊 Final Document Statistics:")
        print(f"   📁 Sections: {len(sections)} → {final_sections} (-{self.stats['sections_removed']})")
        print(f"   📄 Requirements: {len(requirements)} → {final_requirements} (-{self.stats['requirements_removed']})")
        
        expected_sections = 67  # From original document
        expected_requirements = 176  # From original document
        
        print(f"\n🎯 Target vs Actual:")
        print(f"   📁 Sections: Target {expected_sections}, Actual {final_sections}")
        print(f"   📄 Requirements: Target {expected_requirements}, Actual {final_requirements}")
        
        if final_sections == expected_sections and final_requirements == expected_requirements:
            print("✅ SUCCESS: Document restored to expected structure!")
        else:
            print("⚠️  Document structure differs from expected. Manual review may be needed.")


def main():
    import argparse
    
    parser = argparse.ArgumentParser(description='Clean up duplicate sections and requirements')
    parser.add_argument('--dry-run', action='store_true', 
                       help='Show what would be done without making changes')
    parser.add_argument('--token', type=str,
                       help='Authentication token for API access')
    parser.add_argument('--base-url', type=str, default='https://rqmtmgmt.local',
                       help='Base URL for the API')
    
    args = parser.parse_args()
    
    # Create cleanup instance
    cleanup = DocumentCleanup(base_url=args.base_url)
    
    # Setup authentication if token provided
    if args.token:
        cleanup.setup_auth(args.token)
    
    # Run cleanup
    try:
        cleanup.run_cleanup(dry_run=args.dry_run)
    except KeyboardInterrupt:
        print("\n⚠️  Cleanup interrupted by user")
        sys.exit(1)
    except Exception as e:
        print(f"\n❌ Cleanup failed: {e}")
        sys.exit(1)


if __name__ == '__main__':
    main()