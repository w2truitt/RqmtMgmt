# Document Duplicate Cleanup Guide

This guide provides tools and procedures to clean up duplicate sections and requirements in **Project 25 Document 11** (TestFlow Pro SRS).

## Problem Summary

Based on analysis from the comprehensive review reports, the document suffers from extensive duplication:

- **29 duplicate section groups** with identical names and content
- **175 duplicate requirement groups** (49% of all requirements are duplicates)
- **Document inflation**: From 67 sections/176 requirements to 96 sections/355 requirements

### Key Duplicate Examples

**Duplicate Sections:**
- Project Operations (IDs: 1335, 1402)
- User Management (IDs: 1434, 3242)
- Test Suite Management (IDs: 1344, 1411)
- API Design (IDs: 1352, 1419)

**Duplicate Requirements:**
- "The system SHALL support JWT bearer token authentication" (appears 4 times)
- "The system SHALL provide CRUD operations for user accounts" (appears 2 times)

## Cleanup Tools

### 1. Main Cleanup Script: `cleanup_document_duplicates.py`

**Purpose**: Systematically removes duplicate sections and requirements while preserving data integrity.

**Features**:
- Identifies duplicates by title/description matching
- Consolidates sections by keeping the first instance (lowest ID)
- Moves requirements from duplicate sections to primary sections
- Removes duplicate requirements with identical descriptions
- Provides detailed progress reporting and statistics

**Safety Features**:
- Dry-run mode for safe preview
- Comprehensive error handling
- API rate limiting to avoid overwhelming the server
- Detailed logging of all operations

### 2. Cleanup Runner: `run_document_cleanup.sh`

**Purpose**: User-friendly wrapper script with safety checks and authentication handling.

**Features**:
- Interactive confirmations for destructive operations
- Environment variable support for authentication
- Dependency checking (Python, requests library)
- Colored output for better visibility
- Built-in help and usage information

### 3. Verification Script: `verify_cleanup_results.py`

**Purpose**: Analyzes document structure after cleanup to verify success.

**Features**:
- Compares actual vs expected counts
- Detects remaining duplicates
- Analyzes section hierarchy integrity
- Provides overall assessment and recommendations

## Usage Instructions

### Prerequisites

1. **Python 3** with `requests` library
2. **Authentication token** (JWT) for API access
3. **Network access** to the Requirements Management API

### Step 1: Preview Changes (Dry Run)

Always start with a dry run to see what changes will be made:

```bash
# Using environment variable for token
export JWT_TOKEN="your_jwt_token_here"
./run_document_cleanup.sh --dry-run

# Or provide token directly
./run_document_cleanup.sh --dry-run --token "your_jwt_token_here"
```

### Step 2: Execute Cleanup

After reviewing the dry run results, execute the actual cleanup:

```bash
# Using environment variable
export JWT_TOKEN="your_jwt_token_here"
./run_document_cleanup.sh

# Or provide token directly
./run_document_cleanup.sh --token "your_jwt_token_here"
```

### Step 3: Verify Results

After cleanup, verify the results:

```bash
python3 verify_cleanup_results.py --token "your_jwt_token_here"
```

## Expected Results

After successful cleanup, the document should have:

- **67 sections** (down from 96)
- **176 requirements** (down from 355)
- **No duplicate sections** with identical titles
- **No duplicate requirements** with identical descriptions
- **Proper section hierarchy** with all requirements correctly associated

## Safety Considerations

### Before Running Cleanup

1. **Backup the database** - This process permanently deletes data
2. **Review dry run output** - Understand exactly what will be changed
3. **Verify authentication** - Ensure you have proper API access
4. **Check system load** - Avoid running during peak usage times

### During Cleanup

1. **Monitor progress** - The script provides detailed logging
2. **Don't interrupt** - Let the process complete to avoid partial state
3. **Check for errors** - Address any API failures immediately

### After Cleanup

1. **Run verification** - Use the verification script to check results
2. **Test functionality** - Verify the UI and API work correctly
3. **Review manually** - Spot-check a few sections and requirements
4. **Run system tests** - Ensure no functionality was broken

## Troubleshooting

### Common Issues

**Authentication Failures**:
```
Error: 401 Unauthorized
```
- Solution: Check JWT token validity and permissions

**API Rate Limiting**:
```
Error: 429 Too Many Requests
```
- Solution: The script includes delays; wait and retry if needed

**Partial Cleanup**:
```
Some sections/requirements not removed
```
- Solution: Re-run the cleanup script; it's safe to run multiple times

**Count Mismatches**:
```
Final counts don't match expected values
```
- Solution: Manual review needed; some duplicates may have different criteria

### Recovery Procedures

If cleanup fails partway through:

1. **Check verification output** to understand current state
2. **Re-run cleanup script** - it's designed to be idempotent
3. **Manual cleanup** for remaining issues using the UI or direct API calls
4. **Database restore** if major issues occur (requires backup)

## API Endpoints Used

The cleanup process uses these API endpoints:

- `GET /api/DocumentSections/document/{documentId}` - Fetch sections
- `GET /api/Requirement/document/{documentId}` - Fetch requirements
- `GET /api/Requirement/section/{sectionId}` - Get section requirements
- `PUT /api/Requirement/{id}` - Update requirement associations
- `DELETE /api/DocumentSections/{id}` - Remove duplicate sections
- `DELETE /api/Requirement/{id}` - Remove duplicate requirements

## Technical Details

### Duplicate Detection Logic

**Sections**: Duplicates identified by identical `title` field
**Requirements**: Duplicates identified by identical `description` field

### Consolidation Strategy

1. **Sort by ID**: Keep the first created (lowest ID) as primary
2. **Move dependencies**: Transfer requirements from duplicates to primary
3. **Update associations**: Ensure all references point to primary
4. **Remove duplicates**: Delete secondary instances safely

### Data Integrity

- Requirements are moved before sections are deleted
- All operations are atomic at the API level
- Foreign key relationships are preserved
- Section hierarchy is maintained

## Files Overview

```
cleanup_document_duplicates.py  # Main cleanup logic
run_document_cleanup.sh        # User-friendly wrapper
verify_cleanup_results.py      # Post-cleanup verification
DOCUMENT_CLEANUP_README.md     # This documentation
```

## Support

For issues or questions:

1. Check the verification script output for specific problems
2. Review the API logs for detailed error information
3. Consult the comprehensive review reports for context
4. Test with dry-run mode to understand behavior

## Success Criteria

The cleanup is considered successful when:

- ✅ Section count equals 67
- ✅ Requirements count equals 176  
- ✅ No duplicate sections remain
- ✅ No duplicate requirements remain
- ✅ All requirements are properly associated with sections
- ✅ Section hierarchy is intact
- ✅ UI functionality works correctly
- ✅ API tests pass