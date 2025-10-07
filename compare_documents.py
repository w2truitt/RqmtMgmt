#!/usr/bin/env python3
"""
Document Comparison Tool
Compares the original SOFTWARE_REQUIREMENTS_SPECIFICATION.md with project 25 document 11
"""

import json
import subprocess
import re
from typing import Dict, List, Any

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

def api_call(endpoint: str, token: str) -> Any:
    """Make API call with authentication"""
    cmd = [
        'curl', '-k', '-s',
        '-H', f'Authorization: Bearer {token}',
        '-H', 'Accept: application/json',
        f'https://rqmtmgmt.local/api/{endpoint}'
    ]
    
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        raise Exception(f"API call failed: {result.stderr}")
    
    return json.loads(result.stdout)

def parse_original_document():
    """Parse the original SOFTWARE_REQUIREMENTS_SPECIFICATION.md"""
    with open('/home/wtruitt/src/repos/RqmtMgmt/SOFTWARE_REQUIREMENTS_SPECIFICATION.md', 'r') as f:
        content = f.read()
    
    sections = {}
    requirements = {}
    
    # Extract sections using regex
    section_pattern = r'^#{1,4}\s+(\d+(?:\.\d+)*)\s+(.+?)$'
    requirement_pattern = r'^\s*-\s+\*\*([A-Z-]+\d+)\*\*:\s+(.+?)$'
    
    current_section = None
    for line in content.split('\n'):
        # Check for section headers
        section_match = re.match(section_pattern, line)
        if section_match:
            section_num = section_match.group(1)
            section_title = section_match.group(2)
            current_section = section_num
            sections[section_num] = {
                'title': section_title,
                'requirements': []
            }
        
        # Check for requirements
        req_match = re.match(requirement_pattern, line)
        if req_match and current_section:
            req_id = req_match.group(1)
            req_text = req_match.group(2)
            requirements[req_id] = {
                'text': req_text,
                'section': current_section
            }
            sections[current_section]['requirements'].append(req_id)
    
    return sections, requirements

def analyze_api_document(token: str):
    """Analyze document 11 from the API"""
    # Get document info
    document = api_call('Documents/11', token)
    
    # Get sections hierarchy
    sections = api_call('DocumentSections/document/11/hierarchy', token)
    
    # Get all requirements for document 11
    requirements = api_call('Requirement/document/11', token)
    
    return document, sections, requirements

def compare_sections(original_sections: Dict, api_sections: List[Dict]) -> Dict:
    """Compare section structures"""
    comparison = {
        'original_sections': len(original_sections),
        'api_sections': len(api_sections),
        'section_mapping': {},
        'missing_in_api': [],
        'extra_in_api': []
    }
    
    # Create flat list of API sections
    def flatten_sections(sections, level=0):
        flat = []
        for section in sections:
            flat.append({
                'id': section['id'],
                'title': section['title'],
                'number': section.get('sectionNumber', ''),
                'level': level,
                'requirement_count': section.get('requirementCount', 0)
            })
            if section.get('childSections'):
                flat.extend(flatten_sections(section['childSections'], level + 1))
        return flat
    
    api_flat = flatten_sections(api_sections)
    
    # Find matching sections by title similarity
    for orig_num, orig_section in original_sections.items():
        orig_title = orig_section['title']
        best_match = None
        best_score = 0
        
        for api_section in api_flat:
            api_title = api_section['title']
            # Simple title matching
            if orig_title.lower() in api_title.lower() or api_title.lower() in orig_title.lower():
                score = len(set(orig_title.lower().split()) & set(api_title.lower().split()))
                if score > best_score:
                    best_score = score
                    best_match = api_section
        
        if best_match:
            comparison['section_mapping'][orig_num] = {
                'original': orig_section,
                'api': best_match,
                'match_score': best_score
            }
    
    return comparison

def compare_requirements(original_reqs: Dict, api_reqs: List[Dict]) -> Dict:
    """Compare requirements"""
    comparison = {
        'original_count': len(original_reqs),
        'api_count': len(api_reqs),
        'matched_requirements': {},
        'missing_in_api': [],
        'extra_in_api': []
    }
    
    # Create mapping by requirement ID pattern
    api_by_id = {}
    for req in api_reqs:
        title = req.get('title', '')
        # Extract requirement ID from title
        req_match = re.search(r'(REQ-[A-Z]+-\d+)', title)
        if req_match:
            req_id = req_match.group(1)
            api_by_id[req_id] = req
    
    # Compare requirements
    for orig_id, orig_req in original_reqs.items():
        if orig_id in api_by_id:
            comparison['matched_requirements'][orig_id] = {
                'original': orig_req,
                'api': api_by_id[orig_id]
            }
        else:
            comparison['missing_in_api'].append(orig_id)
    
    # Find extra requirements in API
    for req in api_reqs:
        title = req.get('title', '')
        req_match = re.search(r'(REQ-[A-Z]+-\d+)', title)
        if req_match:
            req_id = req_match.group(1)
            if req_id not in original_reqs:
                comparison['extra_in_api'].append(req_id)
    
    return comparison

def generate_report(document_info, section_comparison, requirement_comparison):
    """Generate comprehensive comparison report"""
    report = []
    report.append("# Document Comparison Report")
    report.append("## Project 25 Document 11 vs SOFTWARE_REQUIREMENTS_SPECIFICATION.md")
    report.append("")
    
    # Document overview
    report.append("## Document Overview")
    report.append(f"**API Document Title**: {document_info['title']}")
    report.append(f"**API Document Type**: {document_info['type']}")
    report.append(f"**API Document Status**: {document_info['status']}")
    report.append(f"**API Document Version**: {document_info['version']}")
    report.append("")
    
    # Section comparison
    report.append("## Section Structure Comparison")
    report.append(f"- Original document sections: {section_comparison['original_sections']}")
    report.append(f"- API document sections: {section_comparison['api_sections']}")
    report.append("")
    
    report.append("### Section Mapping")
    for orig_num, mapping in section_comparison['section_mapping'].items():
        orig = mapping['original']
        api = mapping['api']
        report.append(f"- **{orig_num} {orig['title']}** → **{api['title']}** (ID: {api['id']}, Reqs: {api['requirement_count']})")
    report.append("")
    
    # Requirements comparison
    report.append("## Requirements Comparison")
    report.append(f"- Original requirements: {requirement_comparison['original_count']}")
    report.append(f"- API requirements: {requirement_comparison['api_count']}")
    report.append(f"- Matched requirements: {len(requirement_comparison['matched_requirements'])}")
    report.append(f"- Missing in API: {len(requirement_comparison['missing_in_api'])}")
    report.append(f"- Extra in API: {len(requirement_comparison['extra_in_api'])}")
    report.append("")
    
    if requirement_comparison['missing_in_api']:
        report.append("### Missing Requirements in API")
        for req_id in requirement_comparison['missing_in_api'][:10]:  # Show first 10
            report.append(f"- {req_id}")
        if len(requirement_comparison['missing_in_api']) > 10:
            report.append(f"- ... and {len(requirement_comparison['missing_in_api']) - 10} more")
        report.append("")
    
    if requirement_comparison['extra_in_api']:
        report.append("### Extra Requirements in API")
        for req_id in requirement_comparison['extra_in_api'][:10]:  # Show first 10
            report.append(f"- {req_id}")
        if len(requirement_comparison['extra_in_api']) > 10:
            report.append(f"- ... and {len(requirement_comparison['extra_in_api']) - 10} more")
        report.append("")
    
    # Sample matched requirements
    report.append("### Sample Matched Requirements")
    count = 0
    for req_id, match in requirement_comparison['matched_requirements'].items():
        if count >= 5:  # Show first 5
            break
        orig = match['original']
        api = match['api']
        report.append(f"**{req_id}**")
        report.append(f"- Original: {orig['text']}")
        report.append(f"- API: {api['description']}")
        report.append("")
        count += 1
    
    return "\n".join(report)

def main():
    """Main comparison function"""
    print("🔍 Starting document comparison...")
    
    try:
        # Get API token
        print("🔐 Getting API token...")
        token = get_api_token()
        
        # Parse original document
        print("📄 Parsing original SOFTWARE_REQUIREMENTS_SPECIFICATION.md...")
        original_sections, original_requirements = parse_original_document()
        
        # Analyze API document
        print("🌐 Analyzing API document 11...")
        document_info, api_sections, api_requirements = analyze_api_document(token)
        
        # Compare sections
        print("🔗 Comparing sections...")
        section_comparison = compare_sections(original_sections, api_sections)
        
        # Compare requirements
        print("📋 Comparing requirements...")
        requirement_comparison = compare_requirements(original_requirements, api_requirements)
        
        # Generate report
        print("📊 Generating comparison report...")
        report = generate_report(document_info, section_comparison, requirement_comparison)
        
        # Save report
        with open('/home/wtruitt/src/repos/RqmtMgmt/DOCUMENT_COMPARISON_REPORT.md', 'w') as f:
            f.write(report)
        
        print("✅ Comparison complete! Report saved to DOCUMENT_COMPARISON_REPORT.md")
        print("\n" + "="*60)
        print(report)
        
    except Exception as e:
        print(f"❌ Error during comparison: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())