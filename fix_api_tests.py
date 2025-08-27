#!/usr/bin/env python3
"""
Script to fix the formatting issues in converted API test files
"""

import os
import re

def fix_api_test_file(file_path):
    """Fix formatting issues in a converted API test file"""
    print(f"Fixing {file_path}...")
    
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Fix the Collection attribute formatting
    content = re.sub(
        r'\[Collection\("Integration Tests"\)\]\s*\n\s*\n(\s*)public class',
        r'[Collection("Integration Tests")]\n\1public class',
        content
    )
    
    # Add the summary comment update
    content = re.sub(
        r'(/// <summary>\s*\n/// .*?\n/// .*?)\n(\s*/// </summary>)',
        r'\1\n    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.\n\2',
        content,
        flags=re.MULTILINE | re.DOTALL
    )
    
    # Add SkipIfSystemNotAvailableAsync calls to methods that don't have them
    def add_skip_to_method(match):
        method = match.group(0)
        if 'SkipIfSystemNotAvailableAsync' in method:
            return method  # Already has skip call
        
        # Find the first line after the opening brace
        lines = method.split('\n')
        for i, line in enumerate(lines):
            if '{' in line:
                # Insert skip call after opening brace
                insert_pos = i + 1
                # If next line is "// Arrange", insert before it
                if insert_pos < len(lines) and '// Arrange' in lines[insert_pos]:
                    lines.insert(insert_pos, '            await SkipIfSystemNotAvailableAsync();')
                    lines.insert(insert_pos + 1, '')
                else:
                    # Insert arrange comment and skip call
                    lines.insert(insert_pos, '            // Arrange')
                    lines.insert(insert_pos + 1, '            await SkipIfSystemNotAvailableAsync();')
                    lines.insert(insert_pos + 2, '')
                break
        return '\n'.join(lines)
    
    # Apply to all [Fact] methods
    content = re.sub(
        r'(\s*\[Fact\]\s*public async Task \w+\([^)]*\)\s*\{[^\n]*\n(?:[^\n]*\n){0,3})',
        add_skip_to_method,
        content,
        flags=re.MULTILINE
    )
    
    with open(file_path, 'w') as f:
        f.write(content)
    
    print(f"✓ Fixed {file_path}")

def main():
    """Main function to fix all converted API test files"""
    base_dir = "/home/wtruitt/src/repos/RqmtMgmt/backend.ApiTests"
    
    # Files that were converted
    converted_files = [
        "ProjectApiTests.cs",
        "TestCaseApiTests.cs", 
        "TestRunSessionApiTests.cs",
        "RedlineApiTests.cs",
        "RequirementTestCaseLinkApiTests.cs",
        "TestExecutionApiTests.cs",
        "UserApiTests.cs"
    ]
    
    for filename in converted_files:
        file_path = os.path.join(base_dir, filename)
        if os.path.exists(file_path):
            fix_api_test_file(file_path)
        else:
            print(f"Warning: {file_path} not found")
    
    print("Fixes complete!")

if __name__ == "__main__":
    main()