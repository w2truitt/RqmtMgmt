#!/usr/bin/env python3
"""
Script to populate requirements from SOFTWARE_REQUIREMENTS_SPECIFICATION.md
into the RqmtMgmt system via REST API.

This script:
1. Authenticates with IdentityServer
2. Reads the SOFTWARE_REQUIREMENTS_SPECIFICATION.md file
3. Parses all requirements (REQ-*)
4. Creates document sections if needed
5. Creates requirements via REST API
6. Associates them with the appropriate document sections
"""

import json
import re
import requests
import sys
from typing import List, Dict, Optional
from datetime import datetime
import urllib3

# Disable SSL warnings for local development
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# Configuration
API_BASE_URL = "https://rqmtmgmt.local/api"
IDENTITY_BASE_URL = "https://rqmtmgmt.local"
PROJECT_ID = 25
DOCUMENT_ID = 11

# Admin credentials for authentication
USERNAME = "admin@rqmtmgmt.local"
PASSWORD = "Admin123!"

# Requirement type enum mapping
REQUIREMENT_TYPES = {
    "CRD": 0,
    "PRD": 1,
    "SRS": 2,
    "UserStory": 3,
    "BusinessRule": 4,
    "EntityName": 5
}

# Requirement status enum mapping
REQUIREMENT_STATUS = {
    "Draft": 0,
    "Approved": 1,
    "Implemented": 2,
    "Verified": 3
}


def authenticate() -> Optional[str]:
    """
    Authenticate with IdentityServer using Resource Owner Password flow.
    
    Returns:
        Access token or None if authentication failed
    """
    print("Authenticating with IdentityServer...")
    
    # For IdentityServer4/Duende, we need to use the token endpoint
    # This is a simplified version - in production you'd use proper OAuth2 client library
    token_url = f"{IDENTITY_BASE_URL}/connect/token"
    
    data = {
        "client_id": "rqmtmgmt-api-client",  # Need to check what client to use
        "grant_type": "client_credentials",
        "scope": "rqmtmgmt.api"
    }
    
    try:
        response = requests.post(token_url, data=data, verify=False)
        if response.status_code == 200:
            token_data = response.json()
            return token_data.get("access_token")
        else:
            print(f"Authentication failed: {response.status_code}")
            print(f"Response: {response.text}")
            return None
    except Exception as e:
        print(f"Error during authentication: {str(e)}")
        return None


def get_document_sections(token: str, document_id: int) -> List[Dict]:
    """
    Get all sections for a document.
    
    Args:
        token: JWT bearer token
        document_id: Document ID
        
    Returns:
        List of section objects
    """
    headers = {
        "Authorization": f"Bearer {token}",
        "Accept": "application/json"
    }
    
    url = f"{API_BASE_URL}/DocumentSections/document/{document_id}"
    
    try:
        response = requests.get(url, headers=headers, verify=False)
        if response.status_code == 200:
            return response.json()
        else:
            print(f"Failed to get sections: {response.status_code}")
            return []
    except Exception as e:
        print(f"Error getting sections: {str(e)}")
        return []


def create_document_section(token: str, document_id: int, section_data: Dict) -> Optional[Dict]:
    """
    Create a new document section.
    
    Args:
        token: JWT bearer token
        document_id: Document ID
        section_data: Section data
        
    Returns:
        Created section or None
    """
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json",
        "Accept": "application/json"
    }
    
    url = f"{API_BASE_URL}/DocumentSections"
    
    try:
        response = requests.post(url, headers=headers, json=section_data, verify=False)
        if response.status_code in [200, 201]:
            return response.json()
        else:
            print(f"Failed to create section: {response.status_code}")
            print(f"Response: {response.text}")
            return None
    except Exception as e:
        print(f"Error creating section: {str(e)}")
        return None


def parse_sections_and_requirements_from_srs(file_path: str) -> tuple[Dict[str, Dict], Dict[str, List[Dict[str, str]]]]:
    """
    Parse sections and requirements from the SRS markdown file.
    
    Returns:
        Tuple of (sections_dict, requirements_by_section)
        sections_dict maps section numbers to section data
        requirements_by_section maps section numbers to lists of requirements
    """
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    sections = {}
    requirements_by_section = {}
    
    lines = content.split('\n')
    current_section = None
    current_parent = None
    
    for line in lines:
        line = line.strip()
        
        # Check for main section header (### 3.1 Title)
        main_match = re.match(r'^###\s+(\d+\.\d+)\s+(.+)$', line)
        if main_match:
            section_num = main_match.group(1)
            section_name = main_match.group(2)
            current_section = section_num
            current_parent = None
            sections[section_num] = {
                'number': section_num,
                'title': section_name,
                'level': 1,
                'parent': None
            }
            requirements_by_section[section_num] = []
            continue
        
        # Check for subsection header (#### 3.1.1 Title)
        sub_match = re.match(r'^####\s+(\d+\.\d+\.\d+)\s+(.+)$', line)
        if sub_match:
            section_num = sub_match.group(1)
            section_name = sub_match.group(2)
            current_section = section_num
            # Find parent (e.g., 3.1 for 3.1.1)
            parent_num = '.'.join(section_num.split('.')[:-1])
            current_parent = parent_num
            sections[section_num] = {
                'number': section_num,
                'title': section_name,
                'level': 2,
                'parent': parent_num
            }
            requirements_by_section[section_num] = []
            continue
        
        # Check for requirement
        req_match = re.match(r'^-\s+\*\*([A-Z]+-[A-Z]+-\d+)\*\*:\s+(.+)$', line)
        if req_match and current_section:
            req_id = req_match.group(1)
            req_description = req_match.group(2)
            
            requirement = {
                'id': req_id,
                'title': f"{req_id}: {req_description[:100]}",
                'description': req_description
            }
            
            requirements_by_section[current_section].append(requirement)
    
    return sections, requirements_by_section


def create_requirement(token: str, requirement: Dict[str, any]) -> Optional[Dict]:
    """
    Create a requirement via the REST API.
    
    Args:
        token: JWT bearer token for authentication
        requirement: Dictionary containing requirement data
        
    Returns:
        Created requirement data or None if failed
    """
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json",
        "Accept": "application/json"
    }
    
    url = f"{API_BASE_URL}/Requirement"
    
    try:
        response = requests.post(
            url, 
            headers=headers, 
            json=requirement, 
            verify=False  # Skip SSL verification for local dev
        )
        
        if response.status_code in [200, 201]:
            print(f"✓ Created: {requirement['title']}")
            return response.json()
        else:
            print(f"✗ Failed to create {requirement['title']}: {response.status_code}")
            print(f"  Response: {response.text}")
            return None
            
    except Exception as e:
        print(f"✗ Error creating {requirement['title']}: {str(e)}")
        return None


def ensure_sections_exist(token: str, document_id: int, sections: Dict[str, Dict], existing_sections: List[Dict]) -> Dict[str, int]:
    """
    Ensure all sections exist in the document, creating them if necessary.
    
    Args:
        token: JWT bearer token
        document_id: Document ID
        sections: Dictionary of sections to create
        existing_sections: List of existing sections
        
    Returns:
        Mapping of section numbers to section IDs
    """
    # Build mapping of existing sections
    existing_mapping = {}
    for section in existing_sections:
        section_num = section.get('number')
        if section_num:
            existing_mapping[section_num] = section['id']
    
    # Sort sections by level and number to create parents first
    sorted_sections = sorted(sections.items(), key=lambda x: (x[1]['level'], x[0]))
    
    section_ids = existing_mapping.copy()
    
    for section_num, section_data in sorted_sections:
        if section_num in section_ids:
            continue  # Already exists
        
        # Create the section
        parent_id = None
        if section_data['parent']:
            parent_id = section_ids.get(section_data['parent'])
            if not parent_id:
                print(f"⚠ Warning: Parent section {section_data['parent']} not found for {section_num}")
        
        section_dto = {
            "documentId": document_id,
            "number": section_num,
            "title": section_data['title'],
            "description": "",  # Can be updated later
            "parentId": parent_id,
            "order": 0  # Can be updated later
        }
        
        print(f"Creating section: {section_num} {section_data['title']}")
        created_section = create_document_section(token, document_id, section_dto)
        if created_section:
            section_ids[section_num] = created_section['id']
        else:
            print(f"✗ Failed to create section {section_num}")
    
    return section_ids


def main():
    """Main execution function."""
    print("=" * 80)
    print("RqmtMgmt Requirements Population Script")
    print("=" * 80)
    print()
    
    # Check if token is provided as argument
    token = None
    if len(sys.argv) > 1:
        token = sys.argv[1]
        print("Using provided access token")
    else:
        print("ERROR: Please provide an access token as the first argument")
        print("Usage: python3 populate_srs_requirements.py <access_token>")
        print()
        print("You can get a token from the browser's session storage:")
        print("  Open browser dev tools -> Application -> Session Storage")
        print("  -> https://rqmtmgmt.local")
        print("  -> Find 'oidc.user:https://rqmtmgmt.local:rqmtmgmt-frontend'")
        print("  -> Copy the 'access_token' value")
        return 1
    
    print()
    
    # Get document sections
    print("Fetching document sections...")
    existing_sections = get_document_sections(token, DOCUMENT_ID)
    
    if not existing_sections:
        print("⚠ Warning: Could not fetch existing sections")
        print("   Proceeding anyway - sections will need to be created")
    else:
        print(f"✓ Found {len(existing_sections)} existing sections")
        for section in existing_sections:
            print(f"  - {section.get('number')}. {section.get('title')} (ID: {section.get('id')})")
    print()
    
    # Parse sections and requirements from SRS file
    print("Parsing sections and requirements from SOFTWARE_REQUIREMENTS_SPECIFICATION.md...")
    srs_file = "SOFTWARE_REQUIREMENTS_SPECIFICATION.md"
    sections, requirements_by_section = parse_sections_and_requirements_from_srs(srs_file)
    
    total_requirements = sum(len(reqs) for reqs in requirements_by_section.values())
    print(f"✓ Found {len(sections)} sections and {total_requirements} requirements")
    print()
    
    # Display sections
    for section_num, section_data in sections.items():
        level = "  " * (section_data['level'] - 1)
        print(f"{level}{section_num} {section_data['title']}")
    print()
    
    # Display requirements summary
    for section_num, reqs in requirements_by_section.items():
        if len(reqs) > 0:
            section_data = sections[section_num]
            level = "  " * (section_data['level'] - 1)
            print(f"{level}{section_num} {section_data['title']}: {len(reqs)} requirements")
    print()
    
    # Ensure all sections exist
    print("Ensuring all sections exist...")
    section_ids = ensure_sections_exist(token, DOCUMENT_ID, sections, existing_sections)
    print(f"✓ Section mapping complete: {len(section_ids)} sections")
    print()
    
    # Ask for confirmation
    response = input("Do you want to create these requirements? (yes/no): ")
    if response.lower() not in ['yes', 'y']:
        print("Aborted by user")
        return 0
    print()
    
    # Create requirements
    print("Creating requirements via API...")
    print("-" * 80)
    
    created_count = 0
    failed_count = 0
    skipped_count = 0
    
    for section_num, reqs in requirements_by_section.items():
        if len(reqs) == 0:
            continue
            
        # Get section ID
        section_id = section_ids.get(section_num)
        
        if not section_id:
            print(f"⚠ Warning: No section ID found for '{section_num}', skipping {len(reqs)} requirements...")
            skipped_count += len(reqs)
            continue
        
        section_data = sections[section_num]
        level = "  " * (section_data['level'] - 1)
        print(f"\n{level}Section: {section_num} {section_data['title']} (ID: {section_id})")
        print("-" * 40)
        
        for req in reqs:
            # Build requirement DTO
            req_dto = {
                "title": req['title'][:200],  # Limit title length
                "description": req['description'],
                "type": REQUIREMENT_TYPES["SRS"],  # 2 for SRS
                "status": REQUIREMENT_STATUS["Draft"],  # 0 for Draft
                "projectId": PROJECT_ID,
                "documentId": DOCUMENT_ID,
                "sectionId": section_id,
                "version": 1,
                "createdBy": 1  # Required field
            }
            
            result = create_requirement(token, req_dto)
            
            if result:
                created_count += 1
            else:
                failed_count += 1
    
    # Summary
    print()
    print("=" * 80)
    print("SUMMARY")
    print("=" * 80)
    print(f"Total requirements found: {total_requirements}")
    print(f"Successfully created: {created_count}")
    print(f"Failed: {failed_count}")
    print(f"Skipped (no section): {skipped_count}")
    print()
    
    if failed_count == 0 and skipped_count == 0:
        print("✓ All requirements created successfully!")
        return 0
    elif failed_count == 0:
        print(f"⚠ {skipped_count} requirements skipped (sections not found in document)")
        return 0
    else:
        print(f"⚠ {failed_count} requirements failed to create")
        return 1


if __name__ == "__main__":
    sys.exit(main())
