# Software Architecture Decisions for Requirements and Test Management Tool

## 1. Overview

This document outlines the software architecture decisions for developing a web-based requirements and test management tool. It covers the front-end and back-end technology choices, database selection, deployment considerations, and provides guidance for developers.

---


## 2. Front-End Architecture

- The front-end shall be a Single Page Application (SPA) accessible via modern web browsers (Chrome, Edge, Firefox, Safari).
- The front-end will be developed using Blazor WebAssembly for its seamless .NET integration and modern SPA capabilities.
- UI state management will use built-in .NET/Blazor patterns (e.g., cascading parameters, dependency injection, or Fluxor if needed) for scalability and maintainability.
- UI components will be based on MudBlazor (or another Blazor component library) for consistency and accessibility.
- The front-end will communicate with the back-end via RESTful APIs (JSON over HTTPS).
- Authentication will use OAuth 2.0 / OpenID Connect (OIDC) via a cloud identity provider (e.g., Azure AD, Okta).
- Responsive design will be ensured for desktop and tablet use.

```mermaid
flowchart LR
    Browser["Web Browser"] <--> Blazor["Blazor Frontend"]
    Blazor <--> API[".NET 8 API"]
    API --> SQL["Azure SQL DB"]
    API --> Blob["Blob Storage"]
```

---

## 3. Back-End Architecture

- The back-end will be developed using .NET 8 Web API for high performance, security, and cloud-native support.
- The back-end will expose RESTful endpoints for all major entities: CRS, PRS, SRS, Test Suites, Test Cases, Test Runs, Users, Roles.
- Authentication and authorization will be enforced at the API layer using JWT tokens from the identity provider.
- The back-end will be containerized using Docker for portability and scalability.
- Hosting will be in a cloud environment (e.g., Azure App Service, AWS ECS, or Google Cloud Run).
- The back-end will provide OpenAPI (Swagger) documentation for API consumers.

---

## 4. Database/Data Store Decision

- The system will use Microsoft Azure SQL Database as the primary data store for:
  - Enterprise-grade reliability, scalability, and security
  - ACID compliance and strong consistency for requirements and test data
  - Support for relational data, versioning, and complex queries
  - Integration with enterprise authentication and backup solutions
- Entity relationships (requirements, test cases, users, audit logs) will be modeled in normalized relational tables.
- Attachments (e.g., screenshots, logs) will be stored in Azure Blob Storage, referenced by URLs in the database.

```mermaid
flowchart TD
    SQLDB["Azure SQL Database\nTables:\n- Requirements\n- RequirementVersions\n- TestCases\n- TestCaseVersions\n- TestSuites\n- TestRuns\n- Users/Roles\n- AuditLogs"]
    Blob["Azure Blob Storage\nFor attachments, logs"]
    SQLDB --> Blob
```

---

## 5. Key Architectural Considerations

- All data access will be via repository/service patterns in the back-end.
- API versioning will be implemented to support future changes.
- The system will be designed for multi-tenancy and enterprise scalability.
- All communication will be over HTTPS.
- Audit logs will be maintained for compliance and traceability.
- Error handling and logging will use cloud-native monitoring (e.g., Azure Monitor).
- All requirements and test cases will be versioned in append-only version/history tables (e.g., RequirementVersions, TestCaseVersions), supporting comparison, rollback, and export of any historical version.
- The system will provide APIs and UI for generating redline documents that highlight the differences between any two versions or releases of requirements or test cases, supporting regulatory and quality audits.

---

## 6. Developer Guidance

- Follow SOLID and clean architecture principles in both front-end and back-end code.
- Ensure all business logic is in the back-end API, not in the front-end.
- Use environment variables for all secrets and configuration.
- Write automated unit and integration tests for all layers.
- Use CI/CD pipelines for build, test, and deployment (e.g., GitHub Actions, Azure DevOps).

---

## 7. Architectural Review & Recommendations (2025)

### Strategic Improvements (Critical & High Priority)

1. **Domain Model Relationships**
   - Add navigation properties to all models (e.g., TestSuite in TestCase, Requirement in RequirementLink) and configure them in EF Core for better maintainability, easier queries, and data integrity.
   - Use value objects or enums for fields like Status, Type, and Result to enforce valid values and improve code safety.

2. **Service/Repository Layer & Separation of Concerns**
   - Implement a true repository/service pattern, separating business logic from controllers and data access. This enables easier testing, future data store changes, and aligns with clean/hexagonal architecture.

3. **Security & Identity**
   - Integrate Azure AD/OIDC authentication and policy-based authorization as soon as possible.
   - Extend AuditLog to capture the authenticated user’s claims principal.
   - Start enforcing role-based access control (RBAC) in your API endpoints.

### Medium-Priority Improvements

1. **Versioning & History**
   - Move beyond a simple Version integer for requirements and test cases—use append-only version/history tables (RequirementVersions, TestCaseVersions) to track all changes, enable redline comparison, rollback, and satisfy audit/compliance needs.
   - Implement API endpoints and UI features for generating redline documents showing the differences between any two versions or releases of requirements and test cases.

2. **DevOps & Cloud Readiness**
   - Add a Dockerfile and sample CI/CD pipeline (GitHub Actions or Azure DevOps).
   - Provide infrastructure-as-code (IaC) templates for cloud resources (SQL, Blob, App Service).
   - Add structured logging (Serilog, Application Insights) and API versioning (Swashbuckle, Microsoft.AspNetCore.Mvc.Versioning).

3. **Multi-Tenancy**
   - Document your multi-tenancy approach (row-level security, separate schema, or DB-per-tenant) or defer until a real need is validated, to avoid unnecessary complexity.

### Quick Wins

- Enable EF Core navigation properties and foreign key constraints.
- Add XML comments and enable Swagger documentation.
- Add .editorconfig and Prettier/ESLint to the frontend for code consistency.

---

## 8. Diagrams

### High-Level Architecture

```mermaid
flowchart LR
    Browser["Web Browser"] <--> Blazor["Blazor Frontend"]
    Blazor <--> API[".NET 8 API"]
    API --> SQL["Azure SQL DB"]
    API --> Blob["Blob Storage"]
```

---

## 9. Summary Table

| Layer       | Technology        | Cloud Service      |
|-------------|-------------------|--------------------|
| Front-End   | Blazor, MudBlazor | Azure Static Web Apps / CDN |
| Back-End    | .NET 8 Web API    | Azure App Service  |
| Database    | Azure SQL         | Azure SQL Database |
| Attachments | Blob Storage      | Azure Blob Storage |
| Auth        | OAuth2/OIDC       | Azure AD           |

---

## 10. References

- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [.NET 8 Documentation](https://docs.microsoft.com/dotnet/)
- [Azure SQL Database](https://azure.microsoft.com/en-us/products/azure-sql-database/)
- [Azure Blob Storage](https://azure.microsoft.com/en-us/products/storage/blobs/)
- [Material UI](https://mui.com/)
