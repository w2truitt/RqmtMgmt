# Requirements Population Scripts

This directory contains Python scripts to populate requirements from the `SOFTWARE_REQUIREMENTS_SPECIFICATION.md` file into the RqmtMgmt system via REST API.

## Scripts

### 1. `populate_srs_demo.py` (Recommended for Demo)

A simplified script that populates requirements for the 3 existing document sections:
- 3.1 Authentication and Authorization (8 requirements)
- 3.2 User Management (8 requirements)  
- 3.3 API Design and RESTful Services (15 requirements)

**Total: 24 requirements**

### 2. `populate_srs_requirements.py` (Full Script)

A comprehensive script that parses and can populate all 176 requirements from the SRS document across all sections. This would require creating additional document sections first.

## Prerequisites

- Python 3.x
- `requests` library: `pip install requests`
- Active RqmtMgmt system running (https://rqmtmgmt.local)
- Valid authentication token

## Getting an Authentication Token

The scripts require a valid JWT bearer token from an authenticated browser session:

1. **Open your browser** and navigate to https://rqmtmgmt.local
2. **Log in** as an administrator (admin@rqmtmgmt.local / Admin123!)
3. **Open Browser Developer Tools**:
   - Chrome/Edge: Press F12 or Ctrl+Shift+I
   - Firefox: Press F12
4. **Navigate to Application/Storage tab**
5. **Find Session Storage** → `https://rqmtmgmt.local`
6. **Locate the key** `oidc.user:https://rqmtmgmt.local:rqmtmgmt-frontend`
7. **Copy the `access_token` value** from the JSON object

**Note:** Tokens expire after 1 hour. If you get a 401 error, you need to get a fresh token.

## Usage

### Demo Script (Recommended)

```bash
# Run the demo script with your access token
python3 populate_srs_demo.py "<YOUR_ACCESS_TOKEN>"
```

**Example Output:**
```
======================================================================
RqmtMgmt Requirements Population - Demo Script
======================================================================

Parsing SRS document...
Found 24 requirements across 3 sections:
  - 3.1 Authentication and Authorization: 8 requirements
  - 3.2 User Management: 8 requirements
  - 3.3 API Design and RESTful Services: 8 requirements

Creating requirements...
----------------------------------------------------------------------

[3.1 Authentication and Authorization] (Section ID: 1)
  ✓ REQ-AUTH-001: The system SHALL support JWT bearer token authentication
  ✓ REQ-AUTH-002: The system SHALL integrate with OAuth 2.0/OpenID Connect providers...
  ...

======================================================================
COMPLETED: 24 created, 0 failed
======================================================================
```

### Full Script

```bash
# Run the full script (requires sections to be created first)
python3 populate_srs_requirements.py "<YOUR_ACCESS_TOKEN>"
```

## Verifying Results

After running the scripts, verify the requirements were created:

1. Navigate to https://rqmtmgmt.local/projects/25/documents/11
2. You should see the requirements listed under each section
3. Check the requirements count at the top of the document
4. View the traceability matrix to see all requirements

## Troubleshooting

### 401 Unauthorized Error
- **Cause:** Token has expired (tokens last 1 hour)
- **Solution:** Get a fresh token from the browser and run the script again

### Connection Refused
- **Cause:** RqmtMgmt system is not running
- **Solution:** Start the Kubernetes pods:
  ```bash
  kubectl get pods -n rqmtmgmt
  # Ensure all pods are Running
  ```

### SSL Certificate Error
- **Note:** The scripts use `verify=False` to skip SSL verification for local development
- This is normal for self-signed certificates in local/dev environments

### No Sections Found
- **Cause:** Document sections don't exist or section IDs don't match
- **Solution:** Use the UI to verify sections exist in document ID 11

## Architecture Details

### API Endpoints Used

- `POST /api/Requirement` - Creates a new requirement
- `GET /api/DocumentSections/document/{id}` - Gets sections for a document

### Requirement DTO Structure

```json
{
  "title": "REQ-AUTH-001: JWT Bearer Token Authentication",
  "description": "The system SHALL support JWT bearer token authentication",
  "type": 2,              // 0=CRD, 1=PRD, 2=SRS
  "status": 0,            // 0=Draft, 1=Approved, 2=Implemented, 3=Verified
  "projectId": 25,
  "documentId": 11,
  "sectionId": 1,
  "version": 1
}
```

## Future Enhancements

To populate all 176 requirements, you would need to:

1. Create additional document sections via API or UI
2. Update the `SECTION_MAPPING` dictionary in the scripts
3. Run the full `populate_srs_requirements.py` script

Alternatively, the scripts could be enhanced to automatically create missing sections.

## Notes

- The scripts skip the first requirement (REQ-AUTH-001) if it already exists (ID 7)
- Requirements are created with status "Draft"
- All requirements are associated with Project ID 25 (TestFlow Pro)
- Document ID 11 is the "TestFlow Pro Backend Server - Software Requirements Specification"

## Real-Life Test Benefits

Using these scripts to populate requirements serves as a real-life test of:

1. **REST API Functionality** - Tests the `/api/Requirement` POST endpoint
2. **Authentication & Authorization** - Validates JWT token handling
3. **Data Validation** - Tests requirement creation validation rules
4. **Document Structure** - Verifies document-section-requirement relationships
5. **Performance** - Tests bulk requirement creation
6. **Database Integrity** - Ensures referential integrity is maintained

This provides practical validation of the system beyond unit and integration tests.
