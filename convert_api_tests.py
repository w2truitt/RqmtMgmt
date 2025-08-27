#!/usr/bin/env python3
"""
Script to convert API test files from BaseApiTest to BaseIntegrationTest
"""

import os
import re

def convert_api_test_file(file_path):
    """Convert a single API test file to use BaseIntegrationTest"""
    print(f"Converting {file_path}...")
    
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Remove unused imports
    content = re.sub(r'using Microsoft\.AspNetCore\.Mvc\.Testing;\n', '', content)
    content = re.sub(r'using backend;\n', '', content)
    
    # Update class declaration
    content = re.sub(
        r'(\s*)public class (\w+ApiTests) : BaseApiTest',
        r'\1[Collection("Integration Tests")]\n\1public class \2 : BaseIntegrationTest',
        content
    )
    
    # Remove constructor
    content = re.sub(
        r'\s*public \w+ApiTests\(TestWebApplicationFactory<Program> factory\) : base\(factory\)\s*\{\s*\}\s*\n',
        '\n',
        content
    )
    
    # Add SkipIfSystemNotAvailableAsync calls to test methods
    # Find all [Fact] methods and add the skip call
    def add_skip_call(match):
        method_start = match.group(0)
        # Check if skip call already exists
        if 'SkipIfSystemNotAvailableAsync' in method_start:
            return method_start
        
        # Find the opening brace and add the skip call after it
        lines = method_start.split('\n')
        for i, line in enumerate(lines):
            if '{' in line and not line.strip().startswith('//'):
                # Insert the skip call after the opening brace
                if i + 1 < len(lines) and 'Arrange' in lines[i + 1]:
                    lines.insert(i + 1, '            await SkipIfSystemNotAvailableAsync();')
                    lines.insert(i + 2, '')
                elif i + 1 < len(lines):
                    lines.insert(i + 1, '            // Arrange')
                    lines.insert(i + 2, '            await SkipIfSystemNotAvailableAsync();')
                    lines.insert(i + 3, '')
                break
        return '\n'.join(lines)
    
    # Match [Fact] methods with their opening content
    content = re.sub(
        r'(\s*\[Fact\]\s*public async Task \w+\([^)]*\)\s*\{[^\{]*?)(?=\s*//|\s*var|\s*await|\s*\w+)',
        add_skip_call,
        content,
        flags=re.MULTILINE | re.DOTALL
    )
    
    # Update the class summary comment
    content = re.sub(
        r'(/// <summary>\s*\n/// .*?\n/// .*?)\n(/// </summary>)',
        r'\1\n    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.\n\2',
        content,
        flags=re.MULTILINE | re.DOTALL
    )
    
    with open(file_path, 'w') as f:
        f.write(content)
    
    print(f"✓ Converted {file_path}")

def main():
    """Main function to convert all API test files"""
    base_dir = "/home/wtruitt/src/repos/RqmtMgmt/backend.ApiTests"
    
    # Files that inherit from BaseApiTest
    base_api_test_files = [
        "ProjectApiTests.cs",
        "TestCaseApiTests.cs", 
        "TestRunSessionApiTests.cs",
        "RedlineApiTests.cs",
        "RequirementTestCaseLinkApiTests.cs",
        "TestExecutionApiTests.cs"
    ]
    
    # Files that use IClassFixture directly
    class_fixture_files = [
        "UserApiTests.cs"
    ]
    
    # Convert BaseApiTest files
    for filename in base_api_test_files:
        file_path = os.path.join(base_dir, filename)
        if os.path.exists(file_path):
            convert_api_test_file(file_path)
        else:
            print(f"Warning: {file_path} not found")
    
    # Convert IClassFixture files (they need slightly different handling)
    for filename in class_fixture_files:
        file_path = os.path.join(base_dir, filename)
        if os.path.exists(file_path):
            convert_api_test_file(file_path)
        else:
            print(f"Warning: {file_path} not found")
    
    print("Conversion complete!")

if __name__ == "__main__":
    main()