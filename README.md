# Requirements & Test Management Tool Solution

## Overview

This solution provides a web-based enterprise tool for managing Customer, Product, and Software Requirements, as well as Test Suites, Test Cases, and Test Runs. It is designed for use by Product Owners, Engineers, and Quality Assurance teams.

---

## Projects in the Solution

### 1. Front-End (Blazor WebAssembly)

- **Location:** `frontend/`
- **Description:**
  - Single Page Application (SPA) built with C# and Blazor WebAssembly.
  - Provides user interfaces for requirements management, test management, traceability, and reporting.
  - Communicates with the back-end via RESTful APIs.
- **Key Folders:**
  - `Pages/`: Blazor route-based pages
  - `wwwroot/`: Static assets (CSS, JS, images)
  - `Components/`: Reusable Blazor components
  - `Layout/`: Shared layouts and nav menus
  - `Properties/`: Project properties
  - `public/`: (Legacy, safe to delete)

### 2. Back-End (.NET 8 Web API)

- **Location:** `backend/`
- **Description:**
  - RESTful API built with .NET 8 Web API.
  - Handles business logic, data persistence, authentication, and authorization.
  - Containerized for cloud deployment (e.g., Azure App Service).
  - Connects to Azure SQL Database and Blob Storage for data and attachments.
- **Key Folders:**
  - `Controllers/`: API endpoints
  - `Models/`: Domain entities
  - `Data/`: Database context and repositories
  - `Services/`: Business logic and helpers
  - `DTOs/`: Data transfer objects
  - `Configurations/`: Settings and DI

### 3. Frontend Component Tests (bUnit + xUnit)

- **Location:** `frontend.ComponentTests/`
- **Description:**
  - Component-level testing for Blazor components using bUnit framework.
  - Isolated testing with mocked services and dependencies.
  - Fast-running unit tests for component logic and rendering.
- **Key Folders:**
  - `Components/`: Component test suites organized by feature
  - `TestHelpers/`: Test utilities and helper classes
  - `ComponentTestBase.cs`: Base class with common test setup
- **Test Results:** ✅ **73 tests passing** - Complete component coverage

### 4. Frontend E2E Tests (Playwright + xUnit)

- **Location:** `frontend.E2ETests/`
- **Description:**
  - End-to-end browser automation testing using Playwright.
  - Full application testing with real backend integration.
  - Cross-browser testing capabilities for comprehensive coverage.
- **Key Folders:**
  - `PageObjects/`: Page object model classes for maintainable tests
  - `Workflows/`: E2E test suites organized by user workflows
  - `TestData/`: Test data factories and seeding utilities
  - `E2ETestBase.cs`: Base class with Playwright setup and utilities
- **Test Results:** ✅ **53 tests passing** - Complete page coverage

### 5. Backend Unit Tests (xUnit)

- **Location:** `backend.Tests/`
- **Description:**
  - Unit tests for backend services, controllers, and business logic.
  - Isolated testing with mocked dependencies.
  - Fast-running tests for individual component validation.
- **Test Results:** ✅ **499 tests passing** - Comprehensive backend coverage

### 6. Backend API Tests (xUnit)

- **Location:** `backend.ApiTests/`
- **Description:**
  - Integration tests for API endpoints and workflows.
  - End-to-end backend testing with real database integration.
  - Performance and error handling validation.
- **Test Results:** ✅ **160 tests passing** - Complete API coverage

### 7. Shared Library (RqmtMgmtShared)

- **Location:** `RqmtMgmtShared/`
- **Description:**
  - Common DTOs, interfaces, enums, and models shared between projects.
  - Packaged as a local NuGet package for consistent versioning.
  - Service interfaces for dependency injection and testing.

---

## Architecture

- Modern SPA front-end (C# Blazor WebAssembly)
- Cloud-hosted RESTful API back-end (.NET 8)
- Azure SQL Database for enterprise-grade data storage
- Azure Blob Storage for attachments
- OAuth 2.0 / OpenID Connect authentication (e.g., Azure AD, Okta, Google, IdentityServer)
- **Comprehensive Test Coverage:** Component, E2E, Unit, and Integration tests

See `architecture.md` for detailed architecture decisions and diagrams.

---

## Test Infrastructure

The solution includes a comprehensive testing framework with **732 total tests** across all layers:

### **Frontend Testing (126 tests)**
- **Component Tests (73):** bUnit-based isolated component testing
- **E2E Tests (53):** Playwright-based browser automation testing

### **Backend Testing (659 tests)**  
- **Unit Tests (499):** Service and business logic testing
- **API Tests (160):** Integration and endpoint testing (using HTTPS for integration tests)

### **Test Commands**
```bash
# Run all tests
dotnet test RqmtMgmt.sln

# Run specific test suites
dotnet test frontend.ComponentTests/
dotnet test frontend.E2ETests/
dotnet test backend.Tests/
dotnet test backend.ApiTests/

# Run E2E tests with resource management (recommended)
./scripts/run-e2e-tests.sh

# Alternative: Run E2E tests from docker-compose directory
cd docker-compose/
./run-e2e-tests-local.sh
```

**E2E Testing Resource Management:**
E2E tests require careful resource management to prevent out-of-memory issues. See [E2E_RESOURCE_MANAGEMENT.md](E2E_RESOURCE_MANAGEMENT.md) for detailed configuration and troubleshooting.

---

## Getting Started

### Prerequisites
- .NET 8 SDK (for both front-end and back-end)
- Docker and Docker Compose (for containerized development)
- Access to Azure (for cloud deployment)

### Docker-Based Development (Recommended)

The solution is designed to run in Docker containers with nginx as a reverse proxy:

1. **Navigate to the docker-compose directory:**
   ```bash
   cd docker-compose/
   ```

2. **Start all services:**
   ```bash
   docker-compose up -d
   ```

3. **Access the application:**
   - Frontend: http://localhost:8080 or https://localhost (via nginx)
   - Backend API: http://localhost:8080/api or https://localhost/api (via nginx)
   - Swagger: http://localhost:8080/swagger or https://localhost/swagger (via nginx)
   - Database: localhost:1433 (direct connection)

**Note:** HTTPS access requires adding `rqmtmgmt.local` to your `/etc/hosts` file:
```
127.0.0.1   rqmtmgmt.local
```

For production and integration testing, HTTPS is recommended.

The nginx configuration forwards:
- `/api/*` requests to the backend container (port 80)
- `/swagger/*` requests to the backend container (port 80)  
- All other requests to the frontend container (port 80)

### RqmtMgmtShared Package Management

The solution uses a shared library (`RqmtMgmtShared`) that contains common DTOs, interfaces, and models used by both frontend and backend projects. This is packaged as a local NuGet package.

**When to Update the Shared Package:**
- Adding new DTOs, interfaces, or shared models
- Modifying existing shared contracts
- Adding new enums or constants used across projects

**Process for Updating RqmtMgmtShared:**

1. **Make changes to the RqmtMgmtShared project**

2. **Increment the version number in `RqmtMgmtShared/RqmtMgmtShared.csproj`:**
   ```xml
   <Version>1.0.9</Version>
   <PackageVersion>1.0.9</PackageVersion>
   ```

3. **Build and pack the new version:**
   ```bash
   cd RqmtMgmtShared/
   dotnet pack -o ../local_nuget/
   ```

4. **Update all consuming projects to use the new version:**
   - `backend/backend.csproj`
   - `backend.Tests/backend.Tests.csproj`  
   - `backend.ApiTests/backend.ApiTests.csproj`
   - `frontend/frontend.csproj`
   - `frontend.ComponentTests/frontend.ComponentTests.csproj`
   - `frontend.E2ETests/frontend.E2ETests.csproj`
   
   Update the PackageReference:
   ```xml
   <PackageReference Include="RqmtMgmtShared" Version="1.0.9" />
   ```

5. **Clear NuGet caches and restore:**
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

6. **Rebuild Docker containers to pick up the new package:**
   ```bash
   cd docker-compose/
   docker-compose down
   docker-compose up --build -d
   ```

**Important Notes:**
- The `local_nuget/` folder is mounted into both frontend and backend containers
- The `NuGet.Config` files in each project point to the local package store
- Always increment the version number when making changes to avoid caching issues
- The Docker containers will rebuild and pick up the new package version automatically

### Alternative: Local Development Setup

#### Front-End Setup (Blazor)

1. Navigate to `frontend/`
2. Restore dependencies: `dotnet restore`
3. Start development server: `dotnet run` (or use Visual Studio/VS Code launch)

#### Back-End Setup

1. Navigate to `backend/`
2. Restore dependencies: `dotnet restore`
3. Build and run: `dotnet run`

### Database Schema Setup (Entity Framework Migrations)

The recommended way to create and update your database schema is with Entity Framework Core migrations.

**To initialize your database:**

1. Make sure your DbContext and model classes are up to date.
2. In the `backend/` directory, add a migration:
   ```sh
   dotnet ef migrations add InitialCreate
   ```
3. Apply the migration to your database:
   ```sh
   dotnet ef database update
   ```

**To apply schema changes in the future:**
1. Update your model/entity classes.
2. Add a new migration:
   ```sh
   dotnet ef migrations add <MigrationName>
   dotnet ef database update
   ```

> **Note:** For Docker Compose SQL Server, ensure your connection string in `appsettings.Development.json` and compose file matches your container settings. Run migrations from your host or as an entrypoint/startup command in the backend container.

**Legacy:** If you need to use raw SQL scripts (e.g., for legacy or bulk setup), see `backend/Data/tables.sql`.

### Environment Configuration

- Use environment variables or `.env` files to configure API URLs, database connections, authentication, and secrets.

#### Example `backend/appsettings.json` for Authentication
```json
{
  "Authentication": {
    "Authority": "https://YOUR_OIDC_AUTHORITY", // e.g. Azure AD, Okta, Google, or IdentityServer
    "Audience": "YOUR_API_CLIENT_ID"
  }
}
```

#### Configuring OIDC Providers

- **Azure AD**:
  - Set `Authority` to `https://login.microsoftonline.com/{TENANT_ID}/v2.0`
  - Set `Audience` to your Azure AD App Registration's Application (client) ID
  - See [Microsoft Docs](https://docs.microsoft.com/azure/active-directory/develop/v2-protocols-oidc)

- **Okta**:
  - Set `Authority` to `https://{yourOktaDomain}/oauth2/default`
  - Set `Audience` to your Okta API audience/client ID
  - See [Okta OIDC Docs](https://developer.okta.com/docs/guides/implement-auth-code/overview/)

- **Google Identity Platform**:
  - Set `Authority` to `https://accounts.google.com`
  - Set `Audience` to your Google OAuth 2.0 Client ID
  - See [Google OIDC Docs](https://developers.google.com/identity/protocols/oauth2/openid-connect)

- **IdentityServer (Self-hosted)**:
  - Set `Authority` to your IdentityServer base URL (e.g., `https://identity.example.com`)
  - Set `Audience` to the API resource name or client ID
  - See [IdentityServer Docs](https://docs.duendesoftware.com/identityserver/v6/)

> For all providers, you must register your API as an application/client and obtain the correct values for Authority and Audience.

---

## Contribution Guidelines

- Follow the project structure and naming conventions.
- Write clear commit messages and document code.
- Add or update tests for new features and bug fixes.
- Ensure all tests pass before submitting changes: `dotnet test RqmtMgmt.sln`
- See `.structure.md` in each project for organizational guidance.

---

## References

- [requirements.md](requirements.md): Product requirements
- [architecture.md](architecture.md): Architecture decisions and diagrams
- [FRONTEND_TESTING_STATUS.md](FRONTEND_TESTING_STATUS.md): Frontend testing infrastructure status

---

For questions or suggestions, contact the project maintainers.