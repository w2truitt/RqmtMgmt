# Document Structure Visualization

## Current Structure (Flat)

```
📄 Document: TestFlow Pro Backend Server SRS (ID: 11)
│
├── 📁 Section 1: "3.1 Authentication and Authorization"
│   ├── 📋 REQ-AUTH-001: JWT Bearer Token Authentication
│   ├── 📋 REQ-AUTH-002: OAuth 2.0/OpenID Connect Integration
│   ├── 📋 REQ-AUTH-003: JWT Token Validation
│   ├── 📋 REQ-AUTH-004: Authority and Audience Validation
│   ├── 📋 REQ-AUTH-005: Role-Based Access Control (RBAC)
│   ├── 📋 REQ-AUTH-006: System Roles Support
│   ├── 📋 REQ-AUTH-007: Endpoint Access Control
│   └── 📋 REQ-AUTH-008: Project Role Assignments
│
├── 📁 Section 2: "3.2 User Management"
│   ├── 📋 REQ-USER-001: CRUD Operations for Users
│   ├── 📋 REQ-USER-002: User Profile Storage
│   ├── 📋 REQ-USER-003: Role Assignment
│   ├── 📋 REQ-USER-004: User Search and Filtering
│   ├── 📋 REQ-USER-005: Activity Tracking
│   ├── 📋 REQ-USER-006: Role Definitions Management
│   ├── 📋 REQ-USER-007: Hierarchical Role Inheritance
│   └── 📋 REQ-USER-008: Role-Based Data Restrictions
│
└── 📁 Section 3: "3.3 API Design and RESTful Services"
    ├── 📋 REQ-API-001: RESTful API Endpoints
    ├── 📋 REQ-API-002: OpenAPI/Swagger Documentation
    └── ... (more requirements)
```

**Issues:**
- ❌ Lost logical grouping from original SRS (3.1.1 vs 3.1.2)
- ❌ All 8 auth requirements appear at same level
- ❌ Can't tell which requirements are about Authentication vs Authorization
- ❌ Difficult to navigate in large documents

---

## Proposed Structure (Hierarchical)

```
📄 Document: TestFlow Pro Backend Server SRS (ID: 11)
│
├── 📁 Section 1: "3. Functional Requirements" (Level 1)
│   │
│   ├── 📂 Section 1.1: "3.1 Authentication and Authorization" (Level 2)
│   │   │
│   │   ├── 📂 Section 1.1.1: "3.1.1 User Authentication" (Level 3)
│   │   │   ├── 📋 REQ-AUTH-001: JWT Bearer Token Authentication
│   │   │   ├── 📋 REQ-AUTH-002: OAuth 2.0/OpenID Connect Integration
│   │   │   ├── 📋 REQ-AUTH-003: JWT Token Validation
│   │   │   └── 📋 REQ-AUTH-004: Authority and Audience Validation
│   │   │
│   │   └── 📂 Section 1.1.2: "3.1.2 Authorization" (Level 3)
│   │       ├── 📋 REQ-AUTH-005: Role-Based Access Control (RBAC)
│   │       ├── 📋 REQ-AUTH-006: System Roles Support
│   │       ├── 📋 REQ-AUTH-007: Endpoint Access Control
│   │       └── 📋 REQ-AUTH-008: Project Role Assignments
│   │
│   ├── 📂 Section 1.2: "3.2 User Management" (Level 2)
│   │   │
│   │   ├── 📂 Section 1.2.1: "3.2.1 User Operations" (Level 3)
│   │   │   ├── 📋 REQ-USER-001: CRUD Operations for Users
│   │   │   ├── 📋 REQ-USER-002: User Profile Storage
│   │   │   ├── 📋 REQ-USER-003: Role Assignment
│   │   │   ├── 📋 REQ-USER-004: User Search and Filtering
│   │   │   └── 📋 REQ-USER-005: Activity Tracking
│   │   │
│   │   └── 📂 Section 1.2.2: "3.2.2 User Roles" (Level 3)
│   │       ├── 📋 REQ-USER-006: Role Definitions Management
│   │       ├── 📋 REQ-USER-007: Hierarchical Role Inheritance
│   │       └── 📋 REQ-USER-008: Role-Based Data Restrictions
│   │
│   └── 📂 Section 1.3: "3.3 API Design and RESTful Services" (Level 2)
│       ├── 📋 REQ-API-001: RESTful API Endpoints
│       ├── 📋 REQ-API-002: OpenAPI/Swagger Documentation
│       └── ... (more requirements)
│
├── 📁 Section 2: "4. Non-Functional Requirements" (Level 1)
│   │
│   ├── 📂 Section 2.1: "4.1 Performance Requirements" (Level 2)
│   │   ├── 📋 REQ-PERF-001: API Response Time
│   │   └── ... (more requirements)
│   │
│   └── 📂 Section 2.2: "4.2 Scalability Requirements" (Level 2)
│       ├── 📋 REQ-SCALE-001: Horizontal Scaling
│       └── ... (more requirements)
│
└── 📁 Section 3: "5. System Interfaces" (Level 1)
    ├── 📂 Section 3.1: "5.1 User Interfaces" (Level 2)
    └── ... (more subsections)
```

**Benefits:**
- ✅ Perfect 1:1 mapping with source SRS document
- ✅ Clear logical grouping (Authentication separate from Authorization)
- ✅ Collapsible sections for easier navigation
- ✅ Preserves document author's intended structure
- ✅ Automatic numbering (3.1.1, 3.1.2, etc.)

---

## Database Relationships

### Current (Flat)

```
┌─────────────────┐
│   Document      │
│   ID: 11        │
│   Title: SRS    │
└────────┬────────┘
         │
         │ 1:N (DocumentId)
         ▼
┌─────────────────┐
│ DocumentSection │
│ ID: 1           │
│ DocumentId: 11  │◄─────┐
│ Title: "3.1..." │      │
└────────┬────────┘      │
         │               │ Same level,
         │ 1:N           │ no hierarchy
         ▼               │
┌─────────────────┐      │
│  Requirement    │      │
│  ID: 7          │      │
│  SectionId: 1   │      │
│  Title: REQ-... │      │
└─────────────────┘      │
                         │
┌─────────────────┐      │
│ DocumentSection │      │
│ ID: 2           │      │
│ DocumentId: 11  │◄─────┘
│ Title: "3.2..." │
└────────┬────────┘
         │
         ▼
    (Requirements)
```

### Proposed (Hierarchical)

```
┌─────────────────┐
│   Document      │
│   ID: 11        │
│   Title: SRS    │
└────────┬────────┘
         │
         │ 1:N (DocumentId for root sections)
         ▼
┌─────────────────────────────┐
│ DocumentSection (Level 1)   │
│ ID: 1                       │
│ DocumentId: 11              │
│ ParentSectionId: NULL       │◄───────────┐
│ Level: 1                    │            │
│ Title: "3. Functional..."   │            │
└────────┬────────────────────┘            │
         │                                  │
         │ 1:N (ParentSectionId)           │ Self-
         ▼                                  │ referencing
┌─────────────────────────────┐            │ foreign key
│ DocumentSection (Level 2)   │            │
│ ID: 2                       │            │
│ DocumentId: NULL            │            │
│ ParentSectionId: 1          │────────────┘
│ Level: 2                    │
│ Title: "3.1 Auth..."        │
└────────┬────────────────────┘
         │
         │ 1:N (ParentSectionId)
         ▼
┌─────────────────────────────┐
│ DocumentSection (Level 3)   │
│ ID: 3                       │
│ DocumentId: NULL            │
│ ParentSectionId: 2          │────────────┐
│ Level: 3                    │            │
│ Title: "3.1.1 User Auth"    │            │
└────────┬────────────────────┘            │
         │                                  │
         │ 1:N (SectionId)                 │
         ▼                                  │
┌─────────────────────────────┐            │
│      Requirement            │            │
│      ID: 7                  │            │
│      SectionId: 3           │            │
│      Title: "REQ-AUTH-001"  │            │
└─────────────────────────────┘            │
                                            │
┌─────────────────────────────┐            │
│ DocumentSection (Level 3)   │            │
│ ID: 4                       │            │
│ DocumentId: NULL            │            │
│ ParentSectionId: 2          │────────────┘
│ Level: 3                    │
│ Title: "3.1.2 Authorization"│
└────────┬────────────────────┘
         │
         ▼
    (Requirements)
```

---

## UI Mockup Comparison

### Current UI (Flat List)

```
┌─────────────────────────────────────────────────────────┐
│ Document Sections                          [+ Add Section]│
├─────────────────────────────────────────────────────────┤
│                                                           │
│ 1. 3.1 Authentication and Authorization                  │
│    Requirements for user authentication and RBAC         │
│    [8 requirements] [↑] [↓] [Edit] [Delete]             │
│                                                           │
│ 2. 3.2 User Management                                   │
│    Requirements for user account operations              │
│    [8 requirements] [↑] [↓] [Edit] [Delete]             │
│                                                           │
│ 3. 3.3 API Design and RESTful Services                   │
│    Requirements for RESTful API endpoints                │
│    [15 requirements] [↑] [↓] [Edit] [Delete]            │
│                                                           │
└─────────────────────────────────────────────────────────┘
```

### Proposed UI (Tree View)

```
┌─────────────────────────────────────────────────────────┐
│ Document Sections                          [+ Add Section]│
├─────────────────────────────────────────────────────────┤
│                                                           │
│ ▼ 1. 3. Functional Requirements                          │
│   └─► Major functional areas                  [Edit] [+] │
│                                                           │
│   ▼ 1.1. 3.1 Authentication and Authorization            │
│     └─► Requirements for user auth and RBAC   [Edit] [+] │
│                                                           │
│       ▶ 1.1.1. 3.1.1 User Authentication                 │
│         └─► JWT and OAuth integration  [4 req] [Actions] │
│                                                           │
│       ▼ 1.1.2. 3.1.2 Authorization                       │
│         └─► RBAC and access control           [Edit] [+] │
│           │                                               │
│           ├─ 📋 REQ-AUTH-005: RBAC implementation        │
│           ├─ 📋 REQ-AUTH-006: System roles               │
│           ├─ 📋 REQ-AUTH-007: Endpoint access            │
│           └─ 📋 REQ-AUTH-008: Project roles              │
│                                                           │
│   ▶ 1.2. 3.2 User Management                             │
│     └─► User account operations             [8 req] [...] │
│                                                           │
│   ▶ 1.3. 3.3 API Design and RESTful Services             │
│     └─► RESTful endpoints and services     [15 req] [...] │
│                                                           │
│ ▶ 2. 4. Non-Functional Requirements                      │
│   └─► Performance, security, scalability      [Edit] [+] │
│                                                           │
└─────────────────────────────────────────────────────────┘

Legend:
  ▼ = Expanded section
  ▶ = Collapsed section
  [+] = Add subsection
  [Actions] = Move/Edit/Delete menu
```

---

## Navigation Improvements

### Current: Click through 8 requirements to find what you want
```
Section: 3.1 Authentication and Authorization
  ├─ REQ-AUTH-001 (JWT) ← Is this authentication or authorization?
  ├─ REQ-AUTH-002 (OAuth) ← Authentication
  ├─ REQ-AUTH-003 (Validation) ← Authentication
  ├─ REQ-AUTH-004 (Config) ← Authentication
  ├─ REQ-AUTH-005 (RBAC) ← Authorization! (Found it after 5 tries)
  ├─ REQ-AUTH-006 (Roles)
  ├─ REQ-AUTH-007 (Access)
  └─ REQ-AUTH-008 (Project roles)
```

### Proposed: Navigate directly to the right subsection
```
Section: 3.1 Authentication and Authorization
  ├─ 3.1.1 User Authentication ← Click to see only these 4
  │   ├─ REQ-AUTH-001 (JWT)
  │   ├─ REQ-AUTH-002 (OAuth)
  │   ├─ REQ-AUTH-003 (Validation)
  │   └─ REQ-AUTH-004 (Config)
  │
  └─ 3.1.2 Authorization ← Click to see only these 4
      ├─ REQ-AUTH-005 (RBAC) ← Found immediately!
      ├─ REQ-AUTH-006 (Roles)
      ├─ REQ-AUTH-007 (Access)
      └─ REQ-AUTH-008 (Project roles)
```

---

## Implementation Example

### Sample API Call to Create Hierarchy

```json
POST /api/DocumentSections
{
  "documentId": 11,
  "parentSectionId": null,
  "title": "3. Functional Requirements",
  "description": "Major functional areas of the system",
  "level": 1,
  "sectionNumber": "3",
  "sectionOrder": 3
}
→ Returns: { "id": 1, ... }

POST /api/DocumentSections
{
  "documentId": null,
  "parentSectionId": 1,
  "title": "3.1 Authentication and Authorization",
  "description": "Requirements for user authentication and RBAC",
  "level": 2,
  "sectionNumber": "3.1",
  "sectionOrder": 1
}
→ Returns: { "id": 2, ... }

POST /api/DocumentSections
{
  "documentId": null,
  "parentSectionId": 2,
  "title": "3.1.1 User Authentication",
  "description": "JWT and OAuth integration requirements",
  "level": 3,
  "sectionNumber": "3.1.1",
  "sectionOrder": 1
}
→ Returns: { "id": 3, ... }

POST /api/Requirement
{
  "projectId": 25,
  "documentId": 11,
  "sectionId": 3,  ← Links to leaf subsection
  "title": "REQ-AUTH-001: JWT Bearer Token Authentication",
  ...
}
```

---

## Query Examples

### Get Full Section Hierarchy

```csharp
// Recursive CTE query
public async Task<List<DocumentSectionDto>> GetSectionHierarchyAsync(int documentId)
{
    var query = @"
        WITH RECURSIVE SectionTree AS (
            -- Root sections
            SELECT Id, DocumentId, ParentSectionId, Title, Level, SectionNumber, SectionOrder
            FROM DocumentSections
            WHERE DocumentId = @DocumentId AND ParentSectionId IS NULL
            
            UNION ALL
            
            -- Child sections
            SELECT s.Id, s.DocumentId, s.ParentSectionId, s.Title, s.Level, s.SectionNumber, s.SectionOrder
            FROM DocumentSections s
            INNER JOIN SectionTree st ON s.ParentSectionId = st.Id
        )
        SELECT * FROM SectionTree
        ORDER BY SectionNumber;
    ";
    
    return await _context.Database
        .SqlQueryRaw<DocumentSectionDto>(query, new { DocumentId = documentId })
        .ToListAsync();
}
```

### Get Section with All Requirements

```csharp
public async Task<DocumentSectionDto> GetSectionWithRequirementsAsync(int sectionId)
{
    return await _context.DocumentSections
        .Include(s => s.ChildSections)
            .ThenInclude(cs => cs.Requirements)
        .Include(s => s.Requirements)
        .FirstOrDefaultAsync(s => s.Id == sectionId);
}
```

---

## Conclusion

The hierarchical subsections feature would **transform** the document from a flat list of 3 sections into a properly structured document with 31 logical groupings, matching the original SRS intent and providing a much better user experience for navigation and organization.
