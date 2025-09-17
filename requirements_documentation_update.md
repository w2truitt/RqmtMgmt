# Requirements Management Refactor: Document Templates, Schema, and Implementation Plan

## Document Templates (Markdown)

### 1. Customer Requirement Document (CRD)

```
# Customer Requirement Document (CRD)

## 1. Overview
- Document Title:
- Product/Project Name:
- Document Owner:
- Date:
- Version:

## 2. Objective
- Clearly state the business goals and customer needs this document addresses.

## 3. Background & Context
- Relevant background, problem statement, and context.

## 4. Scope
- In Scope:
- Out of Scope:

## 5. Customer Requirements
For each requirement, include:
- CRD-XXX:
  - Title:
  - Description:
  - Priority:
  - Acceptance Criteria:
  - Status:
  - Linked Product Requirements (PRD): (list PRD IDs/titles, or "N/A")

## 6. User Personas / Stakeholders
- Key user groups, stakeholders, or customers.

## 7. Success Criteria
- Measurable customer-focused criteria for success.

## 8. Dependencies
- Related systems, teams, or external dependencies.

## 9. Assumptions & Constraints
- Document assumptions, constraints, or risks.

## 10. Timeline / Milestones
- Estimated timing or key milestones.

## 11. Appendix
- Supporting materials, references, diagrams, or links.
```

---

### 2. Product Requirement Document (PRD)

```
# Product Requirement Document (PRD)

## 1. Overview
- Product Name:
- Document Owner:
- Date:
- Version:

## 2. Objective

## 3. Background & Context

## 4. Scope
- In Scope:
- Out of Scope:

## 5. Requirements
[List of PRD requirements, each with CRD trace links]

## 6. User Stories / Use Cases

## 7. Success Metrics

## 8. Dependencies

## 9. Assumptions & Constraints

## 10. Timeline

## 11. Appendix

## 12. Traceability Table
| Customer Requirement (CRD) | Linked Product Requirements (PRD Tags/IDs) |
|----------------------------|---------------------------------------------|
| CRD-001                    | PRD-101, PRD-102                          |
| CRD-002                    | PRD-103                                   |
```

---

### 3. Software Requirement Specification (SRS)

```
# Software Requirement Specification (SRS)

## 1. Overview
- Document Title:
- Product/Project Name:
- Document Owner:
- Date:
- Version:

## 2. Objective
- Purpose and goals of the software specification.

## 3. Background & Context
- Technical and business context for the software requirements.

## 4. Scope
- In Scope:
- Out of Scope:

## 5. Software Requirements
For each requirement, include:
- SRS-XXX:
  - Title:
  - Description:
  - Priority:
  - Acceptance Criteria:
  - Status:
  - Linked Product Requirements (PRD): (list PRD IDs/titles, or "N/A")

## 6. Functional Requirements
- Detailed description of functional capabilities.

## 7. Non-Functional Requirements
- Performance, security, scalability, usability, etc.

## 8. User Stories / Use Cases
- Key scenarios or workflows.

## 9. Success Metrics
- How software success will be measured.

## 10. Dependencies
- Related systems, teams, or external dependencies.

## 11. Assumptions & Constraints
- Document assumptions, constraints, or risks.

## 12. Timeline / Milestones
- Estimated timing or key milestones.

## 13. Appendix
- Supporting materials, references, diagrams, or links.
```

---

## Requirement Class Changes

- Remove document-level metadata from `RequirementDto` (e.g., Objective, Background, Scope).
- Add foreign keys: `DocumentId`, `SectionId` to link requirements to their parent document and section.
- Requirements will trace up to parent requirements (CRD for PRD, PRD for SRS) using a `RequirementTrace` table.
- Maintain `RequirementVersion` tracking for change history, including design decision timestamps and rationale.

---

## ✅ Implementation Status (Updated: September 16, 2025)

### **BACKEND IMPLEMENTATION: COMPLETE** ✅

1. **✅ Define Document Templates** - COMPLETE
    - ✅ Finalized markdown templates for CRD, PRD, SRS with all required/optional sections
    - ✅ Document templates integrated into backend API structure

2. **✅ Schema and DTO Refactor** - COMPLETE  
    - ✅ Created new EntityFramework models/tables:
        - ✅ `Document` (CRD, PRD, SRS) with full metadata support
        - ✅ `DocumentSection` with ordering and N/A support
        - ✅ Updated `Requirement` to link to document/section via DocumentId/SectionId
        - ✅ `RequirementTrace` for requirement hierarchy and traceability
    - ✅ Updated DTOs in RqmtMgmtShared v1.0.32
    - ✅ Migration scripts created and applied: `DocumentCentricRefactorFixed`

3. **✅ Backend API Refactor** - COMPLETE
    - ✅ Created new endpoints for documents, sections, and requirement traceability:
        - ✅ `DocumentsController` - Full REST API for document management
        - ✅ `DocumentSectionsController` - Section management with reordering
        - ✅ `RequirementTracesController` - Traceability management with validation
    - ✅ Updated `RequirementController` with document/section relationship support
    - ✅ All APIs support requirement versioning and change tracking
    - ✅ **180 API integration tests passing** (100% success rate)

4. **🔄 Frontend UI Refactor** - IN PROGRESS
    - 📋 **Phase 1**: Service layer integration (DocumentService, DocumentSectionService, etc.)
    - 📋 **Phase 2**: Document management pages (Create, Edit, View CRD/PRD/SRS)
    - 📋 **Phase 3**: Section management and requirement integration
    - 📋 **Phase 4**: Traceability visualization and advanced features
    - 📋 **Phase 5**: Navigation integration and UX optimization
    - 📋 Support marking document sections as N/A if not applicable
    - 📋 Update traceability views for PRD and SRS relationships

5. **✅ Test Updates** - COMPLETE (Backend)
    - ✅ **638 unit tests passing** (100% success rate) for all backend services and controllers
    - ✅ **180 API integration tests passing** for all endpoints including new document APIs
    - ✅ Updated tests for DTOs, models, and migration logic
    - ✅ New tests for document, section, and traceability endpoints
    - 📋 Frontend component and E2E tests - pending frontend implementation

6. **✅ Migration and Data Management** - COMPLETE
    - ✅ Database migration applied successfully: `DocumentCentricRefactorFixed`
    - ✅ Updated seeders to use CRD/PRD instead of CRS/PRS
    - ✅ All foreign key relationships and indexes configured
    - ✅ Backward compatibility maintained for existing requirements

7. **📋 Documentation and Training** - PENDING
    - 📋 Update developer and user documentation for new document-centric workflow
    - 📋 Create training materials and user guides
    - 📋 API documentation (auto-generated via Swagger)

### **🎯 CURRENT STATUS**

**Backend: Production Ready** ✅
- All APIs implemented and tested (180/180 integration tests passing)
- Database schema applied and validated
- Full backward compatibility maintained
- Ready for docker-compose deployment

**Frontend: Ready for Implementation** 🔄
- Backend APIs provide all required functionality
- Comprehensive frontend implementation plan created (see `frontend-work.md`)
- Service layer integration can begin immediately
- Phased approach ensures incremental progress

**Next Priority: Frontend Phase 1 - Service Layer Integration**
- Create DocumentService, DocumentSectionService, RequirementTraceService
- Update RequirementService with document context support
- Implement frontend DTOs matching backend APIs

---

## Notes on Requirement Tracking

- Continue to track requirement updates, versioning, and design decisions via `RequirementVersion` or similar mechanism.
- Ensure traceability between requirements and documents is clear and queryable.
- All requirements should have a parent document and section for reporting and document generation.

---

This document serves as the implementation guide for the requirements management refactor.