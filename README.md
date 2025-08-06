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

---

## Architecture

- Modern SPA front-end (C# Blazor WebAssembly)
- Cloud-hosted RESTful API back-end (.NET 8)
- Azure SQL Database for enterprise-grade data storage
- Azure Blob Storage for attachments
- OAuth 2.0 / OpenID Connect authentication (e.g., Azure AD, Okta, Google, IdentityServer)

See `architecture.md` for detailed architecture decisions and diagrams.

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
   - Frontend: http://localhost:8080 (via nginx)
   - Backend API: http://localhost:8080/api (via nginx)
   - Swagger: http://localhost:8080/swagger (via nginx)
   - Database: localhost:1433 (direct connection)

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
- See `.structure.md` in each project for organizational guidance.

---

## References

- [requirements.md](requirements.md): Product requirements
- [architecture.md](architecture.md): Architecture decisions and diagrams

---

For questions or suggestions, contact the project maintainers.