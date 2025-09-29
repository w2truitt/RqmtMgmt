# E2E Test Logging System

This document explains the improved logging system for E2E tests, which provides better control over test output verbosity.

## Overview

The new logging system addresses the issue of excessive console output during test execution, particularly from authentication caching operations. It provides configurable verbosity levels and structured logging categories.

## Logging Levels

| Level | Description | Use Case |
|-------|-------------|----------|
| **Silent** | No output except critical errors | CI/CD pipelines where you only want to see failures |
| **Minimal** | Test results and failures only | Quick test runs, focusing on pass/fail status |
| **Normal** | Include authentication and major operations | Default level, good balance of information |
| **Detailed** | Include debug information | Troubleshooting test issues |
| **Verbose** | All logging including internal operations | Deep debugging, development |

## Logging Categories

The system categorizes log messages:

- **[AUTH]** - Authentication operations (session caching, login attempts)
- **[TEST]** - Test execution steps and major operations  
- **[DEBUG]** - Debug information for troubleshooting
- **[VERBOSE]** - Internal operations and detailed traces
- **[ERROR]** - Errors and critical issues
- **[INFO]** - Important information

## Usage

### Method 1: Using the Helper Scripts

**Linux/macOS:**
```bash
# Run with minimal logging (less verbose)
./run-e2e-tests.sh Minimal

# Run with normal logging (default)
./run-e2e-tests.sh Normal

# Run authentication tests with detailed logging
./run-e2e-tests.sh Detailed "*Authentication*"

# Run specific test with verbose logging
./run-e2e-tests.sh Verbose "*BackendEmailExtraction_VerifyTokenPersistence*"
```

**Windows PowerShell:**
```powershell
# Run with minimal logging
.\run-e2e-tests.ps1 Minimal

# Run with normal logging
.\run-e2e-tests.ps1 Normal

# Run authentication tests with detailed logging  
.\run-e2e-tests.ps1 Detailed "*Authentication*"
```

### Method 2: Environment Variable

Set the environment variable before running tests:

**Linux/macOS:**
```bash
export XUNIT_LOG_LEVEL=Minimal
dotnet test --logger="console;verbosity=minimal"
```

**Windows:**
```cmd
set XUNIT_LOG_LEVEL=Minimal
dotnet test --logger="console;verbosity=minimal"
```

**PowerShell:**
```powershell
$env:XUNIT_LOG_LEVEL="Minimal"
dotnet test --logger="console;verbosity=minimal"
```

### Method 3: Direct dotnet test Command

```bash
# Minimal logging - suppress most authentication messages
XUNIT_LOG_LEVEL=Minimal dotnet test --logger="console;verbosity=minimal" --verbosity minimal

# Normal logging - balanced output (default)
XUNIT_LOG_LEVEL=Normal dotnet test --logger="console;verbosity=normal" --verbosity normal

# Detailed logging - include debug information
XUNIT_LOG_LEVEL=Detailed dotnet test --logger="console;verbosity=detailed" --verbosity detailed
```

## Example: BackendEmailExtraction_VerifyTokenPersistence

### Before (Normal Level):
```
Using cached authentication file for: admin@rqmtmgmt.local
  Passed frontend.E2ETests.Workflows.BackendEmailExtractionWorkflowTests.BackendEmailExtraction_VerifyTokenPersistence [4 s]
```

### After (Minimal Level):
```
  Passed frontend.E2ETests.Workflows.BackendEmailExtractionWorkflowTests.BackendEmailExtraction_VerifyTokenPersistence [4 s]
```

### After (Detailed Level):
```
[AUTH] Using cached authentication file for: admin@rqmtmgmt.local
[TEST] Creating authenticated context for: admin@rqmtmgmt.local
[TEST] Authenticated context ready for: admin@rqmtmgmt.local
[TEST] Testing authentication token persistence
[TEST] Token persisted for page: /dashboard
[TEST] Token persisted for page: /projects
[TEST] Token persisted for page: /users
[TEST] Token persisted for page: /requirements
[TEST] Token persistence verification completed
  Passed frontend.E2ETests.Workflows.BackendEmailExtractionWorkflowTests.BackendEmailExtraction_VerifyTokenPersistence [4 s]
```

## Benefits

1. **Reduced Noise**: Authentication caching messages can be suppressed for cleaner CI output
2. **Flexible Debugging**: Increase verbosity when troubleshooting specific issues
3. **Structured Output**: Categorized messages make it easier to filter and understand logs
4. **Performance Insights**: Detailed timing information available when needed
5. **CI/CD Friendly**: Silent and minimal modes perfect for automated environments

## Integration with Existing Tests

The logging system is backward compatible. Existing tests will continue to work without modification, using the Normal logging level by default.

To add logging to new tests, use the `TestLogger` class:

```csharp
public class MyTest : AuthenticatedE2ETestBase
{
    public MyTest(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        SetAdminUser();
    }

    [Fact]
    public async Task MyTest_DoSomething_Success()
    {
        TestLogger.LogTestStep("Starting test operation", Output);
        
        // Test implementation
        
        TestLogger.LogDebug("Debug information for troubleshooting", Output);
        TestLogger.LogTestStep("Test operation completed", Output);
    }
}
```

## Troubleshooting

If you're still seeing too much output:

1. **Check Environment Variable**: Ensure `XUNIT_LOG_LEVEL` is set correctly
2. **Verify Logger Configuration**: Use the correct `--logger` and `--verbosity` flags
3. **Use Helper Scripts**: The provided scripts handle the configuration automatically
4. **Test Specific Cases**: Use test filters to run only the tests you're debugging

## Migration Guide

For teams currently using the old logging:

1. **No immediate action required** - existing tests work as-is
2. **Gradually adopt** - use `TestLogger` in new tests
3. **Customize CI/CD** - update build scripts to use Minimal or Silent levels
4. **Development workflow** - use Detailed/Verbose levels when debugging