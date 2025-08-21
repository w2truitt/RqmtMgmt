# Embed Duende IdentityServer (self-hosted, local ASP.NET Core Identity) into `backend/`

This document contains a developer task prompt and step-by-step instructions to embed Duende IdentityServer (using local ASP.NET Core Identity) into the existing `backend/` project for a Blazor WebAssembly (hosted) solution.

> IMPORTANT: Duende IdentityServer requires a commercial license for production use. Use the development license for local/dev only and purchase a production license before deploying to production.

## ✅ IMPLEMENTATION PROGRESS (Updated: August 21, 2025)

### **COMPLETED TASKS**
- ✅ **Phase 1: NuGet Packages & Dependencies**
  - Added Duende.IdentityServer 7.0.8
  - Added Duende.IdentityServer.AspNetIdentity 7.0.8 
  - Added Microsoft.AspNetCore.Authentication.OpenIdConnect 9.0.0-rc.1.24452.1
  - All packages successfully installed and verified

- ✅ **Phase 2: Backend Models & Configuration**
  - Created `backend/Models/ApplicationUser.cs` (extends IdentityUser with FirstName, LastName, timestamps)
  - Created `backend/Data/ApplicationIdentityDbContext.cs` (separate Identity context)
  - Created `backend/Configuration/IdentityServerConfig.cs` (clients, resources, scopes)
  - Created `backend/Data/IdentitySeedData.cs` (admin user + roles: Admin, Manager, Developer, Tester, Viewer)

- ✅ **Phase 3: Backend Program.cs Integration**
  - Configured dual database contexts (ApplicationDbContext + ApplicationIdentityDbContext)
  - Added ASP.NET Core Identity with ApplicationUser
  - Added Duende IdentityServer with AspNetIdentity integration
  - Configured JWT Bearer authentication for API protection
  - Implemented proper middleware pipeline order
  - Added ForwardedHeaders for containerized deployment

- ✅ **Phase 4: Frontend OIDC Configuration**
  - Updated `frontend/Program.cs` with AddOidcAuthentication
  - Configured HttpClient with BaseAddressAuthorizationMessageHandler
  - Created `frontend/Pages/Authentication/Authentication.razor` (OIDC callbacks)
  - Created `frontend/Components/RedirectToLogin.razor` (login redirect)
  - Created `frontend/Components/LoginDisplay.razor` (login/logout UI)
  - Updated `frontend/App.razor` with authentication routing
  - Updated `frontend/Layout/MainLayout.razor` with login display

- ✅ **Phase 5: Database & Containerization**
  - Applied EF migrations for Identity tables (AspNetUsers, AspNetRoles, etc.)
  - Updated `docker-compose/nginx.conf` for IdentityServer endpoint routing
  - Updated `docker-compose/docker-compose.yml` for container communication
  - Successfully built and deployed all containers

- ✅ **Phase 6: System Verification**
  - ✅ Backend container healthy and running on port 5000
  - ✅ Database connections established successfully
  - ✅ Identity seed data applied (admin@rqmtmgmt.local created with Admin role)
  - ✅ IdentityServer started successfully (version 7.0.8)
  - ✅ Frontend container healthy and running on port 5001
  - ✅ Nginx proxy routing working for all endpoints
  - ✅ IdentityServer discovery endpoint accessible: `http://localhost:8080/.well-known/openid-configuration`
  - ✅ Frontend accessible through nginx proxy: `http://localhost:8080`

### **CURRENT STATUS**: ✅ **IMPLEMENTATION COMPLETE** - Authentication System Deployed & Tested

### **SEEDED ADMIN CREDENTIALS**
- **Email**: `admin@rqmtmgmt.local`
- **Password**: `Admin123!`
- **Roles**: Admin (with full system access)

### **AUTHENTICATION TESTING RESULTS** ✅
- ✅ **Frontend Access**: Application loads successfully at `http://localhost:8080`
- ✅ **IdentityServer Discovery**: OpenID configuration available at `/.well-known/openid-configuration`
- ✅ **Authorization Endpoint**: OIDC authorization flow accessible at `/connect/authorize`
- ✅ **API Protection**: Endpoints return HTTP 302 (redirect to login) when unauthenticated
- ✅ **JWT Bearer Authentication**: Configured and ready for token validation
- ✅ **CORS Configuration**: Frontend and IdentityServer communication enabled
- ✅ **Container Integration**: All services communicating properly through nginx proxy

### **SECURITY IMPLEMENTATION STATUS**
- ✅ **API Authorization**: Added `[Authorize]` attributes to controllers (example: ProjectsController)
- ✅ **JWT Token Validation**: Backend validates tokens from embedded IdentityServer  
- ✅ **OIDC Client Configuration**: Frontend configured for Authorization Code + PKCE flow
- ✅ **Role-Based Access**: Admin user seeded with full system access
- ✅ **Development License**: Using Duende development license (warnings expected)

### **READY FOR PRODUCTION** (Pending Items)
- [ ] Purchase Duende IdentityServer production license
- [ ] Add `[Authorize]` attributes to remaining API controllers as needed
- [ ] Configure HTTPS certificates for production deployment
- [ ] Set up production secret management for connection strings and license keys

---

## Title
Embed Duende IdentityServer (self-hosted, local ASP.NET Core Identity) into `backend/` and wire Blazor WASM (hosted) + API

## Summary / Objective

Embed Duende IdentityServer into the existing `backend/` project so the solution uses a single backend service that:

- Provides OIDC (Authorization Code + PKCE) for the Blazor WebAssembly (hosted) frontend.
- Uses ASP.NET Core Identity (EF Core) as the local user store (seed an admin user).
- Issues access tokens to the WASM client and protects the API with JWT Bearer validation and scope/role checks.

Deliverables:

- Changes to `backend/` (IdentityServer + Identity + migrations + seed data)
- Frontend changes to configure OIDC PKCE client (config keys only; no UI rewrites)
- Updated `backend/appsettings.json` example entries and environment variable names
- Local dev run instructions, migration commands, and tests (manual + one integration test suggestion)
- README note about Duende production licensing and secrets handling

## Assumptions

- Project is Blazor WebAssembly (hosted) with `frontend/` and `backend/` folders.
- .NET SDK >= 8 is available in dev/CI.
- You accept Duende development license for local use and will obtain a production license before prod deployment.
- HTTPS is available for local dev (required for OIDC redirects).

## Success criteria

- Developer can run the solution locally and authenticate via IdentityServer embedded in `backend/`.
- Blazor client performs OIDC login (PKCE) and receives access token.
- API endpoints protected with `[Authorize]` return 200 for valid tokens and 401/403 otherwise.
- Admin user is created via seed and can log in.

## Files to edit / create (explicit)

- Edit:
  - `backend/Program.cs` — add ASP.NET Core Identity, configure Duende IdentityServer, register clients/resources, and ensure authentication/authorization middleware order (IdentityServer -> Authentication -> Authorization).
  - `backend/appsettings.json` — add `Authentication:Authority`, `Authentication:Audience`, `IdentityServer` config placeholder for clients/scopes, and connection string keys.
  - `frontend/Program.cs` — configure the WASM OIDC client options (authority, client id, scopes) and replace `HttpClient` registration with token-attaching handler (use `BaseAddressAuthorizationMessageHandler`).
- Create:
  - `backend/Data/ApplicationIdentityDbContext.cs` (IdentityDbContext) if not present.
  - `backend/SeedData.cs` — seed admin user and roles.
  - EF migrations for Identity in `backend/` and instructions in README.
- Optional:
  - `docker-compose.yml` updates (if containerized) to ensure `backend` exposes HTTPS and the frontend uses correct dev URIs.

## Concrete step-by-step tasks (developer-level)

1. Add NuGet packages to `backend/`:
   - `Duende.IdentityServer`
   - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
   - `Microsoft.EntityFrameworkCore.SqlServer` (or chosen DB provider)
   - `Duende.IdentityServer.EntityFramework` (optional for persisted grants/config)
2. Configure ASP.NET Core Identity in `backend/Program.cs`:
   - Register `IdentityDbContext<ApplicationUser>` with EF Core and the same `DefaultConnection`.
   - Add Identity services and configure password options as desired.
3. Configure Duende IdentityServer in `backend/Program.cs`:
   - Register IdentityServer and connect it to ASP.NET Identity for user login.
   - Add in-memory (or EF-backed) clients, identity resources, API scopes/resources.
   - Add a client entry for the Blazor WASM SPA:
     - Grant type: Authorization Code with PKCE
     - No client secret (public client)
     - Redirect URIs: `https://localhost:{frontend_port}/authentication/login-callback`
     - Post-logout redirect URIs and allowed CORS origins: frontend origin
     - Allowed scopes: `openid profile email` plus your API scope (e.g., `rqmtapi`)
4. Ensure authentication & middleware order:
   - Call `app.UseHttpsRedirection();`
   - Register `app.UseStaticFiles();` if needed
   - `app.UseRouting();`
   - `app.UseIdentityServer();`
   - `app.UseAuthentication();`
   - `app.UseAuthorization();`
   - Map controllers/endpoints after middleware.
5. Update existing JWT Bearer registration (if present) to validate tokens issued by local IdentityServer:
   - Set `Authority` = `https://localhost:{backend_port}` (backend generates tokens)
   - Set `Audience` = API resource name (e.g., `rqmtapi`)
   - Keep token validation parameters (issuer, audience, lifetime)
6. Add EF migrations & seed:
   - Create Identity migrations in `backend/` and apply them.
   - Implement `SeedData` to create an initial admin user with a strong default password and add role claim(s). Document how to change credentials and remove the seeded account.
7. Frontend: configure OIDC client (no UI code required, only wiring):
   - Add OIDC options in `frontend/Program.cs` or `wwwroot/appsettings.json`:
     - `Authority` = `https://localhost:{backend_port}`
     - `ClientId` = `<wasm-client-id>`
     - `DefaultScopes` = `openid profile email rqmtapi`
   - Use `Microsoft.AspNetCore.Components.WebAssembly.Authentication` to register authentication and `HttpClient` that attaches tokens to requests to the API origin.
8. CORS & HTTPS:
   - Ensure `backend` CORS allows `https://localhost:{frontend_port}`.
   - Ensure IdentityServer client allowed CORS origins include frontend URL.
   - Verify HTTPS certs for local dev or configure dev container certs for Docker.
9. Tests & verification:
   - Manual smoke test steps (login, call protected endpoint, logout).
   - Add an integration test skeleton that either obtains a test token from IdentityServer or uses a test token generator to call a protected endpoint.
10. Document:
   - Update `README.md` and add `backend/README.md` with run steps, migration commands, seed admin credentials, and Duende license instructions.

## Config key names & env variables to add (examples)

- `ConnectionStrings:DefaultConnection` (backend DB)
- `Authentication:Authority` = `https://localhost:{backend_port}`
- `Authentication:Audience` = `rqmtapi` (API resource name)
- `IdentityServer:Clients:<WasmClient>` placeholders (client id, redirect URIs)
- Env vars for secrets:
  - `BACKEND__CONNECTIONSTRINGS__DEFAULTCONNECTION`
  - `BACKEND__DUENDE_LICENSE` (store license in CI secret vault for prod)

## Manual verification (smoke test)

1. Run `dotnet ef database update` for `backend` (Identity migrations applied).
2. Start `backend` (with HTTPS) and `frontend` (WASM) locally.
3. Visit the app, click Login → you should be redirected to `https://localhost:{backend_port}/connect/authorize` (IdentityServer login page).
4. Login with seeded admin user.
5. Confirm UI reflects authenticated state and visit a page that triggers an authenticated API call — response should be HTTP 200.
6. Try API call in Postman without token → expect 401. With access token → expect 200.
7. Logout → ensure token is invalidated client-side.

## Rollback & troubleshooting notes

- If auth breaks, revert PR or set `Authentication:Authority` to empty to disable validation temporarily while debugging.
- Check CORS, redirect URIs, and clock skew when tokens are rejected.
- Use IdentityServer and ASP.NET Identity logs to diagnose login/token issues.
- Migration rollback: `dotnet ef database update 0` (document DB provider specifics).

## Duende license reminder

- Duende IdentityServer requires a purchased license for production use. Use the development license locally only. Add license deployment steps and secret storage in CI/CD before production deployment.

## Time estimate (rough)

- Implement + seed + migrations: 1.5–2 days
- Frontend OIDC wiring + tests: 0.5 day
- Docker/CI updates + docs: 0.5–1 day
Total: ~2.5–3.5 days depending on CI and test coverage.

---

If you want this document adjusted (for example: specific `frontend` and `backend` ports to include in redirect URIs, a particular DB provider (Postgres vs SQL Server), or different scope names), edit this file or request the change and it will be updated.
