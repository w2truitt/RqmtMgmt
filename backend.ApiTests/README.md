# API Integration Tests

This document explains how to run the API integration tests against the deployed application.

## Overview

The API integration tests run against the deployed application at `https://rqmtmgmt.local` and test the real API endpoints with authentication.

## Prerequisites

The tests require either:
- **Docker Compose**: All services running including nginx proxy
- **Kubernetes**: Full deployment with ingress/proxy

Both deployments serve the application on `https://rqmtmgmt.local` with the same API endpoints and authentication.

## Running the Tests

### Start Your Deployment

**For Docker Compose:**
```bash
cd docker-compose
docker-compose up -d
```

**For Kubernetes:**
```bash
./scripts/deploy-local-k8s.sh
```

### Verify Deployment is Ready

Check that the application is accessible:
```bash
curl -k https://rqmtmgmt.local/health
```

### Run the Tests

```bash
cd backend.ApiTests
dotnet test --logger "console;verbosity=detailed"
```

## Configuration

The tests use the following configuration:
- **Base URL**: `https://rqmtmgmt.local`
- **OAuth2 Client ID**: `rqmtmgmt-backend`
- **OAuth2 Client Secret**: `backend-secret`
- **OAuth2 Scope**: `rqmtmgmt.api`
- **Token Endpoint**: `https://rqmtmgmt.local/connect/token`

## Troubleshooting

### Common Issues

1. **Connection Refused**: Ensure your deployment is running and nginx/proxy is started
2. **SSL Certificate Errors**: Tests bypass SSL validation, but ensure the proxy is serving HTTPS
3. **Authentication Failures**: Check that the Identity Server is running and accessible
4. **DNS Issues**: Ensure `rqmtmgmt.local` resolves to `127.0.0.1` in your hosts file

### Docker Compose Specific

- **All services must be running**: Backend, Identity Server, and **nginx proxy**
- **Check service status**: `docker-compose -f docker-compose/docker-compose.yml ps`
- **View logs**: `docker-compose -f docker-compose/docker-compose.yml logs [service-name]`

### Kubernetes Specific

- **Check pod status**: `kubectl get pods`
- **Check ingress**: `kubectl get ingress`
- **View logs**: `kubectl logs [pod-name]`

## Test Structure

### Base Classes
- `BaseIntegrationTest`: Base class with automatic authentication and health checks
- `IntegrationTestCollection`: Ensures tests run sequentially to avoid conflicts

### Key Features
- **Automatic Authentication**: OAuth2 client credentials flow
- **Health Check Verification**: Ensures deployment is ready before running tests
- **SSL Bypass**: For local development with self-signed certificates
- **Sequential Execution**: Prevents test interference with shared database

## Adding New Tests

When creating new integration tests:

1. Inherit from `BaseIntegrationTest`
2. Add the `[Collection("Integration Tests")]` attribute
3. Use `await SkipIfSystemNotAvailableAsync()` at the start of each test method
4. Use the `_client` field for HTTP requests (already configured with authentication)

Example:
```csharp
[Collection("Integration Tests")]
public class MyNewIntegrationTests : BaseIntegrationTest
{
    [Fact]
    public async Task MyTest_ShouldWork_WhenSystemIsRunning()
    {
        // Arrange
        await SkipIfSystemNotAvailableAsync();

        // Act
        var response = await _client.GetAsync("/api/myendpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## Important Notes

- Tests run against the **real database** - be careful with data modifications
- Tests are **sequential** to prevent conflicts with shared resources
- Both Docker Compose and Kubernetes use the **same test configuration**
- The nginx proxy (Docker Compose) or ingress (Kubernetes) **must be running** for tests to work