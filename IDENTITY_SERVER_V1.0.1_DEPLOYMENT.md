# Identity Server v1.0.1 Deployment Guide

## What's New in v1.0.1

### 🔧 OIDC Compliance Fix
- **Issue**: ReturnUrl was required even for direct login page access
- **Fix**: Made ReturnUrl optional with default redirect to home page (`~/`)
- **Impact**: Improves E2E test compatibility and user experience
- **OIDC Compliance**: Now follows OIDC specification correctly

### 📝 Code Changes
- Updated `OnGet(string? returnUrl = null)` to accept optional ReturnUrl
- Added default redirect logic in `BuildModelAsync`
- Added proper validation attributes to `LoginInputModel`
- Added missing using statement for `System.ComponentModel.DataAnnotations`

## Deployment Steps

### 1. Build the Updated Identity Server Image

```bash
cd /home/wtruitt/src/repos/RqmtMgmt

# Build with new version tag
docker build -f identityserver/Dockerfile.k8s -t localhost:5000/rqmtmgmt-identity:v1.0.1 ./identityserver
```

### 2. Push to Local Registry

```bash
# Push the new image
docker push localhost:5000/rqmtmgmt-identity:v1.0.1
```

### 3. Deploy to Kubernetes

```bash
# Apply the updated deployment
kubectl apply -f k8s/local/identity-deployment.yaml

# Or apply entire local config (safe - only identity image tag changed)
kubectl apply -f k8s/local/
```

### 4. Verify Deployment

```bash
# Check pod status
kubectl get pods -l app=identityserver -n rqmtmgmt

# Check logs for startup
kubectl logs -l app=identityserver -n rqmtmgmt --tail=50

# Verify the new image is running
kubectl describe pod -l app=identityserver -n rqmtmgmt | grep Image:
```

### 5. Test the Fix

```bash
# Test direct login page access (should now work without ReturnUrl)
curl -k https://rqmtmgmt.local/Account/Login

# Test E2E authentication
cd /home/wtruitt/src/repos/RqmtMgmt
dotnet test frontend.E2ETests --filter "AuthenticationCoreTests" --logger "console;verbosity=normal"
```

## Independent Versioning

### New Component Versioning System

This deployment introduces **independent component versioning**:

- **Identity Server**: `v1.0.1` (OIDC fix)
- **Backend**: `v1.0.0` (unchanged)
- **Frontend**: `v1.0.0` (unchanged)

### Using the New Version Script

```bash
# Bump Identity Server version independently
./scripts/version-identity.sh patch   # 1.0.1 -> 1.0.2
./scripts/version-identity.sh minor   # 1.0.1 -> 1.1.0
./scripts/version-identity.sh major   # 1.0.1 -> 2.0.0

# Set specific version
./scripts/version-identity.sh patch 1.0.1
```

## Rollback Plan

If issues occur, rollback to previous version:

```bash
# Rollback deployment
kubectl patch deployment identityserver -n rqmtmgmt -p '{"spec":{"template":{"spec":{"containers":[{"name":"identityserver","image":"localhost:5000/rqmtmgmt-identity:v1.0.0"}]}}}}'

# Verify rollback
kubectl get pods -l app=identityserver -n rqmtmgmt
```

## Benefits of This Approach

1. **Independent Deployment**: Only Identity Server updates, no risk to other components
2. **Faster Iterations**: Can fix auth issues without rebuilding entire stack
3. **Better Testing**: Isolate identity changes for targeted testing
4. **Version Control**: Clear tracking of which component has which fixes
5. **Reduced Risk**: Smaller blast radius for changes

## Monitoring After Deployment

Watch for:
- ✅ **Login page loads** without ReturnUrl errors
- ✅ **E2E tests pass** authentication steps
- ✅ **User authentication** works correctly
- ✅ **No regression** in existing functionality

## File Changes Summary

### Modified Files:
- `identityserver/Pages/Account/Login.cshtml.cs` - OIDC compliance fix
- `k8s/local/identity-deployment.yaml` - Version bump to v1.0.1

### New Files:
- `identityserver/VERSION` - Component-specific version tracking
- `scripts/version-identity.sh` - Independent Identity Server versioning
- `IDENTITY_SERVER_V1.0.1_DEPLOYMENT.md` - This deployment guide

This focused deployment approach allows you to get the OIDC authentication fix into production quickly while maintaining system stability.