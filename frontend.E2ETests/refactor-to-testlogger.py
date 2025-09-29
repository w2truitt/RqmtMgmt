#!/usr/bin/env python3
"""
E2E Test Logger Refactoring Script

This script automatically transforms Output.WriteLine calls in E2E test files
to use the appropriate TestLogger methods based on message content analysis.
"""

import os
import re
import sys
import argparse
import shutil
from pathlib import Path

# Transformation rules - patterns mapped to logger methods
TRANSFORMATION_RULES = {
    # Authentication-related messages
    r'(authenticated|authentication|login|session|token|cache|user|email)': 'TestLogger.LogAuthentication',
    
    # Debug information (detailed technical info)
    r'(debug|found|current|page|url|title|element|input|form|button|selector|locator)': 'TestLogger.LogDebug',
    
    # Error messages
    r'(error|failed|exception|invalid|missing|cannot|unable)': 'TestLogger.LogError',
    
    # Test execution steps (default for most messages)
    r'(testing|verifying|checking|validating|navigating|creating|updating|deleting|starting|completing|finished|success|passed)': 'TestLogger.LogTestStep',
}

def analyze_message_content(message_content):
    """Analyze message content to determine the appropriate logger method."""
    # Clean up the message for analysis
    clean_message = message_content.lower()
    
    # Remove quotes and interpolation markers
    clean_message = re.sub(r'["\$\{\}]', '', clean_message)
    
    # Check each pattern in order of specificity
    for pattern, logger_method in TRANSFORMATION_RULES.items():
        if re.search(pattern, clean_message, re.IGNORECASE):
            return logger_method
    
    # Default to TestStep for general messages
    return 'TestLogger.LogTestStep'

def add_using_statement(content):
    """Add the TestLogger using statement if not present."""
    using_pattern = r'using frontend\.E2ETests\.Infrastructure;'
    
    if re.search(using_pattern, content):
        return content, False  # Already present
    
    # Find the last using statement
    using_statements = list(re.finditer(r'^using\s+[^;]+;', content, re.MULTILINE))
    
    if using_statements:
        last_using = using_statements[-1]
        insert_pos = last_using.end()
        
        # Insert after the last using statement
        new_content = (
            content[:insert_pos] + 
            '\nusing frontend.E2ETests.Infrastructure;' +
            content[insert_pos:]
        )
        return new_content, True
    else:
        # No using statements found, add at the beginning
        new_content = 'using frontend.E2ETests.Infrastructure;\n' + content
        return new_content, True

def transform_output_writeline(content):
    """Transform all Output.WriteLine calls to appropriate TestLogger calls."""
    # Pattern to match Output.WriteLine calls
    pattern = r'(\s*)Output\.WriteLine\(([^)]+)\);'
    changes = []
    
    def replace_match(match):
        indent = match.group(1)
        message_content = match.group(2).strip()
        
        # Determine the appropriate logger method
        logger_method = analyze_message_content(message_content)
        
        # Create the replacement
        replacement = f'{indent}{logger_method}({message_content}, Output);'
        
        # Track the change
        original = f'Output.WriteLine({message_content});'
        new = f'{logger_method}({message_content}, Output);'
        changes.append((original, new))
        
        return replacement
    
    new_content = re.sub(pattern, replace_match, content)
    return new_content, changes

def transform_file(file_path, dry_run=False, backup_files=False):
    """Transform a single file."""
    print(f"Processing: {file_path}")
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            original_content = f.read()
        
        content = original_content
        total_changes = []
        
        # Add using statement if needed
        content, using_added = add_using_statement(content)
        if using_added:
            total_changes.append("Added using statement for TestLogger")
        
        # Transform Output.WriteLine calls
        content, writeline_changes = transform_output_writeline(content)
        total_changes.extend([f"Changed: {old} -> {new}" for old, new in writeline_changes])
        
        if total_changes:
            print(f"  Found {len(total_changes)} changes:")
            for change in total_changes[:5]:  # Show first 5 changes
                print(f"    {change}")
            if len(total_changes) > 5:
                print(f"    ... and {len(total_changes) - 5} more changes")
            
            if not dry_run:
                if backup_files:
                    backup_path = f"{file_path}.bak"
                    shutil.copy2(file_path, backup_path)
                    print(f"  Created backup: {backup_path}")
                
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(content)
                print("  File updated successfully!")
            else:
                print("  [DRY RUN] File would be updated")
        else:
            print("  No changes needed")
            
    except Exception as e:
        print(f"  Error processing file: {e}")
        return False
    
    print()
    return len(total_changes) > 0

def main():
    parser = argparse.ArgumentParser(
        description="Refactor E2E test files to use TestLogger instead of Output.WriteLine"
    )
    parser.add_argument(
        '--dry-run', 
        action='store_true', 
        help='Show what changes would be made without modifying files'
    )
    parser.add_argument(
        '--backup', 
        action='store_true', 
        help='Create .bak files before making changes'
    )
    parser.add_argument(
        '--workflows-path',
        default='frontend.E2ETests/Workflows',
        help='Path to the Workflows directory (default: frontend.E2ETests/Workflows)'
    )
    
    args = parser.parse_args()
    
    print("E2E Test Logger Refactoring Script")
    print("=================================")
    
    if args.dry_run:
        print("DRY RUN MODE - No files will be modified")
    
    if args.backup and not args.dry_run:
        print("BACKUP MODE - .bak files will be created")
    
    print()
    
    # Check if workflows directory exists
    workflows_path = Path(args.workflows_path)
    if not workflows_path.exists():
        print(f"Error: Workflows directory not found at {workflows_path}")
        print("Please run this script from the repository root directory.")
        sys.exit(1)
    
    # Find all C# files in the workflows directory
    cs_files = list(workflows_path.glob('*.cs'))
    # Exclude base classes
    cs_files = [f for f in cs_files if 'TestBase' not in f.name]
    
    print(f"Found {len(cs_files)} files to process")
    print()
    
    files_changed = 0
    for file_path in cs_files:
        if transform_file(file_path, args.dry_run, args.backup):
            files_changed += 1
    
    print(f"Refactoring complete! {files_changed} files were modified.")
    
    if args.dry_run:
        print()
        print("To apply these changes, run the script without --dry-run:")
        print(f"  python3 {sys.argv[0]}")
        print()
        print("To apply changes with backups:")
        print(f"  python3 {sys.argv[0]} --backup")

if __name__ == '__main__':
    main()