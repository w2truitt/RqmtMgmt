# Requirements & Test Management Tool Solution

## Overview
This solution provides a web-based enterprise tool for managing Customer, Product, and Software Requirements, as well as Test Suites, Test Cases, and Test Runs. It is designed for use by Product Owners, Engineers, and Quality Assurance teams.

---

## Projects in the Solution

### 1. Front-End (React)
- **Location:** `frontend/`
- **Description:**
  - Single Page Application (SPA) built with React.js and Material-UI.
  - Provides user interfaces for requirements management, test management, traceability, and reporting.
  - Communicates with the back-end via RESTful APIs.
  - Uses Redux Toolkit for state management.
- **Key Folders:**
  - `src/components/`: Reusable UI components
  - `src/pages/`: Route-based page components
  - `src/state/`: Redux store and slices
  - `src/services/`: API calls and utilities
  - `src/assets/`: Static files and images
  - `public/`: Static public assets

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
- Modern SPA front-end (React)
- Cloud-hosted RESTful API back-end (.NET 8)
- Azure SQL Database for enterprise-grade data storage
- Azure Blob Storage for attachments
- OAuth 2.0 / OpenID Connect authentication (e.g., Azure AD)

See `architecture.md` for detailed architecture decisions and diagrams.

---

## Getting Started

### Prerequisites
- Node.js (for front-end)
- .NET 8 SDK (for back-end)
- Access to Azure (for cloud deployment)

### Front-End Setup
1. Navigate to `frontend/`
2. Install dependencies: `npm install`
3. Start development server: `npm run dev` (or as specified by chosen tooling)

### Back-End Setup
1. Navigate to `backend/`
2. Restore dependencies: `dotnet restore`
3. Build and run: `dotnet run`

### Environment Configuration
- Use environment variables or `.env` files to configure API URLs, database connections, authentication, and secrets.

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