#!/bin/bash
# Build script for v1.0.2 - Hierarchical Sections Service Layer Complete

set -e

BACKEND_VERSION="v1.0.2"
FRONTEND_VERSION="v1.0.4"

echo "======================================================================="
echo "Building v1.0.2 - Hierarchical Sections Service Layer"
echo "======================================================================="
echo ""
echo "Changes in this version:"
echo "  Backend v1.0.2:"
echo "    - Hierarchical section query methods implemented"
echo "    - Move and reorder section operations"
echo "    - Circular reference validation"
echo "    - Automatic section numbering"
echo "    - Recursive requirement counting"
echo "    - RqmtMgmtShared upgraded to 1.0.40"
echo ""
echo "  Frontend v1.0.4:"
echo "    - Updated service to call new hierarchical endpoints"
echo "    - RqmtMgmtShared upgraded to 1.0.40"
echo ""

# Build Backend
echo "Step 1: Building backend Docker image..."
cd "$(dirname "$0")/backend"
docker build -f Dockerfile.k8s -t localhost:5000/rqmtmgmt-backend:${BACKEND_VERSION} .

if [ $? -eq 0 ]; then
    echo "✓ Backend image built successfully"
    docker push localhost:5000/rqmtmgmt-backend:${BACKEND_VERSION}
    docker tag localhost:5000/rqmtmgmt-backend:${BACKEND_VERSION} localhost:5000/rqmtmgmt-backend:latest
    docker push localhost:5000/rqmtmgmt-backend:latest
    echo "✓ Backend image pushed"
else
    echo "✗ Backend build failed"
    exit 1
fi

echo ""

# Build Frontend  
echo "Step 2: Building frontend Docker image..."
cd ../frontend
docker build -f Dockerfile.k8s -t localhost:5000/rqmtmgmt-frontend:${FRONTEND_VERSION} .

if [ $? -eq 0 ]; then
    echo "✓ Frontend image built successfully"
    docker push localhost:5000/rqmtmgmt-frontend:${FRONTEND_VERSION}
    docker tag localhost:5000/rqmtmgmt-frontend:${FRONTEND_VERSION} localhost:5000/rqmtmgmt-frontend:latest
    docker push localhost:5000/rqmtmgmt-frontend:latest
    echo "✓ Frontend image pushed"
else
    echo "✗ Frontend build failed"
    exit 1
fi

echo ""
echo "======================================================================="
echo "Build Complete!"
echo "======================================================================="
echo ""
echo "Backend:  localhost:5000/rqmtmgmt-backend:${BACKEND_VERSION}"
echo "Frontend: localhost:5000/rqmtmgmt-frontend:${FRONTEND_VERSION}"
echo ""
echo "Next steps:"
echo "  1. Update k8s/local/backend-deployment.yaml to use ${BACKEND_VERSION}"
echo "  2. Update k8s/local/frontend-deployment.yaml to use ${FRONTEND_VERSION}"
echo "  3. Apply deployments:"
echo "     kubectl apply -f k8s/local/backend-deployment.yaml"
echo "     kubectl apply -f k8s/local/frontend-deployment.yaml"
echo "  4. Wait for pods to restart"
echo "  5. Test hierarchical endpoints"
echo ""
