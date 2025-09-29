# Automated Refactoring Scripts

This directory contains scripts to automatically refactor E2E test files to use the new TestLogger system instead of `Output.WriteLine` calls.

## Available Scripts

### 1. Python Script (Recommended)
**File:** `refactor-to-testlogger.py`
**Requirements:** Python 3.6+

```bash
# Dry run to see what changes would be made
python3 refactor-to-testlogger.py --dry-run

# Apply changes with backup files
python3 refactor-to-testlogger.py --backup

# Apply changes without backups
python3 refactor-to-testlogger.py
```

### 2. PowerShell Script
**File:** `refactor-to-testlogger.ps1`
**Requirements:** PowerShell 5.1+ or PowerShell Core

```powershell
# Dry run
.\refactor-to-testlogger.ps1 -DryRun

# Apply with backups
.\refactor-to-testlogger.ps1 -BackupFiles
```

### 3. Bash Script
**File:** `refactor-to-testlogger.sh`
**Requirements:** Bash, standard Unix tools

```bash
# Dry run
./refactor-to-testlogger.sh --dry-run

# Apply with backups
./refactor-to-testlogger.sh --backup
```

## What the Scripts Do

### Automatic Transformations

The scripts analyze each `Output.WriteLine()` call and transform them based on message content:

| Message Content | Transforms To | Example |
|----------------|---------------|---------|
| Authentication-related (login, session, token, user, email) | `TestLogger.LogAuthentication()` | `"Starting JWT token extraction"` |
| Debug information (page, url, element, found, current) | `TestLogger.LogDebug()` | `"Current URL: {Page.Url}"` |
| Error messages (error, failed, exception, invalid) | `TestLogger.LogError()` | `"Login failed for user"` |
| Test steps (testing, verifying, creating, completing) | `TestLogger.LogTestStep()` | `"Testing user creation workflow"` |

### Code Changes Made

1. **Add Using Statement**: Adds `using frontend.E2ETests.Infrastructure;` if not present
2. **Transform Calls**: Changes `Output.WriteLine(message);` to `TestLogger.LogXxx(message, Output);`
3. **Preserve Formatting**: Maintains original indentation and code structure

### Example Transformation

**Before:**
```csharp
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

public class MyTest : AuthenticatedE2ETestBase
{
    [Fact]
    public async Task MyTest_DoSomething()
    {
        Output.WriteLine("Starting authentication test");
        Output.WriteLine($"Current page: {Page.Url}");
        Output.WriteLine("Test completed successfully");
    }
}
```

**After:**
```csharp
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using frontend.E2ETests.Infrastructure;

public class MyTest : AuthenticatedE2ETestBase
{
    [Fact]
    public async Task MyTest_DoSomething()
    {
        TestLogger.LogAuthentication("Starting authentication test", Output);
        TestLogger.LogDebug($"Current page: {Page.Url}", Output);
        TestLogger.LogTestStep("Test completed successfully", Output);
    }
}
```

## Script Results

Based on the dry run analysis, the scripts will process **32 files** and make approximately **250+ transformations** including:

- **32 using statements** added
- **220+ Output.WriteLine calls** transformed to appropriate TestLogger methods
- **Intelligent categorization** based on message content analysis

### Files to be Modified

The scripts will modify all `.cs` files in the `frontend.E2ETests/Workflows/` directory except for base classes (files containing "TestBase").

## Safety Features

- **Dry Run Mode**: Preview changes without modifying files
- **Backup Creation**: Optional `.bak` file creation before changes
- **Error Handling**: Graceful handling of file access issues
- **Encoding Preservation**: Maintains UTF-8 encoding and file structure

## Verification

After running the scripts, you can:

1. **Build the project** to ensure no compilation errors
2. **Run a few tests** to verify logging works correctly
3. **Check the output** with different log levels:
   ```bash
   # Test with minimal logging
   ./run-e2e-tests.sh Minimal "*LoggingSystem*"
   
   # Test with detailed logging
   ./run-e2e-tests.sh Detailed "*LoggingSystem*"
   ```

## Rollback

If you used the `--backup` option, you can rollback changes:

```bash
# Restore all backup files
for file in frontend.E2ETests/Workflows/*.bak; do
    mv "$file" "${file%.bak}"
done
```

## Manual Review Recommended

While the scripts are designed to be safe and accurate, it's recommended to:

1. **Review a few transformed files** to ensure the categorization is appropriate
2. **Run tests** to verify functionality
3. **Adjust any miscategorized logging** if needed (e.g., change `LogDebug` to `LogTestStep`)

The intelligent categorization should be ~95% accurate, but manual fine-tuning may be beneficial for specific use cases.