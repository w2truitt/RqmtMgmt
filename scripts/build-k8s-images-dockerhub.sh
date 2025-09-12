#!/bin/bash

# Build Kubernetes Images for Local Rancher Desktop (Docker Hub Version)
# This script builds production Docker images and pushes them to Docker Hub

set -e

echo "🐳 Building Kubernetes images for Docker Hub..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration - CHANGE THIS TO YOUR DOCKER HUB USERNAME
DOCKER_HUB_USERNAME="your-dockerhub-username"
IMAGE_TAG="latest"
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo -e "${BLUE}Project root: ${PROJECT_ROOT}${NC}"

# Check if Docker Hub username is set
if [ "$DOCKER_HUB_USERNAME" = "your-dockerhub-username" ]; then
    echo -e "${RED}✗ Please set your Docker Hub username in the script${NC}"
    echo -e "${YELLOW}Edit this script and change DOCKER_HUB_USERNAME to your actual Docker Hub username${NC}"
    exit 1
fi

# Function to build and push image
build_and_push() {
    local service=$1
    local dockerfile=$2
    local image_name="${DOCKER_HUB_USERNAME}/rqmtmgmt-${service}:${IMAGE_TAG}"
    
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
    
    # Push to Docker Hub
    echo -e "${YELLOW}Pushing ${image_name}...${NC}"
    docker push "${image_name}"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Pushed ${image_name}${NC}"
    else
        echo -e "${RED}✗ Failed to push ${image_name}${NC}"
        exit 1
    fi
}

# Check if logged into Docker Hub
echo -e "${BLUE}Checking Docker Hub authentication...${NC}"
if ! docker info | grep -q "Username:"; then
    echo -e "${YELLOW}Please log in to Docker Hub:${NC}"
    docker login
fi

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
echo -e "  • ${DOCKER_HUB_USERNAME}/rqmtmgmt-backend:${IMAGE_TAG}"
echo -e "  • ${DOCKER_HUB_USERNAME}/rqmtmgmt-frontend:${IMAGE_TAG}"
echo -e "  • ${DOCKER_HUB_USERNAME}/rqmtmgmt-identity:${IMAGE_TAG}"

echo -e "${YELLOW}Next steps:${NC}"
echo -e "  1. Update Kubernetes manifests to use Docker Hub images"
echo -e "  2. Start database: ${BLUE}cd docker-compose && docker-compose up -d db${NC}"
echo -e "  3. Add to /etc/hosts: ${BLUE}127.0.0.1 rqmtmgmt.local${NC}"
echo -e "  4. Deploy to Kubernetes: ${BLUE}./scripts/deploy-local-k8s.sh${NC}"