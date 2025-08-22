# Docker Compose Setup for Local Development

## Overview
This setup runs the backend API and a SQL Server database in containers for local development and testing.

## Identity Provider Integration

For a complete setup with authentication and user management, use the identity provider configuration:

```sh
./start-with-identity.sh
# OR
docker compose -f docker-compose.identity.yml up --build
```

This includes:
- **Duende IdentityServer** for OAuth2/OIDC authentication
- **Role-based access control** with test users
- **JWT token authentication** for the API
- **OIDC authentication** for the frontend

See [IDENTITY-PROVIDER-README.md](./IDENTITY-PROVIDER-README.md) for detailed documentation.

## Basic Setup (No Authentication)

## Usage
1. Make sure Docker Desktop is running.
2. From the `docker-compose` folder, run:
   
   ```sh
   docker compose up --build
   ```
3. The backend API will be available at http://localhost:5000
4. SQL Server will be available at localhost:1433 (user: sa, password: Your_password123)

## Notes
- The backend is configured to use the local SQL container. Connection string is set via environment variable.
- If you change the database schema, run migrations as needed.
- Default credentials are for local testing only. Change the SA password for production use.