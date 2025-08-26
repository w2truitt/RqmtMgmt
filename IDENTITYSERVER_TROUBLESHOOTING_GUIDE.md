# IdentityServer OAuth2 Flow Troubleshooting Guide

## Problem Summary
The backend API endpoint `/api/User/me` is not finding the identity of the signed-in user from the OAuth2 identity utilized by both the frontend and backend applications.

## Root Causes Identified

### 1. Authority/Issuer Mismatch
- **Problem**: Backend API was configured to trust `http://localhost:80` but IdentityServer issues tokens with `https://rqmtmgmt.local`
- **Solution**: Updated backend to use `https://rqmtmgmt.local` as authority

### 2. Audience Configuration Mismatch
- **Problem**: API resource name vs expected audience inconsistency
- **Solution**: Configured multiple valid audiences in backend: `["rqmtapi", "rqmtmgmt-api", "rqmtmgmt.api"]`

### 3. JWT Claim Mapping Issues
- **Problem**: Default .NET JWT claim mapping was interfering with claim extraction
- **Solution**: Disabled default claim mapping and configured proper claim types

### 4. Mixed HTTP/HTTPS Configuration
- **Problem**: Inconsistent protocol usage across components
- **Solution**: Standardized on HTTPS for IdentityServer issuer while allowing HTTP for development

## Changes Made

### Backend API (`backend/Program.cs`)
```csharp
// Disabled default JWT claim mapping
Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// Updated authority to HTTPS
var identityServerUrl = "https://rqmtmgmt.local";

// Added multiple valid audiences
ValidAudiences = new[] { "rqmtapi", "rqmtmgmt-api", "rqmtmgmt.api" }

// Added proper claim mapping
NameClaimType = "name",
RoleClaimType = "role"

// Added debugging event handlers
```

### Backend Configuration (`backend/appsettings.json`)
```json
{
  "Authentication": {
    "Authority": "https://rqmtmgmt.local",
    "Audience": "rqmtmgmt-api"
  }
}
```

### IdentityServer Configuration (`identityserver/Config.cs`)
```csharp
// Enhanced API resource with proper claims
new ApiResource("rqmtmgmt-api", "Requirements Management API")
{
    Scopes = { "rqmtmgmt.api" },
    UserClaims = new List<string> { "role", "email", "name", "sub" },
    ShowInDiscoveryDocument = true
}
```

### Enhanced UserController (`backend/Controllers/UserController.cs`)
- Added comprehensive claim debugging
- Multiple approaches to extract email claim
- Better error reporting with available claims
- Console logging for troubleshooting

## Testing and Verification Steps

### 1. Verify Token Flow
```bash
# Check if tokens are being issued correctly
curl -X GET "https://rqmtmgmt.local/.well-known/openid-configuration"
```

### 2. Inspect JWT Token
- Use a JWT decoder (jwt.io) to examine token contents
- Verify `iss` (issuer) matches `https://rqmtmgmt.local`
- Verify `aud` (audience) contains expected values
- Check for `email`, `name`, `role`, and `sub` claims

### 3. Test API Endpoint
```bash
# Test the /api/User/me endpoint with a valid token
curl -X GET "https://rqmtmgmt.local/api/User/me" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Check Logs
- Monitor backend logs for authentication events
- Look for claim validation messages
- Check for token validation failures

## Common Issues and Solutions

### Issue: "Email claim not found in token"
**Causes:**
- Claims not being included in token
- Claim mapping issues
- Wrong claim type names

**Solutions:**
1. Verify IdentityServer includes email in UserClaims
2. Check if claim mapping is interfering
3. Use multiple claim type variations in extraction

### Issue: "Token signature validation failed"
**Causes:**
- Authority mismatch between issuer and validator
- Signing key not accessible
- Clock skew issues

**Solutions:**
1. Ensure Authority URL matches exactly
2. Verify discovery endpoint is accessible
3. Configure appropriate ClockSkew

### Issue: "Audience validation failed"
**Causes:**
- Token audience doesn't match expected audience
- API resource configuration issues

**Solutions:**
1. Configure multiple ValidAudiences
2. Verify API resource name matches token audience
3. Check client scope configuration

## Debugging Commands

### 1. Check IdentityServer Discovery
```bash
curl -s https://rqmtmgmt.local/.well-known/openid-configuration | jq
```

### 2. Decode JWT Token (using jq)
```bash
# Extract payload from JWT token
echo "YOUR_JWT_TOKEN" | cut -d. -f2 | base64 -d | jq
```

### 3. Test Token Introspection
```bash
curl -X POST "https://rqmtmgmt.local/connect/introspect" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "token=YOUR_ACCESS_TOKEN&client_id=YOUR_CLIENT_ID&client_secret=YOUR_CLIENT_SECRET"
```

## Additional Recommendations

### 1. Enable Detailed Logging (Development Only)
```csharp
// In Program.cs - REMOVE IN PRODUCTION
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
```

### 2. Add Health Check Endpoint
```csharp
app.MapGet("/auth-debug", [Authorize] (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        IsAuthenticated = user.Identity?.IsAuthenticated,
        Name = user.Identity?.Name,
        Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
    });
});
```

### 3. Implement Claims Transformation (if needed)
```csharp
public class CustomClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Add custom claim transformation logic here
        return Task.FromResult(principal);
    }
}
```

## Production Considerations

1. **Remove Debug Logging**: Disable PII logging and detailed authentication logs
2. **Use HTTPS**: Ensure all communications use HTTPS in production
3. **Proper Certificates**: Replace developer signing credentials with production certificates
4. **Security Headers**: Implement proper CORS and security headers
5. **Token Lifetime**: Configure appropriate token lifetimes for security
6. **Rate Limiting**: Implement rate limiting on authentication endpoints

## Next Steps

1. **Test the Changes**: Restart all services and test the authentication flow
2. **Monitor Logs**: Check both IdentityServer and backend API logs
3. **Verify Claims**: Use the enhanced debugging in UserController to verify claims
4. **Frontend Testing**: Ensure frontend can successfully authenticate and call APIs
5. **Performance Testing**: Test under load to ensure the changes don't impact performance

## Support Resources

- [Duende IdentityServer Documentation](https://docs.duendesoftware.com/identityserver/v6)
- [ASP.NET Core Authentication Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT.io Token Debugger](https://jwt.io/)
- [OAuth 2.0 RFC](https://tools.ietf.org/html/rfc6749)