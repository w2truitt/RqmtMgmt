# Identity Provider Integration with Duende IdentityServer

This document describes the integration of **Duende IdentityServer** as the identity provider for the Requirements Management System, providing OAuth2/OIDC authentication for both the backend API and frontend Blazor WebAssembly application.

## Overview

The identity provider setup includes:
- **Duende IdentityServer** as the OAuth2/OIDC provider
- **ASP.NET Core Identity** for user management
- **SQL Server** database for identity storage
- **JWT Bearer authentication** in the backend API
- **OIDC authentication** in the frontend Blazor app

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │  Identity       │    │   Backend       │
│  (Blazor WASM)  │    │   Server        │    │    API          │
│                 │    │ (IdentityServer)│    │                 │
│  Port: 5001     │    │  Port: 5002     │    │  Port: 5000     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │   Database      │
                    │ (SQL Server)    │
                    │  Port: 1433     │
                    └─────────────────┘
```

## Services

### 1. IdentityServer (Port 5002)
- **Purpose**: OAuth2/OIDC provider for authentication and authorization
- **Technology**: Duende IdentityServer 7.0 with ASP.NET Core Identity
- **Database**: Separate SQL Server database (`RqmtMgmtIdentity`)
- **Features**:
  - User registration and management
  - Role-based access control
  - JWT token issuance
  - OIDC discovery endpoint
  - Development signing credentials

### 2. Backend API (Port 5000)
- **Authentication**: JWT Bearer tokens from IdentityServer
- **Authorization**: Scope-based (`rqmtmgmt.api`) and role-based policies
- **Swagger Integration**: OAuth2 authentication flow for API documentation

### 3. Frontend (Port 5001)
- **Authentication**: OIDC Authorization Code flow with PKCE
- **Token Management**: Automatic token refresh and storage
- **Protected Routes**: All routes require authentication by default

### 4. Nginx Reverse Proxy (Port 8080)
- **Routes**:
  - `/identity/*` → IdentityServer (5002)
  - `/.well-known/*` → IdentityServer (5002) 
  - `/connect/*` → IdentityServer (5002)
  - `/api/*` → Backend API (5000)
  - `/swagger/*` → Backend API (5000)
  - `/*` → Frontend (5001)

## Getting Started

### 1. Start the Services

Use the new Docker Compose configuration with identity provider:

```bash
cd docker-compose
docker compose -f docker-compose.identity.yml up --build
```

### 2. Access the Application

- **Main Application**: http://localhost:8080
- **IdentityServer**: http://localhost:8080/identity
- **API Documentation**: http://localhost:8080/swagger
- **Direct Backend**: http://localhost:5000
- **Direct Frontend**: http://localhost:5001
- **Direct IdentityServer**: http://localhost:5002

### 3. Test Users

The system comes with pre-configured test users:

| Email | Password | Role | Description |
|-------|----------|------|-------------|
| `admin@rqmtmgmt.local` | `Admin123!` | Administrator | Full system access |
| `pm@rqmtmgmt.local` | `PM123!` | ProjectManager | Project management access |
| `dev@rqmtmgmt.local` | `Dev123!` | Developer | Development access |
| `tester@rqmtmgmt.local` | `Test123!` | Tester | Testing access |
| `viewer@rqmtmgmt.local` | `View123!` | Viewer | Read-only access |

## Configuration

### IdentityServer Configuration

The IdentityServer is configured with the following clients:

#### Frontend Client (`rqmtmgmt-frontend`)
- **Grant Type**: Authorization Code with PKCE
- **Scopes**: `openid`, `profile`, `email`, `role`, `rqmtmgmt.api`
- **Redirect URIs**: 
  - `http://localhost:8080/authentication/login-callback`
  - `http://localhost:5001/authentication/login-callback`

#### Backend Client (`rqmtmgmt-backend`)
- **Grant Type**: Client Credentials
- **Scopes**: `rqmtmgmt.api`
- **Use**: Machine-to-machine communication

#### Swagger UI Client (`swagger-ui`)
- **Grant Type**: Authorization Code with PKCE
- **Scopes**: `openid`, `profile`, `rqmtmgmt.api`
- **Use**: API documentation authentication

### Backend API Configuration

Environment variables for the backend:

```yaml
environment:
  - Authentication__Authority=http://identityserver:5002
  - Authentication__Audience=rqmtmgmt-api
  - Authentication__RequireHttpsMetadata=false
```

### Frontend Configuration

The frontend is configured via `wwwroot/appsettings.json`:

```json
{
  "OIDC": {
    "Authority": "http://localhost:8080/identity",
    "ClientId": "rqmtmgmt-frontend",
    "DefaultScopes": [
      "openid", "profile", "email", "role", "rqmtmgmt.api"
    ]
  }
}
```

## Security Features

### 1. Role-Based Authorization

The system supports five roles with hierarchical permissions:

- **Administrator**: Full system access
- **ProjectManager**: Project and team management
- **Developer**: Development and testing features
- **Tester**: Testing and quality assurance
- **Viewer**: Read-only access

### 2. API Security

- All API endpoints require valid JWT tokens
- Scope-based authorization (`rqmtmgmt.api`)
- Role-based policies for sensitive operations
- CORS configuration for frontend access

### 3. Frontend Security

- All routes protected by default
- Automatic token refresh
- Secure token storage
- Logout functionality

## Development Notes

### 1. HTTPS in Production

The current setup uses HTTP for development. For production:

1. Replace `AddDeveloperSigningCredential()` with proper certificates
2. Enable HTTPS metadata validation
3. Update all URLs to use HTTPS
4. Configure proper SSL certificates

### 2. Database Migrations

IdentityServer uses Entity Framework migrations for:
- Configuration database (clients, resources, scopes)
- Operational database (grants, tokens)
- Identity database (users, roles)

### 3. Customization

To customize the identity provider:

1. **UI Customization**: Modify Razor pages in the IdentityServer project
2. **Claims**: Add custom claims in `Config.cs`
3. **Policies**: Update authorization policies in backend `Program.cs`
4. **Scopes**: Modify API scopes and resources as needed

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure all origins are configured in both IdentityServer and backend
2. **Token Validation**: Check that `Authority` and `Audience` match between services
3. **Database Connection**: Verify SQL Server is running and connection strings are correct
4. **Port Conflicts**: Ensure ports 5000, 5001, 5002, and 8080 are available

### Logs

- **IdentityServer**: Configured with Serilog for detailed authentication logs
- **Backend**: ASP.NET Core logging for API requests and authentication
- **Frontend**: Browser developer tools for client-side authentication issues

## Next Steps

1. **Production Deployment**: Configure proper certificates and HTTPS
2. **External Providers**: Add support for Azure AD, Google, etc.
3. **User Management**: Build admin UI for user and role management
4. **Audit Logging**: Enhance audit trails for security compliance
5. **Multi-tenancy**: Extend for multi-tenant scenarios if needed

## References

- [Duende IdentityServer Documentation](https://docs.duendesoftware.com/identityserver/v7)
- [Blazor WebAssembly Authentication](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/)
- [ASP.NET Core JWT Bearer Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)