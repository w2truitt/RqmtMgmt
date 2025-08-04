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
- OAuth 2.0 / OpenID Connect authentication (e.g., Azure AD)

See `architecture.md` for detailed architecture decisions and diagrams.

---

## Getting Started


### Prerequisites

- .NET 8 SDK (for both front-end and back-end)
- Access to Azure (for cloud deployment)



### Front-End Setup (Blazor)

1. Navigate to `frontend/`
2. Restore dependencies: `dotnet restore`
3. Start development server: `dotnet run` (or use Visual Studio/VS Code launch)


### Back-End Setup

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

### Prerequisites

- Node.js (for front-end)
- .NET 8 SDK (for back-end)
- Access to Azure (for cloud deployment)

### Environment Configuration

- Use environment variables or `.env` files to configure API URLs, database connections, authentication, and secrets.


#### Example `wwwroot/appsettings.json` for Front-End
```json
{
  "ApiUrl": "http://localhost:5000"
}
```

#### Example `appsettings.Development.json` for Back-End
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RqmtMgmtDb;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Note:** Update connection strings and API URLs as needed for your dev, test, or production environments.

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
