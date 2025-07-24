# Software Architecture Decisions for Requirements and Test Management Tool

## 1. Overview
This document outlines the software architecture decisions for developing a web-based requirements and test management tool. It covers the front-end and back-end technology choices, database selection, deployment considerations, and provides guidance for developers.

---

## 2. Front-End Architecture

- The front-end shall be a Single Page Application (SPA) accessible via modern web browsers (Chrome, Edge, Firefox, Safari).
- The front-end will be developed using React.js for its robust ecosystem and enterprise support.
- UI state management will use Redux Toolkit for scalability and maintainability.
- UI components will be based on Material-UI (MUI) for consistency and accessibility.
- The front-end will communicate with the back-end via RESTful APIs (JSON over HTTPS).
- Authentication will use OAuth 2.0 / OpenID Connect (OIDC) via a cloud identity provider (e.g., Azure AD, Okta).
- Responsive design will be ensured for desktop and tablet use.

```
+-----------------------+
|  Browser (React SPA)  |
+-----------------------+
            |
      REST APIs (HTTPS)
            |
+-----------------------+
|    Cloud Back-End     |
+-----------------------+
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

```
+----------------------+
| Azure SQL Database   |
+----------------------+
| Tables:              |
| - Requirements       |
| - TestSuites         |
| - TestCases          |
| - TestRuns           |
| - Users/Roles        |
| - AuditLogs          |
+----------------------+
         |
+-----------------------+
| Azure Blob Storage    |
+-----------------------+
| For attachments, logs |
+-----------------------+
```

---

## 5. Key Architectural Considerations

- All data access will be via repository/service patterns in the back-end.
- API versioning will be implemented to support future changes.
- The system will be designed for multi-tenancy and enterprise scalability.
- All communication will be over HTTPS.
- Audit logs will be maintained for compliance and traceability.
- Error handling and logging will use cloud-native monitoring (e.g., Azure Monitor).

---

## 6. Developer Guidance

- Follow SOLID and clean architecture principles in both front-end and back-end code.
- Ensure all business logic is in the back-end API, not in the front-end.
- Use environment variables for all secrets and configuration.
- Write automated unit and integration tests for all layers.
- Use CI/CD pipelines for build, test, and deployment (e.g., GitHub Actions, Azure DevOps).

---

## 7. Diagrams

### High-Level Architecture

```
+-------------------+       +-------------------+       +-------------------+
|   Web Browser     | <---> |   React Frontend  | <---> |   .NET 8 API      |
+-------------------+       +-------------------+       +-------------------+
                                                    |
                                             +-------------------+
                                             |   Azure SQL DB    |
                                             +-------------------+
                                             |   Blob Storage    |
                                             +-------------------+
```

---

## 8. Summary Table

| Layer       | Technology         | Cloud Service      |
|-------------|-------------------|--------------------|
| Front-End   | React, MUI        | Azure Static Web Apps / CDN |
| Back-End    | .NET 8 Web API    | Azure App Service  |
| Database    | Azure SQL         | Azure SQL Database |
| Attachments | Blob Storage      | Azure Blob Storage |
| Auth        | OAuth2/OIDC       | Azure AD           |

---

## 9. References
- [React Documentation](https://reactjs.org/)
- [.NET 8 Documentation](https://docs.microsoft.com/dotnet/)
- [Azure SQL Database](https://azure.microsoft.com/en-us/products/azure-sql-database/)
- [Azure Blob Storage](https://azure.microsoft.com/en-us/products/storage/blobs/)
- [Material UI](https://mui.com/)
