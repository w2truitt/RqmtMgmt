# Database Management for Kubernetes Development

## Quick Start Database Commands

### **Option 1: Use Existing Docker Compose (Recommended)**
```bash
# Start database (same as before)
cd docker-compose/
docker-compose up -d db

# Stop database
docker-compose down db
```

### **Option 2: Use Dedicated Database Script**
```bash
# Start database (handles both docker-compose and standalone)
./scripts/start-database.sh

# This script will:
# 1. Try to use existing docker-compose setup first
# 2. Fall back to standalone container if needed
# 3. Test connectivity before completing
```

### **Option 3: Manual Standalone Container**
```bash
# Start standalone container
docker run -d --name local-mssql \
  -e SA_PASSWORD=Your_password123 \
  -e ACCEPT_EULA=Y \
  -p 1433:1433 \
  -v sqlvolume:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest

# Stop standalone container  
docker stop local-mssql
docker rm local-mssql
```

## Connection Details

All options use the same connection settings:
- **Server**: `localhost,1433`
- **Database**: `RqmtMgmt` 
- **User**: `sa`
- **Password**: `Your_password123`

## Complete Kubernetes Workflow

### **Daily Development Workflow**
```bash
# 1. Start database (choose your preferred method)
cd docker-compose/ && docker-compose up -d db
# OR
./scripts/start-database.sh

# 2. Build Kubernetes images
./scripts/build-k8s-images.sh

# 3. Deploy to Kubernetes
./scripts/deploy-local-k8s.sh

# 4. Access application
# https://rqmtmgmt.local
```

### **Cleanup**
```bash
# Stop Kubernetes deployment
./scripts/cleanup-k8s.sh

# Stop database
cd docker-compose/ && docker-compose down
# OR
docker stop local-mssql && docker rm local-mssql
```

## Database Persistence

- **Docker Compose**: Uses named volume `sqlvolume` (persistent)
- **Standalone**: Uses named volume `sqlvolume` (persistent) 
- **Data survives** container restarts in both cases

## Troubleshooting

### Database Won't Start
```bash
# Check if port is in use
netstat -an | grep 1433

# Remove existing containers
docker rm -f local-mssql
docker-compose -f docker-compose/docker-compose.yml down db

# Check Docker logs
docker logs local-mssql
docker-compose -f docker-compose/docker-compose.yml logs db
```

### Connection Issues from Kubernetes
```bash
# Test connectivity from Kubernetes pod
kubectl exec -it deployment/backend -n rqmtmgmt -- nslookup host.docker.internal

# Check if database is accessible
kubectl exec -it deployment/backend -n rqmtmgmt -- curl -v telnet://host.docker.internal:1433
```

## Recommendation

**Use the existing Docker Compose setup** for consistency:
```bash
cd docker-compose/
docker-compose up -d db
```

This maintains the same workflow you're already familiar with and uses the same volume and network configuration as your current development setup.