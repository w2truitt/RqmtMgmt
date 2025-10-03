#!/usr/bin/env python3
"""
Enhanced script to populate the SOFTWARE_REQUIREMENTS_SPECIFICATION.md
into the RqmtMgmt system with full hierarchical section support.

This script:
1. Authenticates with IdentityServer or uses provided token
2. Parses the SRS markdown to extract hierarchical sections (##, ###, ####)
3. Creates sections in proper parent-child relationships
4. Extracts and creates requirements under their respective sections
5. Handles subsections properly (Level 1, 2, 3, etc.)

The system already supports hierarchical sections via ParentSectionId.
"""

import json
import re
import requests
import sys
from typing import List, Dict, Optional, Tuple
from dataclasses import dataclass
from datetime import datetime
import urllib3

# Disable SSL warnings for local development
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# Configuration
API_BASE_URL = "https://rqmtmgmt.local/api"
IDENTITY_BASE_URL = "https://rqmtmgmt.local"
PROJECT_ID = 25
DOCUMENT_ID = 11  # The SRS document ID

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


@dataclass
class Section:
    """Represents a document section with hierarchical information."""
    number: str  # e.g., "3.1.2"
    title: str
    description: Optional[str]
    level: int  # 1, 2, 3, etc.
    order: int  # Order within parent
    parent_number: Optional[str]  # Parent section number, e.g., "3.1" for "3.1.2"
    requirements: List[Dict[str, str]]  # List of requirements in this section
    
    @property
    def full_title(self) -> str:
        """Get the full title with number."""
        return f"{self.number} {self.title}"


def parse_section_number(number_str: str) -> Tuple[int, Optional[str], int]:
    """
    Parse section number to determine level, parent, and order.
    
    Args:
        number_str: Section number like "3.1.2"
        
    Returns:
        Tuple of (level, parent_number, order)
        - level: 1 for "3", 2 for "3.1", 3 for "3.1.2"
        - parent_number: "3.1" for "3.1.2", "3" for "3.1", None for "3"
        - order: The last number in the sequence
    """
    parts = number_str.split('.')
    level = len(parts)
    
    if level == 1:
        parent_number = None
        order = int(parts[0])
    else:
        parent_number = '.'.join(parts[:-1])
        order = int(parts[-1])
    
    return level, parent_number, order


def parse_srs_markdown(file_path: str) -> List[Section]:
    """
    Parse the SRS markdown file to extract hierarchical sections and requirements.
    
    Handles:
    - ## 3. Functional Requirements (Level 1)
    - ### 3.1 Authentication and Authorization (Level 2)
    - #### 3.1.1 User Authentication (Level 3)
    - Requirements: - **REQ-XXX-NNN**: Description
    
    Returns:
        List of Section objects in document order
    """
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    sections = []
    current_section = None
    
    # Patterns
    section_l1_pattern = re.compile(r'^##\s+(\d+)\.\s+(.+)$')  # ## 3. Title
    section_l2_pattern = re.compile(r'^###\s+(\d+\.\d+)\s+(.+)$')  # ### 3.1 Title
    section_l3_pattern = re.compile(r'^####\s+(\d+\.\d+\.\d+)\s+(.+)$')  # #### 3.1.1 Title
    requirement_pattern = re.compile(r'^-\s+\*\*([A-Z]+-[A-Z]+-\d+)\*\*:\s+(.+)$')
    
    for i, line in enumerate(lines):
        line = line.rstrip()
        
        # Check for level 1 section
        match = section_l1_pattern.match(line)
        if match:
            number = match.group(1)
            title = match.group(2)
            level, parent_num, order = parse_section_number(number)
            
            # Save previous section if exists
            if current_section:
                sections.append(current_section)
            
            current_section = Section(
                number=number,
                title=title,
                description=None,
                level=level,
                order=order,
                parent_number=parent_num,
                requirements=[]
            )
            continue
        
        # Check for level 2 section
        match = section_l2_pattern.match(line)
        if match:
            number = match.group(1)
            title = match.group(2)
            level, parent_num, order = parse_section_number(number)
            
            # Save previous section if exists
            if current_section:
                sections.append(current_section)
            
            current_section = Section(
                number=number,
                title=title,
                description=None,
                level=level,
                order=order,
                parent_number=parent_num,
                requirements=[]
            )
            continue
        
        # Check for level 3 section
        match = section_l3_pattern.match(line)
        if match:
            number = match.group(1)
            title = match.group(2)
            level, parent_num, order = parse_section_number(number)
            
            # Save previous section if exists
            if current_section:
                sections.append(current_section)
            
            current_section = Section(
                number=number,
                title=title,
                description=None,
                level=level,
                order=order,
                parent_number=parent_num,
                requirements=[]
            )
            continue
        
        # Check for requirement
        match = requirement_pattern.match(line)
        if match and current_section:
            req_id = match.group(1)
            req_description = match.group(2)
            
            current_section.requirements.append({
                'id': req_id,
                'title': f"{req_id}",
                'description': req_description
            })
            continue
    
    # Don't forget the last section
    if current_section:
        sections.append(current_section)
    
    return sections


def create_section_via_api(token: str, section_data: Dict) -> Optional[Dict]:
    """
    Create a document section via the REST API.
    
    Args:
        token: JWT bearer token
        section_data: Section data (must include either documentId or parentSectionId)
        
    Returns:
        Created section data or None if failed
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
            print(f"  ✗ Failed to create section: {response.status_code}")
            print(f"    Response: {response.text}")
            return None
    except Exception as e:
        print(f"  ✗ Error creating section: {str(e)}")
        return None


def create_requirement_via_api(token: str, requirement_data: Dict) -> Optional[Dict]:
    """
    Create a requirement via the REST API.
    
    Args:
        token: JWT bearer token
        requirement_data: Requirement data
        
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
        response = requests.post(url, headers=headers, json=requirement_data, verify=False)
        if response.status_code in [200, 201]:
            return response.json()
        else:
            print(f"    ✗ Failed to create requirement: {response.status_code}")
            return None
    except Exception as e:
        print(f"    ✗ Error creating requirement: {str(e)}")
        return None


def main():
    """Main execution function."""
    print("=" * 80)
    print("RqmtMgmt - Hierarchical SRS Population Script")
    print("=" * 80)
    print()
    
    # Check for access token
    if len(sys.argv) < 2:
        print("ERROR: Please provide an access token as the first argument")
        print("Usage: python3 populate_srs_hierarchical.py <access_token>")
        print()
        print("To get a token:")
        print("  1. Open https://rqmtmgmt.local in your browser")
        print("  2. Open browser dev tools (F12) -> Application tab")
        print("  3. Session Storage -> https://rqmtmgmt.local")
        print("  4. Find 'oidc.user:https://rqmtmgmt.local:rqmtmgmt-frontend'")
        print("  5. Copy the 'access_token' value")
        return 1
    
    token = sys.argv[1]
    print("✓ Using provided access token")
    print()
    
    # Parse SRS markdown
    srs_file = "SOFTWARE_REQUIREMENTS_SPECIFICATION.md"
    print(f"Parsing {srs_file}...")
    
    try:
        sections = parse_srs_markdown(srs_file)
    except FileNotFoundError:
        print(f"ERROR: File not found: {srs_file}")
        return 1
    except Exception as e:
        print(f"ERROR: Failed to parse file: {str(e)}")
        return 1
    
    total_sections = len(sections)
    total_requirements = sum(len(s.requirements) for s in sections)
    
    print(f"✓ Parsed {total_sections} sections with {total_requirements} requirements")
    print()
    
    # Show summary by level
    by_level = {}
    for section in sections:
        by_level.setdefault(section.level, []).append(section)
    
    for level in sorted(by_level.keys()):
        level_sections = by_level[level]
        level_reqs = sum(len(s.requirements) for s in level_sections)
        print(f"  Level {level}: {len(level_sections)} sections, {level_reqs} requirements")
    print()
    
    # Show some examples
    print("Example sections:")
    for section in sections[:5]:
        indent = "  " * section.level
        print(f"{indent}{section.number} {section.title} (L{section.level}, parent: {section.parent_number or 'document'})")
    print("  ...")
    print()
    
    # Ask for confirmation
    response = input("Create these sections and requirements? (yes/no): ")
    if response.lower() not in ['yes', 'y']:
        print("Aborted by user")
        return 0
    print()
    
    # Create sections and requirements
    print("Creating sections and requirements...")
    print("-" * 80)
    
    # Map section number to created section ID
    section_id_map = {}
    
    # Process sections level by level to ensure parents exist
    for level in sorted(by_level.keys()):
        level_sections = by_level[level]
        print(f"\nLevel {level} Sections ({len(level_sections)}):")
        print("-" * 40)
        
        for section in level_sections:
            indent = "  " * (section.level - 1)
            print(f"{indent}{section.number} {section.title}")
            
            # Build section data
            section_data = {
                "title": section.title,
                "description": section.description,
                "sectionOrder": section.order,
                "sectionNumber": section.number,
                "isNotApplicable": False
            }
            
            # Set either documentId (for root sections) or parentSectionId (for subsections)
            if section.parent_number is None:
                section_data["documentId"] = DOCUMENT_ID
            else:
                parent_id = section_id_map.get(section.parent_number)
                if not parent_id:
                    print(f"  ✗ ERROR: Parent section {section.parent_number} not found, skipping")
                    continue
                section_data["parentSectionId"] = parent_id
            
            # Create section
            created_section = create_section_via_api(token, section_data)
            
            if created_section:
                section_id = created_section.get('id')
                section_id_map[section.number] = section_id
                print(f"  ✓ Created section (ID: {section_id})")
                
                # Create requirements in this section
                if section.requirements:
                    print(f"    Creating {len(section.requirements)} requirements...")
                    for req in section.requirements:
                        req_data = {
                            "title": req['title'],
                            "description": req['description'],
                            "type": REQUIREMENT_TYPES["SRS"],
                            "status": REQUIREMENT_STATUS["Draft"],
                            "projectId": PROJECT_ID,
                            "documentId": DOCUMENT_ID,
                            "sectionId": section_id,
                            "version": 1
                        }
                        
                        created_req = create_requirement_via_api(token, req_data)
                        if created_req:
                            print(f"      ✓ {req['id']}")
                        else:
                            print(f"      ✗ {req['id']} (failed)")
            else:
                print(f"  ✗ Failed to create section")
    
    # Summary
    print()
    print("=" * 80)
    print("SUMMARY")
    print("=" * 80)
    print(f"Sections created: {len(section_id_map)} / {total_sections}")
    print(f"Target requirements: {total_requirements}")
    print()
    
    if len(section_id_map) == total_sections:
        print("✓ All sections created successfully!")
        print()
        print("Next steps:")
        print("  1. Open https://rqmtmgmt.local")
        print(f"  2. Navigate to Project #{PROJECT_ID}")
        print(f"  3. Open Document #{DOCUMENT_ID}")
        print("  4. View the hierarchical sections with expand/collapse")
        return 0
    else:
        print(f"⚠ Some sections failed to create")
        return 1


if __name__ == "__main__":
    sys.exit(main())
