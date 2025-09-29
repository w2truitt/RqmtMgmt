# JWT Issuer Validation Fix

## Problem Summary

The backend was experiencing a `SecurityTokenInvalidIssuerException` with the error:
```
IDX10205: Issuer validation failed. Issuer: 'https://rqmtmgmt.local'. 
Did not match: validationParameters.ValidIssuer: 'http://identityserver-service:80'
```

This occurred because:
1. **IdentityServer** was issuing tokens with `https://rqmtmgmt.local` as the issuer (configured via `IdentityServer__IssuerUri`)
2. **Backend API** was only accepting `http://identityserver-service:80` as a valid issuer (from ConfigMap)
3. **Token validation failed** because the issuer in the JWT token didn't match the expected issuer

## Root Cause

In a containerized environment with Traefik as a reverse proxy:
- External clients access the system via `https://rqmtmgmt.local`
- Internal Kubernetes services communicate via `http://identityserver-service:80`
- IdentityServer issues tokens with the external issuer URI for consistency
- The backend needs to accept both external and internal issuer URIs

## Solution Implemented

### 1. Updated Backend JWT Configuration

Modified `/backend/Configuration/ServiceCollectionExtensions.cs` to support multiple valid issuers:

```csharp
// Configure multiple valid issuers to handle different environments
var validIssuers = new List<string>
{
    "https://rqmtmgmt.local",              // External URL (Traefik proxy)
    "http://identityserver-service:80",    // Internal Kubernetes service
    "http://identityserver-service",       // Internal Kubernetes service (no port)
    identityServerUrl                      // Configured authority URL
};

// Add any additional valid issuer from configuration
var configuredValidIssuer = configuration["Authentication:ValidIssuer"];
if (!string.IsNullOrEmpty(configuredValidIssuer) && !validIssuers.Contains(configuredValidIssuer))
{
    validIssuers.Add(configuredValidIssuer);
}

options.TokenValidationParameters = new TokenValidationParameters
{
    // ... other settings ...
    
    // Configure multiple valid issuers to handle proxy and internal communication
    ValidIssuers = validIssuers,  // Changed from ValidIssuer to ValidIssuers
    
    // ... rest of configuration ...
};
```

### 2. Key Changes Made

- **Replaced `ValidIssuer`** with `ValidIssuers` to accept multiple issuer URIs
- **Added comprehensive issuer list** covering all possible scenarios:
  - `https://rqmtmgmt.local` - External access via Traefik
  - `http://identityserver-service:80` - Internal K8s service with port
  - `http://identityserver-service` - Internal K8s service without port
  - Any configured authority URL from settings
- **Maintained backward compatibility** by including configured ValidIssuer if present
- **Added documentation** in ConfigMap explaining the multi-issuer setup

### 3. Current Configuration Status

**IdentityServer Deployment** (already correct):
```yaml
env:
- name: IdentityServer__IssuerUri
  value: "https://rqmtmgmt.local"
```

**Backend Configuration** (now updated):
- Accepts multiple issuers as listed above
- Maintains existing authority configuration
- Preserves all other JWT validation settings

## Verification Steps

After deploying these changes:

1. **Check IdentityServer Discovery Document**:
   ```bash
   curl https://rqmtmgmt.local/.well-known/openid-configuration
   ```
   Should show `"issuer": "https://rqmtmgmt.local"`

2. **Test Token Validation**:
   - Tokens issued by IdentityServer should now be accepted by the backend
   - Both external (via Traefik) and internal (K8s service) communication should work

3. **Monitor Logs**:
   - Backend should no longer show issuer validation errors
   - JWT authentication events should show successful validation

## Benefits of This Approach

1. **Environment Flexibility**: Works in both development and production environments
2. **Proxy Compatibility**: Handles reverse proxy scenarios correctly
3. **Internal Communication**: Supports direct service-to-service communication
4. **Security Maintained**: Still validates issuers, just accepts multiple valid ones
5. **Backward Compatible**: Existing configurations continue to work

## Alternative Approaches Considered

1. **Change IdentityServer issuer to internal URL**: Would break external client access
2. **Disable issuer validation**: Would compromise security
3. **Use different tokens for internal/external**: Would complicate token management

The implemented solution provides the best balance of security, flexibility, and maintainability.

## Deployment Notes

- **No database changes required**
- **No IdentityServer configuration changes needed**
- **Only backend code changes required**
- **ConfigMap updated with documentation only**
- **Backward compatible with existing tokens**

This fix resolves the JWT issuer validation error while maintaining security and supporting both internal Kubernetes communication and external access via the Traefik proxy.