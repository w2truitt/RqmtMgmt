using Duende.IdentityServer.Models;

namespace backend.Configuration
{
    /// <summary>
    /// Configuration for Duende IdentityServer clients, resources, and scopes.
    /// Defines OIDC clients for the Blazor WebAssembly frontend and API resources.
    /// </summary>
    public static class IdentityServerConfig
    {
        /// <summary>
        /// Gets the identity resources (user claims) available to clients.
        /// </summary>
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        /// <summary>
        /// Gets the API scopes that can be requested by clients.
        /// </summary>
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("rqmtapi", "Requirements Management API")
                {
                    Description = "Access to the Requirements Management API endpoints"
                }
            };

        /// <summary>
        /// Gets the API resources protected by IdentityServer.
        /// </summary>
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("rqmtapi", "Requirements Management API")
                {
                    Scopes = { "rqmtapi" },
                    UserClaims = { "name", "email", "role" }
                }
            };

        /// <summary>
        /// Gets the clients that can request tokens from IdentityServer.
        /// </summary>
        /// <param name="frontendUrl">The frontend application URL for redirect URIs.</param>
        /// <param name="backendUrl">The backend application URL for CORS origins.</param>
        public static IEnumerable<Client> GetClients(string frontendUrl, string backendUrl) =>
            new Client[]
            {
                // Blazor WebAssembly SPA Client
                new Client
                {
                    ClientId = "rqmtmgmt-wasm",
                    ClientName = "Requirements Management Blazor WASM",
                    ClientUri = frontendUrl,

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false, // Public client
                    RequirePkce = true, // PKCE required for security

                    RedirectUris = 
                    {
                        $"{frontendUrl}/authentication/login-callback",
                        "http://localhost:8080/authentication/login-callback",
                        "https://localhost:7160/authentication/login-callback",
                        "https://localhost:5001/authentication/login-callback",
                        "https://rqmtmgmt.local/authentication/login-callback"
                    },

                    PostLogoutRedirectUris = 
                    {
                        $"{frontendUrl}/authentication/logout-callback",
                        "http://localhost:8080/authentication/logout-callback", 
                        "https://localhost:7160/authentication/logout-callback", 
                        "https://localhost:5001/authentication/logout-callback",
                        "https://rqmtmgmt.local/authentication/logout-callback"
                    },

                    AllowedScopes = 
                    {
                        "openid",
                        "profile", 
                        "email",
                        "rqmtapi"
                    },

                    AllowedCorsOrigins = 
                    {
                        frontendUrl,
                        "http://localhost:8080",
                        "https://localhost:7160",
                        "https://localhost:5001",
                        "http://localhost:5000",
                        "http://localhost:5239",
                        "https://rqmtmgmt.local"
                    },

                    // Token lifetimes
                    AccessTokenLifetime = 3600, // 1 hour
                    IdentityTokenLifetime = 300, // 5 minutes
                    AuthorizationCodeLifetime = 300, // 5 minutes

                    // Additional security settings
                    AllowOfflineAccess = false,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    SlidingRefreshTokenLifetime = 1296000, // 15 days
                    AbsoluteRefreshTokenLifetime = 2592000, // 30 days

                    RequireConsent = false, // No consent screen for first-party app
                    AlwaysIncludeUserClaimsInIdToken = true
                }
            };
    }
}
