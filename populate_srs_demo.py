#!/usr/bin/env python3
"""
Simplified demo script to populate the first 3 sections (24 requirements) 
from SOFTWARE_REQUIREMENTS_SPECIFICATION.md into the RqmtMgmt system.

This demonstrates the API integration for real-life testing.
"""

import json
import re
import requests
import sys
import urllib3

# Disable SSL warnings
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# Configuration
API_BASE_URL = "https://rqmtmgmt.local/api"
PROJECT_ID = 25
DOCUMENT_ID = 11

# Manual section mapping based on the existing document structure
SECTION_MAPPING = {
    "3.1 Authentication and Authorization": 1,
    "3.2 User Management": 2,
    "3.3 API Design and RESTful Services": 3
}

def parse_requirements_from_srs(file_path: str):
    """Parse requirements for the 3 existing sections only."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    requirements_by_section = {
        "3.1 Authentication and Authorization": [],
        "3.2 User Management": [],
        "3.3 API Design and RESTful Services": []
    }
    
    lines = content.split('\n')
    current_section = None
    
    for i, line in enumerate(lines):
        line = line.strip()
        
        # Check for section headers (### 3.1, ### 3.2, ### 3.3)
        if line.startswith('###') and ('3.1' in line or '3.2' in line or '3.3' in line):
            if '3.1' in line and 'Authentication and Authorization' in line:
                current_section = "3.1 Authentication and Authorization"
            elif '3.2' in line and 'User Management' in line:
                current_section = "3.2 User Management"
            elif '3.3' in line and 'API Design' in line:
                current_section = "3.3 API Design and RESTful Services"
            continue
        
        # Check for requirement
        req_match = re.match(r'^-\s+\*\*([A-Z]+-[A-Z]+-\d+)\*\*:\s+(.+)$', line)
        if req_match and current_section and current_section in requirements_by_section:
            req_id = req_match.group(1)
            req_description = req_match.group(2)
            
            requirement = {
                'id': req_id,
                'title': f"{req_id}: {req_description[:100]}",
                'description': req_description
            }
            
            requirements_by_section[current_section].append(requirement)
    
    return requirements_by_section


def create_requirement(token: str, requirement: dict):
    """Create a single requirement via API."""
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    url = f"{API_BASE_URL}/Requirement"
    
    try:
        response = requests.post(url, headers=headers, json=requirement, verify=False)
        
        if response.status_code in [200, 201]:
            print(f"  ✓ {requirement['title']}")
            return response.json()
        else:
            print(f"  ✗ Failed: {requirement['title']} ({response.status_code})")
            if response.status_code == 401:
                print(f"     ERROR: Token expired or invalid!")
                print(f"     Please get a fresh token from the browser")
                sys.exit(1)
            return None
    except Exception as e:
        print(f"  ✗ Error: {str(e)}")
        return None


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 populate_srs_demo.py <access_token>")
        print()
        print("Get token from browser session storage:")
        print("  Dev Tools -> Application -> Session Storage")
        print("  -> Find 'oidc.user:https://rqmtmgmt.local:rqmtmgmt-frontend'")
        print("  -> Copy the 'access_token' value")
        return 1
    
    token = sys.argv[1]
    
    print("="  * 70)
    print("RqmtMgmt Requirements Population - Demo Script")
    print("=" * 70)
    print()
    
    # Parse requirements
    print("Parsing SRS document...")
    requirements_by_section = parse_requirements_from_srs("SOFTWARE_REQUIREMENTS_SPECIFICATION.md")
    
    total = sum(len(reqs) for reqs in requirements_by_section.values())
    print(f"Found {total} requirements across 3 sections:")
    for section, reqs in requirements_by_section.items():
        print(f"  - {section}: {len(reqs)} requirements")
    print()
    
    # Create requirements
    print("Creating requirements...")
    print("-" * 70)
    
    created = 0
    failed = 0
    
    for section, reqs in requirements_by_section.items():
        section_id = SECTION_MAPPING[section]
        print(f"\n[{section}] (Section ID: {section_id})")
        
        for req in reqs:
            req_dto = {
                "title": req['title'][:200],
                "description": req['description'],
                "type": 2,  # SRS
                "status": 0,  # Draft
                "projectId": PROJECT_ID,
                "documentId": DOCUMENT_ID,
                "sectionId": section_id,
                "version": 1
            }
            
            result = create_requirement(token, req_dto)
            if result:
                created += 1
            else:
                failed += 1
    
    print()
    print("=" * 70)
    print(f"COMPLETED: {created} created, {failed} failed")
    print("=" * 70)
    
    return 0 if failed == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
