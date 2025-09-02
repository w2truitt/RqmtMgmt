# E2E Test Troubleshooting Guide

## Problem
The `frontend.E2ETests.Workflows.ProjectsPageTests.Projects_HasExpectedPageElements_AuthenticatedUser` test was causing timeout issues that would exit the entire shell when running the segmented E2E test script.

## Solution
We've created a segmented approach with multiple scripts to isolate and troubleshoot the problematic tests without crashing the shell.

## Available Scripts

### 1. Main Segmented Test Runner
**File:** `scripts/run-e2e-tests-segmented.sh`
- **Purpose:** Runs E2E tests in smaller groups to identify issues
- **Improvements:** 
  - Better timeout handling (doesn't exit shell)
  - Duration tracking
  - Helpful suggestions when timeouts occur
  - New option (13) to switch to individual ProjectsPageTests runner

**Usage:**
```bash
./scripts/run-e2e-tests-segmented.sh
# Or run specific segment:
./scripts/run-e2e-tests-segmented.sh 4  # basic-navigation
```

### 2. Individual ProjectsPageTests Runner
**File:** `scripts/run-projects-page-tests-individual.sh`
- **Purpose:** Runs each ProjectsPageTests method individually to isolate timeout issues
- **Features:**
  - Runs each test method separately
  - 5-minute timeout per test (instead of 10 minutes for the whole class)
  - Detailed reporting of which tests pass/fail/timeout
  - Graceful timeout handling

**Usage:**
```bash
./scripts/run-projects-page-tests-individual.sh
# Options:
# 1-11: Run individual test methods
# 12 (all): Run all tests individually
# 13 (problematic): Run the specific problematic test
```

**Individual Test Methods:**
1. `Projects_NavigatesSuccessfully_AuthenticatedUser`
2. `Projects_LoadsWithoutErrors_AuthenticatedUser`
3. `Projects_HasExpectedPageElements_AuthenticatedUser` ⚠️ (problematic)
4. `Projects_CanCreateNewProject_AuthenticatedAdmin`
5. `Projects_CanSearchProjects_AuthenticatedUser`
6. `Projects_CanOpenAndCancelForm_AuthenticatedUser`
7. `Projects_FormValidatesRequiredFields_AuthenticatedUser`
8. `Projects_CanEditExistingProject_AuthenticatedAdmin`
9. `Projects_CanDeleteProject_AuthenticatedAdmin`
10. `Projects_CanPerformFullCrudWorkflow_AuthenticatedAdmin`
11. `Projects_ShowsProjectCounts_AuthenticatedUser`

### 3. Quick Problematic Test Runner
**File:** `scripts/run-problematic-test.sh`
- **Purpose:** Quickly test the specific problematic test method
- **Features:**
  - 5-minute timeout
  - Clear reporting of results
  - Confirms if the test is the source of timeout issues

**Usage:**
```bash
./scripts/run-problematic-test.sh
```

## Troubleshooting Workflow

### Step 1: Confirm the Issue
Run the problematic test in isolation:
```bash
./scripts/run-problematic-test.sh
```

### Step 2: Run All ProjectsPageTests Individually
If the problematic test times out, run all tests individually to see which ones work:
```bash
./scripts/run-projects-page-tests-individual.sh all
```

### Step 3: Investigate the Timeout
If `Projects_HasExpectedPageElements_AuthenticatedUser` times out, investigate:
- Page load times
- Element visibility waits
- Network requests
- JavaScript errors
- Blazor app initialization

### Step 4: Fix and Verify
After making fixes, run the individual test again:
```bash
./scripts/run-projects-page-tests-individual.sh problematic
```

### Step 5: Run Full Segment
Once individual tests pass, run the full basic-navigation segment:
```bash
./scripts/run-e2e-tests-segmented.sh 4
```

## Key Improvements Made

1. **Graceful Timeout Handling:** Scripts now use `set +e` around timeout commands to prevent shell exit
2. **Duration Tracking:** All scripts now track and report test duration
3. **Individual Test Isolation:** Can run each test method separately
4. **Better Error Reporting:** Clear indication of which tests timeout vs fail
5. **Helpful Suggestions:** Scripts provide guidance on next steps when issues occur

## Expected Behavior

- **Before:** Timeout would exit the entire shell/agent
- **After:** Timeout is caught gracefully, duration is reported, and execution continues

## Next Steps

1. Run `./scripts/run-problematic-test.sh` to confirm the timeout issue
2. If it times out, investigate the `Projects_HasExpectedPageElements_AuthenticatedUser` test implementation
3. Look for:
   - Long-running page load waits
   - Element visibility checks that might be failing
   - Network timeouts
   - Blazor-specific initialization issues

The segmented approach allows you to troubleshoot systematically without losing your shell session.