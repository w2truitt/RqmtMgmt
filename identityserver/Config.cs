using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace identityserver;

/// <summary>
/// Configuration for IdentityServer clients, resources, and scopes.
/// </summary>
public static class Config
{
    /// <summary>
    /// Identity resources that can be requested by clients.
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };

    /// <summary>
    /// API scopes that can be requested by clients.
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("rqmtmgmt.api", "Requirements Management API")
            {
                UserClaims = new List<string> { "role", "email", "name" }
            }
        };

    /// <summary>
    /// API resources protected by this IdentityServer.
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("rqmtmgmt-api", "Requirements Management API")
            {
                Scopes = { "rqmtmgmt.api" },
                UserClaims = new List<string> { "role", "email", "name" }
            }
        };

    /// <summary>
    /// Clients that can request tokens from this IdentityServer.
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // Requirements Management Frontend (Blazor WebAssembly)
            new Client
            {
                ClientId = "rqmtmgmt-frontend",
                ClientName = "Requirements Management Frontend",
                
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,

                RedirectUris =
                {
                    "http://localhost/authentication/login-callback",
                    "http://localhost:5001/authentication/login-callback",
                    "https://localhost:5001/authentication/login-callback"
                },

                PostLogoutRedirectUris =
                {
                    "http://localhost/",
                    "http://localhost:5001/",
                    "https://localhost:5001/"
                },

                AllowedCorsOrigins =
                {
                    "http://localhost",
                    "http://localhost:5001",
                    "https://localhost:5001"
                },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "role",
                    "rqmtmgmt.api"
                },

                AllowAccessTokensViaBrowser = true,
                RequireConsent = false,
                AccessTokenLifetime = 3600, // 1 hour
                IdentityTokenLifetime = 300, // 5 minutes
                
                // Allow refresh tokens
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 86400 // 24 hours
            },

            // Backend API Client (for machine-to-machine communication if needed)
            new Client
            {
                ClientId = "rqmtmgmt-backend",
                ClientName = "Requirements Management Backend",
                ClientSecrets = { new Secret("backend-secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.ClientCredentials,

                AllowedScopes =
                {
                    "rqmtmgmt.api"
                }
            },

            // Swagger UI Client (for API documentation)
            new Client
            {
                ClientId = "swagger-ui",
                ClientName = "Swagger UI",
                
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,

                RedirectUris =
                {
                    "http://localhost/swagger/oauth2-redirect.html",
                    "http://localhost:5000/swagger/oauth2-redirect.html"
                },

                AllowedCorsOrigins =
                {
                    "http://localhost",
                    "http://localhost:5000"
                },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "rqmtmgmt.api"
                },

                AllowAccessTokensViaBrowser = true,
                RequireConsent = false
            }
        };
}