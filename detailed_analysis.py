#!/usr/bin/env python3
"""
Detailed Document Analysis Tool
Provides in-depth analysis of project 25 document 11 structure and content
"""

import json
import subprocess
import re
from collections import defaultdict
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
    return json.loads(result.stdout)

def analyze_section_structure(sections: List[Dict]) -> Dict:
    """Analyze section structure for issues"""
    analysis = {
        'total_sections': 0,
        'duplicated_sections': [],
        'orphaned_sections': [],
        'section_tree': [],
        'requirements_distribution': {},
        'structural_issues': []
    }
    
    def process_sections(sections_list, level=0, parent_path=""):
        section_titles = defaultdict(list)
        
        for section in sections_list:
            analysis['total_sections'] += 1
            
            title = section['title']
            section_id = section['id']
            req_count = section.get('requirementCount', 0)
            total_req_count = section.get('totalRequirementCount', 0)
            
            # Track section path
            current_path = f"{parent_path}/{title}" if parent_path else title
            
            # Check for duplicated titles at same level
            section_titles[title].append({
                'id': section_id,
                'path': current_path,
                'req_count': req_count
            })
            
            # Add to section tree
            analysis['section_tree'].append({
                'level': level,
                'id': section_id,
                'title': title,
                'path': current_path,
                'requirement_count': req_count,
                'total_requirement_count': total_req_count,
                'section_number': section.get('sectionNumber', 'None')
            })
            
            # Track requirements distribution
            analysis['requirements_distribution'][section_id] = {
                'title': title,
                'direct_requirements': req_count,
                'total_requirements': total_req_count,
                'level': level
            }
            
            # Process child sections
            if section.get('childSections'):
                process_sections(section['childSections'], level + 1, current_path)
        
        # Check for duplicates at this level
        for title, instances in section_titles.items():
            if len(instances) > 1:
                analysis['duplicated_sections'].append({
                    'title': title,
                    'level': level,
                    'instances': instances
                })
    
    process_sections(sections)
    
    # Identify structural issues
    for duplicate in analysis['duplicated_sections']:
        analysis['structural_issues'].append(f"Duplicate section '{duplicate['title']}' found at level {duplicate['level']} with {len(duplicate['instances'])} instances")
    
    return analysis

def analyze_requirements_coverage(requirements: List[Dict]) -> Dict:
    """Analyze requirements for coverage and duplications"""
    analysis = {
        'total_requirements': len(requirements),
        'requirements_by_section': defaultdict(list),
        'duplicate_requirements': [],
        'requirement_id_patterns': defaultdict(list),
        'status_distribution': defaultdict(int),
        'type_distribution': defaultdict(int)
    }
    
    # Group requirements by various criteria
    requirement_texts = defaultdict(list)
    
    for req in requirements:
        req_id = req['id']
        title = req.get('title', '')
        description = req.get('description', '')
        section_id = req.get('sectionId')
        status = req.get('status', 'Unknown')
        req_type = req.get('type', 'Unknown')
        
        # Track by section
        analysis['requirements_by_section'][section_id].append({
            'id': req_id,
            'title': title,
            'description': description
        })
        
        # Track status and type distribution
        analysis['status_distribution'][status] += 1
        analysis['type_distribution'][req_type] += 1
        
        # Extract requirement ID pattern
        req_match = re.search(r'(REQ-[A-Z]+-\d+)', title)
        if req_match:
            req_pattern = req_match.group(1)
            analysis['requirement_id_patterns'][req_pattern].append(req_id)
        
        # Check for duplicate descriptions
        if description:
            requirement_texts[description].append({
                'id': req_id,
                'title': title,
                'section_id': section_id
            })
    
    # Find duplicates
    for text, instances in requirement_texts.items():
        if len(instances) > 1:
            analysis['duplicate_requirements'].append({
                'description': text,
                'instances': instances
            })
    
    return analysis

def generate_detailed_report(document_info, section_analysis, requirement_analysis):
    """Generate detailed analysis report"""
    report = []
    report.append("# Detailed Document Analysis Report")
    report.append("## Project 25 Document 11 - TestFlow Pro SRS")
    report.append("")
    
    # Document overview
    report.append("## Document Overview")
    report.append(f"- **Title**: {document_info['title']}")
    report.append(f"- **Type**: {document_info['type']}")
    report.append(f"- **Status**: {document_info['status']}")
    report.append(f"- **Version**: {document_info['version']}")
    report.append(f"- **Created**: {document_info['createdAt']}")
    report.append(f"- **Project ID**: {document_info['projectId']}")
    report.append("")
    
    # Section structure analysis
    report.append("## Section Structure Analysis")
    report.append(f"- **Total Sections**: {section_analysis['total_sections']}")
    report.append(f"- **Duplicated Sections**: {len(section_analysis['duplicated_sections'])}")
    report.append(f"- **Structural Issues**: {len(section_analysis['structural_issues'])}")
    report.append("")
    
    if section_analysis['duplicated_sections']:
        report.append("### Duplicated Sections")
        for dup in section_analysis['duplicated_sections']:
            report.append(f"**{dup['title']}** (Level {dup['level']})")
            for instance in dup['instances']:
                report.append(f"  - ID: {instance['id']}, Path: {instance['path']}, Requirements: {instance['req_count']}")
        report.append("")
    
    if section_analysis['structural_issues']:
        report.append("### Structural Issues")
        for issue in section_analysis['structural_issues']:
            report.append(f"- {issue}")
        report.append("")
    
    # Requirements analysis
    report.append("## Requirements Analysis")
    report.append(f"- **Total Requirements**: {requirement_analysis['total_requirements']}")
    report.append(f"- **Duplicate Requirements**: {len(requirement_analysis['duplicate_requirements'])}")
    report.append(f"- **Sections with Requirements**: {len([s for s in requirement_analysis['requirements_by_section'] if requirement_analysis['requirements_by_section'][s]])}")
    report.append("")
    
    # Status distribution
    report.append("### Requirements Status Distribution")
    for status, count in requirement_analysis['status_distribution'].items():
        report.append(f"- **{status}**: {count}")
    report.append("")
    
    # Type distribution
    report.append("### Requirements Type Distribution")
    for req_type, count in requirement_analysis['type_distribution'].items():
        report.append(f"- **{req_type}**: {count}")
    report.append("")
    
    # Duplicate requirements
    if requirement_analysis['duplicate_requirements']:
        report.append("### Duplicate Requirements")
        for i, dup in enumerate(requirement_analysis['duplicate_requirements'][:10]):  # Show first 10
            report.append(f"**Duplicate {i+1}**: {len(dup['instances'])} instances")
            report.append(f"Description: {dup['description'][:100]}...")
            for instance in dup['instances'][:3]:  # Show first 3 instances
                report.append(f"  - ID: {instance['id']}, Section: {instance['section_id']}")
            if len(dup['instances']) > 3:
                report.append(f"  - ... and {len(dup['instances']) - 3} more")
            report.append("")
        
        if len(requirement_analysis['duplicate_requirements']) > 10:
            report.append(f"... and {len(requirement_analysis['duplicate_requirements']) - 10} more duplicate groups")
        report.append("")
    
    # Section tree
    report.append("## Section Hierarchy")
    for section in section_analysis['section_tree'][:20]:  # Show first 20
        indent = "  " * section['level']
        report.append(f"{indent}- **{section['title']}** (ID: {section['id']}, Reqs: {section['requirement_count']}/{section['total_requirement_count']})")
    
    if len(section_analysis['section_tree']) > 20:
        report.append(f"... and {len(section_analysis['section_tree']) - 20} more sections")
    report.append("")
    
    # Requirements by section summary
    report.append("## Requirements Distribution by Section")
    sorted_sections = sorted(
        requirement_analysis['requirements_by_section'].items(),
        key=lambda x: len(x[1]),
        reverse=True
    )
    
    for section_id, reqs in sorted_sections[:15]:  # Top 15 sections
        section_title = next((s['title'] for s in section_analysis['section_tree'] if s['id'] == section_id), f"Section {section_id}")
        report.append(f"- **{section_title}** (ID: {section_id}): {len(reqs)} requirements")
    
    if len(sorted_sections) > 15:
        report.append(f"... and {len(sorted_sections) - 15} more sections")
    report.append("")
    
    # Recommendations
    report.append("## Recommendations")
    recommendations = []
    
    if section_analysis['duplicated_sections']:
        recommendations.append("🔄 **Consolidate Duplicate Sections**: Multiple sections with identical names should be merged or renamed for clarity")
    
    if requirement_analysis['duplicate_requirements']:
        recommendations.append("📋 **Remove Duplicate Requirements**: Identical requirement descriptions should be consolidated")
    
    # Check for sections with many requirements
    high_req_sections = [s for s in requirement_analysis['requirements_by_section'].items() if len(s[1]) > 10]
    if high_req_sections:
        recommendations.append("📊 **Consider Section Subdivision**: Some sections have many requirements and could benefit from further subdivision")
    
    # Check for empty sections
    empty_sections = [s for s in section_analysis['section_tree'] if s['requirement_count'] == 0 and s['total_requirement_count'] == 0]
    if empty_sections:
        recommendations.append("📝 **Populate Empty Sections**: Some sections have no requirements and may need content or removal")
    
    if not recommendations:
        recommendations.append("✅ **Document Structure Looks Good**: No major structural issues identified")
    
    for rec in recommendations:
        report.append(f"- {rec}")
    report.append("")
    
    return "\n".join(report)

def main():
    """Main analysis function"""
    print("🔍 Starting detailed document analysis...")
    
    try:
        # Get API token
        print("🔐 Getting API token...")
        token = get_api_token()
        
        # Get document info
        print("📄 Getting document information...")
        document_info = api_call('Documents/11', token)
        
        # Get sections hierarchy
        print("🗂️ Analyzing section structure...")
        sections = api_call('DocumentSections/document/11/hierarchy', token)
        section_analysis = analyze_section_structure(sections)
        
        # Get requirements
        print("📋 Analyzing requirements...")
        requirements = api_call('Requirement/document/11', token)
        requirement_analysis = analyze_requirements_coverage(requirements)
        
        # Generate report
        print("📊 Generating detailed report...")
        report = generate_detailed_report(document_info, section_analysis, requirement_analysis)
        
        # Save report
        with open('/home/wtruitt/src/repos/RqmtMgmt/DETAILED_DOCUMENT_ANALYSIS.md', 'w') as f:
            f.write(report)
        
        print("✅ Analysis complete! Report saved to DETAILED_DOCUMENT_ANALYSIS.md")
        print("\n" + "="*80)
        print(report)
        
    except Exception as e:
        print(f"❌ Error during analysis: {e}")
        import traceback
        traceback.print_exc()
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())