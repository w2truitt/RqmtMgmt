# Kubernetes Development Workflow

## Overview

This document describes the development workflow for running RqmtMgmt in local Rancher Desktop Kubernetes, replacing the nginx-based Docker Compose setup with Traefik ingress.

## Prerequisites

### Required Software
- **Rancher Desktop** with Kubernetes enabled
- **Docker** (included with Rancher Desktop)
- **kubectl** (included with Rancher Desktop)
- **.NET 8/9 SDK** (for building applications)
- **OpenSSL** (for generating TLS certificates)

### Rancher Desktop Configuration
1. **Enable Kubernetes**: Settings → Kubernetes → Enable Kubernetes
2. **Enable Container Registry**: Settings → Container Engine → Enable registry
3. **Resource Allocation**: Allocate at least 8GB RAM and 4 CPUs

## Initial Setup

### 1. Prepare External Database
```bash
# Start SQL Server container (external to Kubernetes)
docker run -d --name local-mssql \
  -e SA_PASSWORD=Your_password123 \
  -e ACCEPT_EULA=Y \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Configure Local DNS
```bash
# Add to /etc/hosts (Linux/macOS) or C:\Windows\System32\drivers\etc\hosts (Windows)
echo "127.0.0.1    rqmtmgmt.local" | sudo tee -a /etc/hosts
```

### 3. Build and Deploy
```bash
# Build production Docker images
./scripts/build-k8s-images.sh

# Deploy to Kubernetes
./scripts/deploy-local-k8s.sh
```

## Development Workflow

### Daily Development Cycle

1. **Make Code Changes**
   - Edit source code in `backend/`, `frontend/`, or `identityserver/`
   - Update shared library in `RqmtMgmtShared/` if needed

2. **Rebuild and Redeploy**
   ```bash
   # Quick rebuild and redeploy
   ./scripts/build-k8s-images.sh
   kubectl rollout restart deployment/backend -n rqmtmgmt
   kubectl rollout restart deployment/frontend -n rqmtmgmt
   ```

3. **Monitor Deployment**
   ```bash
   # Watch deployment status
   kubectl get pods -n rqmtmgmt -w
   
   # View logs
   kubectl logs -f deployment/backend -n rqmtmgmt
   kubectl logs -f deployment/frontend -n rqmtmgmt
   ```

### Hot Reload Development (Optional)

For faster development cycles, you can mount source code directly:

```yaml
# Add to deployment (development only)
volumeMounts:
- name: source-code
  mountPath: /src
volumes:
- name: source-code
  hostPath:
    path: /path/to/your/backend
```

## Application Access

### URLs
- **Frontend**: https://rqmtmgmt.local
- **Backend API**: https://rqmtmgmt.local/api
- **Swagger**: https://rqmtmgmt.local/swagger
- **Health Checks**: https://rqmtmgmt.local/health
- **IdentityServer**: https://rqmtmgmt.local/.well-known/openid_configuration

### Direct Service Access (for debugging)
```bash
# Port forward to access services directly
kubectl port-forward service/backend-service 8080:80 -n rqmtmgmt
kubectl port-forward service/frontend-service 8081:80 -n rqmtmgmt
kubectl port-forward service/identityserver-service 8082:80 -n rqmtmgmt
```

## Database Management

### Running Migrations
```bash
# Option 1: From local development environment
cd backend/
dotnet ef database update

# Option 2: Using kubectl exec (if migration container is available)
kubectl exec -it deployment/backend -n rqmtmgmt -- dotnet ef database update
```

### Connecting to Database
```bash
# Connect using SQL Server Management Studio or Azure Data Studio
Server: localhost,1433
Database: RqmtMgmt
User: sa
Password: Your_password123
```

## Testing

### Running Tests in Kubernetes Environment
```bash
# Run all tests
dotnet test RqmtMgmt.sln

# Run specific test suites
dotnet test backend.Tests/
dotnet test backend.ApiTests/
dotnet test frontend.ComponentTests/

# E2E tests (update URLs to use Kubernetes endpoints)
dotnet test frontend.E2ETests/
```

### Health Checks
```bash
# Check application health
curl -k https://rqmtmgmt.local/health

# Check individual services
kubectl exec -it deployment/backend -n rqmtmgmt -- curl http://localhost/health
```

## Troubleshooting

### Common Issues

1. **Images Not Found**
   ```bash
   # Verify images are built and available
   docker images | grep rqmtmgmt
   
   # Rebuild if necessary
   ./scripts/build-k8s-images.sh
   ```

2. **Database Connection Issues**
   ```bash
   # Check if SQL Server is running
   docker ps | grep mssql
   
   # Check connectivity from pod
   kubectl exec -it deployment/backend -n rqmtmgmt -- nslookup host.docker.internal
   ```

3. **Certificate Issues**
   ```bash
   # Regenerate TLS certificate
   ./scripts/deploy-local-k8s.sh  # Will regenerate cert automatically
   ```

4. **DNS Resolution Issues**
   ```bash
   # Verify /etc/hosts entry
   cat /etc/hosts | grep rqmtmgmt.local
   
   # Test DNS resolution
   nslookup rqmtmgmt.local
   ```

### Debugging Commands

```bash
# View all resources
kubectl get all -n rqmtmgmt

# Describe problematic pod
kubectl describe pod <pod-name> -n rqmtmgmt

# View events
kubectl get events -n rqmtmgmt --sort-by='.lastTimestamp'

# Check ingress
kubectl describe ingress rqmtmgmt-ingress -n rqmtmgmt

# View Traefik dashboard (if enabled)
kubectl port-forward -n kube-system service/traefik 9000:9000
# Access: http://localhost:9000/dashboard/
```

## Performance Monitoring

### Resource Usage
```bash
# View resource usage
kubectl top pods -n rqmtmgmt
kubectl top nodes

# View resource limits
kubectl describe deployment backend -n rqmtmgmt | grep -A 10 "Limits:"
```

### Scaling
```bash
# Scale deployments
kubectl scale deployment/backend --replicas=2 -n rqmtmgmt
kubectl scale deployment/frontend --replicas=2 -n rqmtmgmt

# Auto-scaling (if HPA is configured)
kubectl autoscale deployment backend --cpu-percent=50 --min=1 --max=10 -n rqmtmgmt
```

## Cleanup

### Remove Deployment
```bash
# Complete cleanup
./scripts/cleanup-k8s.sh

# Or manual cleanup
kubectl delete namespace rqmtmgmt
```

### Reset Environment
```bash
# Stop external database
docker stop local-mssql
docker rm local-mssql

# Remove images
docker rmi localhost:5000/rqmtmgmt-backend:latest
docker rmi localhost:5000/rqmtmgmt-frontend:latest
docker rmi localhost:5000/rqmtmgmt-identity:latest
```

## Migration from Docker Compose

### Key Differences

| Aspect | Docker Compose | Kubernetes |
|--------|----------------|------------|
| **Networking** | Container names | Service names |
| **Load Balancing** | nginx | Traefik ingress |
| **Configuration** | Environment files | ConfigMaps/Secrets |
| **Service Discovery** | Built-in | DNS-based |
| **Health Checks** | Docker healthcheck | Kubernetes probes |
| **Scaling** | Manual | Automatic (HPA) |

### Configuration Changes

1. **Service URLs**: Replace container names with service names
2. **Health Endpoints**: Add Kubernetes-specific health check endpoints
3. **Configuration**: Move from `.env` files to ConfigMaps
4. **Secrets**: Use Kubernetes Secrets instead of environment variables

## Next Steps

1. **Azure Migration**: Adapt these manifests for Azure AKS deployment
2. **CI/CD Integration**: Set up automated builds and deployments
3. **Monitoring**: Add Prometheus/Grafana for observability
4. **Security**: Implement network policies and security scanning

---

**Last Updated**: December 2024  
**Environment**: Rancher Desktop Kubernetes  
**Ingress**: Traefik