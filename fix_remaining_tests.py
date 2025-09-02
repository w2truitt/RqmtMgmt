#!/usr/bin/env python3
import re

# Read the file
with open('backend.Tests/TestCaseServiceTests.cs', 'r') as f:
    content = f.read()

# Find all test methods that use user.Id but don't have user creation
test_methods = re.findall(r'(\[Fact\].*?public async Task (\w+)\(\).*?\{.*?using var db = GetDbContext\(nameof\(\2\)\);)(.*?)(?=\[Fact\]|$)', content, re.DOTALL)

fixed_content = content

for full_match, test_name, method_body in test_methods:
    # Check if this method uses user.Id but doesn't have user creation
    if 'user.Id' in method_body and 'TestDataHelper.SetupBasicTestDataAsync' not in method_body:
        print(f"Fixing test: {test_name}")
        
        # Find the pattern to replace
        pattern = rf'(\[Fact\]\s+public async Task {test_name}\(\)\s+\{{\s+using var db = GetDbContext\(nameof\({test_name}\)\);)'
        replacement = rf'\1\n            var (user, _) = await TestDataHelper.SetupBasicTestDataAsync(db);'
        
        fixed_content = re.sub(pattern, replacement, fixed_content, flags=re.MULTILINE | re.DOTALL)

# Write the fixed content back
with open('backend.Tests/TestCaseServiceTests.cs', 'w') as f:
    f.write(fixed_content)

print("Fixed all remaining tests")