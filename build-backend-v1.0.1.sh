#!/bin/bash
# Build script for backend v1.0.1 with hierarchical sections migration

set -e

VERSION="v1.0.1"
IMAGE_NAME="localhost:5000/rqmtmgmt-backend:${VERSION}"

echo "==============================================="
echo "Building Backend v1.0.1"
echo "==============================================="
echo ""
echo "Changes in this version:"
echo "  - Added hierarchical subsections support"
echo "  - Migration: 20251002151558_AddHierarchicalSections"
echo "  - RqmtMgmtShared upgraded to 1.0.39"
echo ""

# Navigate to backend directory
cd "$(dirname "$0")/backend"

echo "Step 1: Building Docker image..."
docker build -f Dockerfile.k8s -t ${IMAGE_NAME} .

if [ $? -eq 0 ]; then
    echo "✓ Docker image built successfully: ${IMAGE_NAME}"
else
    echo "✗ Docker build failed"
    exit 1
fi

echo ""
echo "Step 2: Pushing to local registry..."
docker push ${IMAGE_NAME}

if [ $? -eq 0 ]; then
    echo "✓ Image pushed successfully to local registry"
else
    echo "✗ Docker push failed"
    exit 1
fi

echo ""
echo "Step 3: Tagging as latest..."
docker tag ${IMAGE_NAME} localhost:5000/rqmtmgmt-backend:latest
docker push localhost:5000/rqmtmgmt-backend:latest

echo ""
echo "==============================================="
echo "Build Complete!"
echo "==============================================="
echo ""
echo "Image: ${IMAGE_NAME}"
echo ""
echo "Next steps:"
echo "  1. Update k8s/local/backend-deployment.yaml to use ${VERSION}"
echo "  2. Apply deployment: kubectl apply -f k8s/local/backend-deployment.yaml"
echo "  3. Wait for pod to restart and apply migration"
echo "  4. Verify migration: kubectl logs -n rqmtmgmt deployment/backend -f"
echo ""
