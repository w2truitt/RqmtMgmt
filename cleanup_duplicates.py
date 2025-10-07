#!/usr/bin/env python3
"""
Document Cleanup Tool
Removes duplicate sections and requirements from Project 25 Document 11
"""

import json
import subprocess
import time
from collections import defaultdict
from typing import Dict, List, Any, Tuple

def get_api_token():
    """Get authentication token for API calls"""
    cmd = [
        'curl', '-k', '-s', '-X', 'POST', 
        'https://rqmtmgmt.local/connect/token',
        '-H', 'Content-Type: application/x-www-form-urlencoded',
        '-H', 'Accept: application/json',
        '-d', 'grant_type=client_credentials&client_id=rqmtmgmt-backend&client_secret=backend-secret&scope=rqmtmgmt.api'
    ]
    
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        raise Exception(f"Failed to get token: {result.stderr}")
    
    token_response = json.loads(result.stdout)
    return token_response['access_token']

def api_call(endpoint: str, token: str, method: str = 'GET', data: str = None) -> Any:
    """Make API call with authentication"""
    cmd = [
        'curl', '-k', '-s',
        '-X', method,
        '-H', f'Authorization: Bearer {token}',
        '-H', 'Accept: application/json'
    ]
    
    if method in ['POST', 'PUT'] and data:
        cmd.extend(['-H', 'Content-Type: application/json', '-d', data])
    
    cmd.append(f'https://rqmtmgmt.local/api/{endpoint}')
    
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        raise Exception(f"API call failed: {result.stderr}")
    
    if result.stdout.strip():
        try:
            return json.loads(result.stdout)
        except json.JSONDecodeError:
            return result.stdout
    return None

def get_document_sections(token: str, document_id: int = 11) -> List[Dict]:
    """Get all sections for the document"""
    return api_call(f'DocumentSections/document/{document_id}/hierarchy', token)

def get_document_requirements(token: str, document_id: int = 11) -> List[Dict]:
    """Get all requirements for the document"""
    return api_call(f'Requirement/document/{document_id}', token)

def identify_duplicate_sections(sections: List[Dict]) -> Dict[str, List[Dict]]:
    """Identify duplicate sections by title and level"""
    duplicates = defaultdict(list)
    
    def process_sections(sections_list, level=0, parent_path=""):
        for section in sections_list:
            title = section['title']
            section_key = f"{title}|{level}"  # Use title + level as key
            
            duplicates[section_key].append({
                'section': section,
                'level': level,
                'parent_path': parent_path
            })
            
            # Process child sections
            if section.get('childSections'):
                current_path = f"{parent_path}/{title}" if parent_path else title
                process_sections(section['childSections'], level + 1, current_path)
    
    process_sections(sections)
    
    # Filter to only return actual duplicates (more than 1 instance)
    return {key: instances for key, instances in duplicates.items() if len(instances) > 1}

def identify_duplicate_requirements(requirements: List[Dict]) -> Dict[str, List[Dict]]:
    """Identify duplicate requirements by description"""
    duplicates = defaultdict(list)
    
    for req in requirements:
        description = req.get('description', '').strip()
        if description:  # Only process requirements with descriptions
            duplicates[description].append(req)
    
    # Filter to only return actual duplicates
    return {desc: reqs for desc, reqs in duplicates.items() if len(reqs) > 1}

def move_requirements_to_primary_section(token: str, requirements: List[Dict], 
                                       primary_section_id: int, sections_to_delete: List[int]) -> List[str]:
    """Move requirements from duplicate sections to primary section"""
    results = []
    
    for req in requirements:
        current_section_id = req.get('sectionId')
        
        # If requirement is in a section that will be deleted, move it to primary
        if current_section_id in sections_to_delete:
            req_id = req['id']
            
            # Update the requirement's section ID
            update_data = {
                'id': req_id,
                'type': req['type'],
                'title': req['title'],
                'description': req['description'],
                'status': req['status'],
                'projectId': req['projectId'],
                'sectionId': primary_section_id
            }
            
            try:
                result = api_call(f'Requirement/{req_id}', token, 'PUT', json.dumps(update_data))
                results.append(f"✅ Moved requirement {req_id} to section {primary_section_id}")
                time.sleep(0.1)  # Rate limiting
            except Exception as e:
                results.append(f"❌ Failed to move requirement {req_id}: {e}")
    
    return results

def delete_duplicate_sections(token: str, duplicate_groups: Dict[str, List[Dict]]) -> Tuple[List[str], List[int]]:
    """Delete duplicate sections, keeping the first instance of each group"""
    results = []
    sections_deleted = []
    
    for section_key, instances in duplicate_groups.items():
        if len(instances) <= 1:
            continue
            
        # Sort by ID to keep the earliest created section
        instances.sort(key=lambda x: x['section']['id'])
        primary = instances[0]
        duplicates = instances[1:]
        
        primary_id = primary['section']['id']
        primary_title = primary['section']['title']
        
        results.append(f"\n🔍 Processing duplicate group: '{primary_title}' (Level {primary['level']})")
        results.append(f"   Primary section (keeping): ID {primary_id}")
        
        # Delete duplicate sections
        for dup in duplicates:
            dup_section = dup['section']
            dup_id = dup_section['id']
            dup_req_count = dup_section.get('requirementCount', 0)
            
            try:
                # First, get requirements in this section and move them to primary
                if dup_req_count > 0:
                    section_requirements = api_call(f'Requirement/section/{dup_id}', token)
                    if section_requirements:
                        move_results = move_requirements_to_primary_section(
                            token, section_requirements, primary_id, [dup_id]
                        )
                        results.extend(move_results)
                
                # Delete the duplicate section
                api_call(f'DocumentSections/{dup_id}', token, 'DELETE')
                results.append(f"   ✅ Deleted duplicate section ID {dup_id} (had {dup_req_count} requirements)")
                sections_deleted.append(dup_id)
                time.sleep(0.2)  # Rate limiting
                
            except Exception as e:
                results.append(f"   ❌ Failed to delete section ID {dup_id}: {e}")
    
    return results, sections_deleted

def delete_duplicate_requirements(token: str, duplicate_groups: Dict[str, List[Dict]]) -> List[str]:
    """Delete duplicate requirements, keeping the first instance of each group"""
    results = []
    
    for description, instances in duplicate_groups.items():
        if len(instances) <= 1:
            continue
            
        # Sort by ID to keep the earliest created requirement
        instances.sort(key=lambda x: x['id'])
        primary = instances[0]
        duplicates = instances[1:]
        
        primary_id = primary['id']
        primary_title = primary.get('title', 'No title')[:50]
        
        results.append(f"\n🔍 Processing duplicate requirement group:")
        results.append(f"   Description: {description[:100]}...")
        results.append(f"   Primary requirement (keeping): ID {primary_id} - {primary_title}")
        
        # Delete duplicate requirements
        for dup in duplicates:
            dup_id = dup['id']
            dup_title = dup.get('title', 'No title')[:50]
            
            try:
                api_call(f'Requirement/{dup_id}', token, 'DELETE')
                results.append(f"   ✅ Deleted duplicate requirement ID {dup_id} - {dup_title}")
                time.sleep(0.1)  # Rate limiting
                
            except Exception as e:
                results.append(f"   ❌ Failed to delete requirement ID {dup_id}: {e}")
    
    return results

def generate_cleanup_report(sections_before: int, requirements_before: int,
                          sections_after: int, requirements_after: int,
                          sections_deleted: List[int], cleanup_results: List[str]) -> str:
    """Generate cleanup summary report"""
    report = []
    report.append("# Document Cleanup Summary Report")
    report.append("## Project 25 Document 11 - Duplicate Removal")
    report.append("")
    
    # Statistics
    report.append("## Cleanup Statistics")
    report.append(f"- **Sections Before**: {sections_before}")
    report.append(f"- **Sections After**: {sections_after}")
    report.append(f"- **Sections Deleted**: {sections_before - sections_after}")
    report.append(f"- **Requirements Before**: {requirements_before}")
    report.append(f"- **Requirements After**: {requirements_after}")
    report.append(f"- **Requirements Deleted**: {requirements_before - requirements_after}")
    report.append("")
    
    # Deleted sections
    if sections_deleted:
        report.append("## Deleted Section IDs")
        for section_id in sections_deleted:
            report.append(f"- Section ID: {section_id}")
        report.append("")
    
    # Detailed results
    report.append("## Detailed Cleanup Log")
    for result in cleanup_results:
        report.append(result)
    report.append("")
    
    return "\n".join(report)

def main():
    """Main cleanup function"""
    print("🧹 Starting document cleanup process...")
    print("⚠️  This will delete duplicate sections and requirements!")
    
    # Confirmation
    confirm = input("Are you sure you want to proceed? (yes/no): ").lower().strip()
    if confirm != 'yes':
        print("❌ Cleanup cancelled by user")
        return 1
    
    try:
        # Get API token
        print("🔐 Getting API token...")
        token = get_api_token()
        
        # Get initial state
        print("📊 Analyzing current document state...")
        initial_sections = get_document_sections(token)
        initial_requirements = get_document_requirements(token)
        
        sections_before = len([s for s in flatten_sections(initial_sections)])
        requirements_before = len(initial_requirements)
        
        print(f"   Initial state: {sections_before} sections, {requirements_before} requirements")
        
        # Identify duplicates
        print("🔍 Identifying duplicate sections...")
        duplicate_sections = identify_duplicate_sections(initial_sections)
        print(f"   Found {len(duplicate_sections)} duplicate section groups")
        
        print("🔍 Identifying duplicate requirements...")
        duplicate_requirements = identify_duplicate_requirements(initial_requirements)
        print(f"   Found {len(duplicate_requirements)} duplicate requirement groups")
        
        # Cleanup process
        cleanup_results = []
        sections_deleted = []
        
        if duplicate_sections:
            print("🗑️  Removing duplicate sections...")
            section_results, deleted_ids = delete_duplicate_sections(token, duplicate_sections)
            cleanup_results.extend(section_results)
            sections_deleted.extend(deleted_ids)
            print(f"   Deleted {len(deleted_ids)} duplicate sections")
        
        if duplicate_requirements:
            print("🗑️  Removing duplicate requirements...")
            req_results = delete_duplicate_requirements(token, duplicate_requirements)
            cleanup_results.extend(req_results)
            print(f"   Processed {len(duplicate_requirements)} duplicate requirement groups")
        
        # Get final state
        print("📊 Analyzing final document state...")
        final_sections = get_document_sections(token)
        final_requirements = get_document_requirements(token)
        
        sections_after = len([s for s in flatten_sections(final_sections)])
        requirements_after = len(final_requirements)
        
        print(f"   Final state: {sections_after} sections, {requirements_after} requirements")
        
        # Generate report
        print("📝 Generating cleanup report...")
        report = generate_cleanup_report(
            sections_before, requirements_before,
            sections_after, requirements_after,
            sections_deleted, cleanup_results
        )
        
        # Save report
        with open('/home/wtruitt/src/repos/RqmtMgmt/CLEANUP_REPORT.md', 'w') as f:
            f.write(report)
        
        print("✅ Cleanup complete!")
        print(f"   Sections: {sections_before} → {sections_after} (-{sections_before - sections_after})")
        print(f"   Requirements: {requirements_before} → {requirements_after} (-{requirements_before - requirements_after})")
        print("📄 Detailed report saved to CLEANUP_REPORT.md")
        
        return 0
        
    except Exception as e:
        print(f"❌ Error during cleanup: {e}")
        import traceback
        traceback.print_exc()
        return 1

def flatten_sections(sections):
    """Flatten hierarchical sections into a list"""
    flat = []
    for section in sections:
        flat.append(section)
        if section.get('childSections'):
            flat.extend(flatten_sections(section['childSections']))
    return flat

if __name__ == "__main__":
    exit(main())