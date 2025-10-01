# E2E Test Data Seeding Solution

This document explains the solution for test failures caused by missing seed data after moving to a new database.

## Problem Analysis

The E2E test failures were caused by missing essential data in the new database:

### Root Causes Identified:
1. **Missing Identity Server Users**: Required test users (admin, pm, tester, viewer, developer) were missing from the Identity Server database
2. **Incorrect Static Projects**: The static projects expected by tests didn't match what was seeded in the database
3. **Database Migration Issues**: User seeding wasn't triggered properly during the database migration

### Test Failure Symptoms:
- 29 out of 123 tests failing (76% pass rate)
- Primary error: "Login failed for pm@rqmtmgmt.local. Errors: Invalid username or password"
- Missing user accounts detected: admin, pm, tester, viewer, developer
- Some tests expecting specific project names that didn't exist

## Solution Implementation

### 1. Enhanced TestDataSeeder Class

**Location**: `frontend.E2ETests/TestData/TestDataSeeder.cs`

**New Features**:
- `SeedRequiredTestProjectsAsync()`: Creates the exact static projects that tests expect
- Enhanced project seeding matching `TestDataFactory.GetStaticProject()` definitions
- Better error handling and reporting

### 2. Updated Backend DatabaseSeeder

**Location**: `backend/Data/DatabaseSeeder.cs`

**Changes**:
- Fixed static project names to match what E2E tests expect:
  - "Performance Test Project 638944753472742469"
  - "Requirements Test Project 638944753477280014" 
  - "Test Project"
- Ensures projects match `TestDataFactory.GetStaticProject()` exactly

### 3. New Diagnostic Test Suite

**Location**: `frontend.E2ETests/Workflows/DataSeedingDiagnosticTests.cs`

**Test Methods**:
- `DataSeeding_VerifyRequiredProjects_ReportsStatus`: Checks if required static projects exist
- `DataSeeding_SeedRequiredProjects_CreatesStaticProjects`: Seeds missing projects
- `DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication`: Verifies user authentication
- `DataSeeding_ShowRecoveryInstructions_DisplaysActions`: Shows detailed recovery steps

### 4. Automated Fix Script

**Location**: `scripts/fix-e2e-data.sh`

**Commands**:
- `verify-projects`: Check if required static projects exist
- `seed-projects`: Create missing static projects
- `verify-users`: Check if Identity Server users can authenticate
- `restart-services`: Restart pods to trigger user seeding
- `full-diagnosis`: Run complete diagnosis
- `fix-all`: Attempt to fix all missing data automatically

## How to Use the Solution

### Quick Fix (Recommended)
```bash
# Run complete diagnosis and fix
./scripts/fix-e2e-data.sh fix-all
```

### Step-by-Step Diagnosis
```bash
# 1. Diagnose what's missing
./scripts/fix-e2e-data.sh full-diagnosis

# 2. Fix missing projects
./scripts/fix-e2e-data.sh seed-projects

# 3. Fix missing users (restart services to trigger seeding)
./scripts/fix-e2e-data.sh restart-services

# 4. Verify everything is working
./scripts/fix-e2e-data.sh verify-projects
./scripts/fix-e2e-data.sh verify-users
```

### Manual Testing
```bash
# Run individual diagnostic tests
dotnet test frontend.E2ETests --filter "Name~DataSeeding_VerifyRequiredProjects_ReportsStatus"
dotnet test frontend.E2ETests --filter "Name~DataSeeding_SeedRequiredProjects_CreatesStaticProjects"
dotnet test frontend.E2ETests --filter "Name~DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication"
```

## Technical Details

### Required Test Users
The following users must exist in both Identity Server and the application database:

| Key | Email | Password | Role |
|-----|-------|----------|------|
| admin | admin@rqmtmgmt.local | Admin123! | Administrator |
| pm | pm@rqmtmgmt.local | Pm123! | ProjectManager |
| tester | tester@rqmtmgmt.local | Test123! | Tester |
| viewer | viewer@rqmtmgmt.local | View123! | Viewer |
| developer | dev@rqmtmgmt.local | Dev123! | Developer |

### Required Static Projects
These projects must exist and match `TestDataFactory.GetStaticProject()`:

| Index | Name | Code | Description |
|-------|------|------|-------------|
| 0 | Legacy Requirements | LEG | Default project for existing requirements |
| 1 | Performance Test Project 638944753472742469 | PTP3474 | Performance testing scenarios |
| 2 | Requirements Test Project 638944753477280014 | RTP198 | Requirements testing workflows |
| 3 | Test Project | TST | General purpose test project |

### Authentication Flow
The E2E tests require:
1. **Identity Server Users**: For authentication and JWT token generation
2. **Application Database Users**: For API operations and role-based access control
3. **Matching Email/Role Data**: Between both systems for proper user context

### Seeding Process
1. **Identity Server**: Automatically seeds users on startup via `SeedUsersAsync()` in `Program.cs`
2. **Backend Database**: Seeds users and projects via `DatabaseSeeder.SeedAsync()`
3. **E2E Test Seeder**: Can create missing projects and application users via API calls

## Troubleshooting

### If Users Still Can't Authenticate
```bash
# Check Identity Server logs
kubectl logs -l app=identityserver

# Restart Identity Server to trigger user seeding
kubectl delete pod -l app=identityserver
```

### If Projects Are Still Missing
```bash
# Run project seeding directly
dotnet test frontend.E2ETests --filter "Name~DataSeeding_SeedRequiredProjects_CreatesStaticProjects"

# Check backend API
curl https://rqmtmgmt.local/api/project
```

### If Tests Continue to Fail
1. Verify services are running: `kubectl get pods`
2. Check service logs: `kubectl logs -l app=backend` and `kubectl logs -l app=identityserver`
3. Run diagnostic tests to identify specific issues
4. Ensure database migrations completed successfully

## Prevention

To prevent this issue in the future:
1. Always run `./scripts/fix-e2e-data.sh full-diagnosis` after database changes
2. Include data seeding verification in CI/CD pipelines
3. Document any changes to required test data
4. Keep `TestDataFactory.GetStaticProject()` in sync with `DatabaseSeeder` static projects

## Validation

After applying the fix, you should see:
- ✅ All required projects exist
- ✅ All required users can authenticate  
- ✅ E2E test pass rate improves significantly (should be >95%)
- ✅ No more "Invalid username or password" errors in test logs