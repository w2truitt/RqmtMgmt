#!/usr/bin/env python3
import re

# Read the file
with open('backend.Tests/TestCaseServiceTests.cs', 'r') as f:
    content = f.read()

# Define the test methods that need to be fixed
failing_tests = [
    'CreateAsync_AddsTestCase',
    'GetAllAsync_ReturnsAllTestCases', 
    'GetByIdAsync_ReturnsCorrectTestCaseOrNull',
    'GetByIdAsync_WithValidId_ReturnsTestCaseWithSteps',
    'CreateAsync_WithMultipleValidSteps_Succeeds',
    'CreateAsync_WithNullSteps_Succeeds',
    'CreateAsync_WithEmptySteps_Succeeds',
    'GetAllAsync_WithTestCases_ReturnsAllWithSteps'
]

# For each failing test, add user creation and fix CreatedBy references
for test_name in failing_tests:
    # Pattern to match the test method
    pattern = rf'(\[Fact\]\s+public async Task {test_name}\(\)\s+\{{\s+using var db = GetDbContext\(nameof\({test_name}\)\);)'
    
    # Replacement that adds user creation
    replacement = rf'\1\n            var (user, _) = await TestDataHelper.SetupBasicTestDataAsync(db);'
    
    content = re.sub(pattern, replacement, content, flags=re.MULTILINE | re.DOTALL)

# Replace all CreatedBy = 1 with CreatedBy = user.Id
content = re.sub(r'CreatedBy = 1', 'CreatedBy = user.Id', content)

# Write the fixed content back
with open('backend.Tests/TestCaseServiceTests.cs', 'w') as f:
    f.write(content)

print("Fixed TestCaseServiceTests.cs")