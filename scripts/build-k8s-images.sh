#!/bin/bash

# Build Kubernetes Images for Local Rancher Desktop
# This script builds production Docker images and pushes them to the local registry

set -e

echo "🐳 Building Kubernetes images for local Rancher Desktop..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
LOCAL_REGISTRY="localhost:5000"
IMAGE_TAG="latest"
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo -e "${BLUE}Project root: ${PROJECT_ROOT}${NC}"

# Function to build and push image
build_and_push() {
    local service=$1
    local dockerfile=$2
    local image_name="${LOCAL_REGISTRY}/rqmtmgmt-${service}:${IMAGE_TAG}"
    
    echo -e "${YELLOW}Building ${service}...${NC}"
    
    # Build the image
    docker build \
        -f "${PROJECT_ROOT}/${dockerfile}" \
        -t "${image_name}" \
        "${PROJECT_ROOT}"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Built ${image_name}${NC}"
    else
        echo -e "${RED}✗ Failed to build ${image_name}${NC}"
        exit 1
    fi
    
    # Push to local registry
    echo -e "${YELLOW}Pushing ${image_name}...${NC}"
    docker push "${image_name}"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Pushed ${image_name}${NC}"
    else
        echo -e "${RED}✗ Failed to push ${image_name}${NC}"
        exit 1
    fi
}

# Check if local registry is available
echo -e "${BLUE}Checking local registry availability...${NC}"
if ! curl -f http://localhost:5000/v2/ > /dev/null 2>&1; then
    echo -e "${RED}✗ Local registry not available at localhost:5000${NC}"
    echo -e "${YELLOW}Make sure Rancher Desktop is running with registry enabled${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Local registry is available${NC}"

# Build shared library first (if needed)
echo -e "${BLUE}Building shared library...${NC}"
cd "${PROJECT_ROOT}/RqmtMgmtShared"
dotnet pack -o ../local_nuget/ --configuration Release
cd "${PROJECT_ROOT}"

# Build all services
build_and_push "backend" "backend/Dockerfile.k8s"
build_and_push "frontend" "frontend/Dockerfile.k8s" 
build_and_push "identity" "identityserver/Dockerfile.k8s"

echo -e "${GREEN}🎉 All images built and pushed successfully!${NC}"
echo -e "${BLUE}Images available:${NC}"
echo -e "  • ${LOCAL_REGISTRY}/rqmtmgmt-backend:${IMAGE_TAG}"
echo -e "  • ${LOCAL_REGISTRY}/rqmtmgmt-frontend:${IMAGE_TAG}"
echo -e "  • ${LOCAL_REGISTRY}/rqmtmgmt-identity:${IMAGE_TAG}"

echo -e "${YELLOW}Next steps:${NC}"
echo -e "  1. Make sure external database is running: ${BLUE}docker run -d --name local-mssql -e SA_PASSWORD=Your_password123 -e ACCEPT_EULA=Y -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest${NC}"
echo -e "  2. Add to /etc/hosts: ${BLUE}127.0.0.1 rqmtmgmt.local${NC}"
echo -e "  3. Deploy to Kubernetes: ${BLUE}./scripts/deploy-local-k8s.sh${NC}"