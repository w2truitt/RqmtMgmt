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


def parse_requirements_from_srs(file_path: str) -> Dict[str, List[Dict[str, str]]]:
    """
    Parse requirements from the SRS markdown file.
    
    Returns a dictionary mapping section names to lists of requirements.
    Each requirement is a dict with 'id', 'title', and 'description'.
    """
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    requirements_by_section = {}
    current_section = None
    
    # Pattern to match requirement lines like:
    # - **REQ-AUTH-001**: The system SHALL support JWT bearer token authentication
    req_pattern = re.compile(r'^-\s+\*\*([A-Z]+-[A-Z]+-\d+)\*\*:\s+(.+)$', re.MULTILINE)
    
    # Pattern to match section headers
    section_pattern = re.compile(r'^###\s+(\d+\.\d+)\s+(.+)$', re.MULTILINE)
    
    lines = content.split('\n')
    i = 0
    
    while i < len(lines):
        line = lines[i].strip()
        
        # Check for section header
        section_match = re.match(r'^###\s+(\d+\.\d+)\s+(.+)$', line)
        if section_match:
            section_num = section_match.group(1)
            section_name = section_match.group(2)
            current_section = f"{section_num} {section_name}"
            if current_section not in requirements_by_section:
                requirements_by_section[current_section] = []
            i += 1
            continue
        
        # Check for requirement
        req_match = re.match(r'^-\s+\*\*([A-Z]+-[A-Z]+-\d+)\*\*:\s+(.+)$', line)
        if req_match and current_section:
            req_id = req_match.group(1)
            req_description = req_match.group(2)
            
            # Create requirement object
            requirement = {
                'id': req_id,
                'title': f"{req_id}: {req_description[:100]}",  # Truncate title if too long
                'description': req_description
            }
            
            requirements_by_section[current_section].append(requirement)
        
        i += 1
    
    return requirements_by_section


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
    sections = get_document_sections(token, DOCUMENT_ID)
    
    if not sections:
        print("⚠ Warning: Could not fetch existing sections")
        print("   Proceeding anyway - sections will need to be created")
    else:
        print(f"✓ Found {len(sections)} existing sections")
        for section in sections:
            print(f"  - {section.get('number')}. {section.get('title')} (ID: {section.get('id')})")
    print()
    
    # Build section mapping
    section_mapping = {}
    if sections:
        for section in sections:
            section_title = f"{section.get('number')}. {section.get('title')}"
            section_mapping[section_title] = section.get('id')
    
    # Parse requirements from SRS file
    print("Parsing requirements from SOFTWARE_REQUIREMENTS_SPECIFICATION.md...")
    srs_file = "SOFTWARE_REQUIREMENTS_SPECIFICATION.md"
    requirements_by_section = parse_requirements_from_srs(srs_file)
    
    total_requirements = sum(len(reqs) for reqs in requirements_by_section.values())
    print(f"✓ Found {total_requirements} requirements across {len(requirements_by_section)} sections")
    print()
    
    # Display summary
    for section, reqs in requirements_by_section.items():
        if len(reqs) > 0:
            print(f"  {section}: {len(reqs)} requirements")
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
    
    for section_name, reqs in requirements_by_section.items():
        if len(reqs) == 0:
            skipped_count += len(reqs) if reqs else 0
            continue
            
        # Try to find section ID
        section_id = section_mapping.get(section_name)
        
        # Try alternative formatting
        if not section_id:
            # Remove leading numbers if present
            alt_name = re.sub(r'^\d+\.\d+\s+', '', section_name)
            for key in section_mapping.keys():
                if alt_name in key or section_name in key:
                    section_id = section_mapping[key]
                    break
        
        if not section_id:
            print(f"⚠ Warning: No section found for '{section_name}', skipping {len(reqs)} requirements...")
            skipped_count += len(reqs)
            continue
        
        print(f"\nSection: {section_name} (ID: {section_id})")
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
                "version": 1
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
