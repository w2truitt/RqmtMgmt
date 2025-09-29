# Version Management

This project uses semantic versioning for Docker images deployed to Kubernetes.

## Current Version
See `VERSION` file for current version.

## Version Format
`MAJOR.MINOR.PATCH` (e.g., `1.0.0`)

## Version Bumping

Use the version script to bump versions and update deployments:

```bash
# Patch version (1.0.0 -> 1.0.1)
./scripts/version.sh patch

# Minor version (1.0.0 -> 1.1.0)
./scripts/version.sh minor

# Major version (1.0.0 -> 2.0.0)
./scripts/version.sh major
```

The script will:
- Update the `VERSION` file
- Update all Kubernetes deployment files with new version tags
- Provide build and deployment instructions

## Deployment Process

After bumping version:

1. **Build images with new tags:**
   ```bash
   docker build -f backend/Dockerfile.k8s -t localhost:5000/rqmtmgmt-backend:v1.0.0 .
   docker build -f frontend/Dockerfile -t localhost:5000/rqmtmgmt-frontend:v1.0.0 ./frontend
   docker build -f identityserver/Dockerfile.k8s -t localhost:5000/rqmtmgmt-identity:v1.0.0 ./identityserver
   ```

2. **Push images:**
   ```bash
   docker push localhost:5000/rqmtmgmt-backend:v1.0.0
   docker push localhost:5000/rqmtmgmt-frontend:v1.0.0
   docker push localhost:5000/rqmtmgmt-identity:v1.0.0
   ```

3. **Deploy to Kubernetes:**
   ```bash
   kubectl apply -f k8s/local/
   ```

4. **Commit changes:**
   ```bash
   git add VERSION k8s/local/
   git commit -m "Bump version to v1.0.0"
   ```

## Why Version Tags?

Using specific version tags instead of `latest` ensures:
- Kubernetes always pulls new images when deployments change
- Rollbacks to specific versions are possible
- Image immutability and reproducibility
- Better deployment tracking and debugging