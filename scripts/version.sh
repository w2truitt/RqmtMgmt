#!/bin/bash

# Version management script for RqmtMgmt Kubernetes deployment
# Usage: ./scripts/version.sh [patch|minor|major]

set -e

VERSION_FILE="VERSION"
CURRENT_VERSION=$(cat $VERSION_FILE)

# Parse version components
IFS='.' read -ra VERSION_PARTS <<< "$CURRENT_VERSION"
MAJOR=${VERSION_PARTS[0]}
MINOR=${VERSION_PARTS[1]}
PATCH=${VERSION_PARTS[2]}

case "${1:-patch}" in
    "patch")
        PATCH=$((PATCH + 1))
        ;;
    "minor")
        MINOR=$((MINOR + 1))
        PATCH=0
        ;;
    "major")
        MAJOR=$((MAJOR + 1))
        MINOR=0
        PATCH=0
        ;;
    *)
        echo "Usage: $0 [patch|minor|major]"
        echo "Current version: $CURRENT_VERSION"
        exit 1
        ;;
esac

NEW_VERSION="$MAJOR.$MINOR.$PATCH"

echo "Bumping version from $CURRENT_VERSION to $NEW_VERSION"

# Update VERSION file
echo $NEW_VERSION > $VERSION_FILE

# Update Kubernetes deployment files
sed -i "s/rqmtmgmt-backend:.*/rqmtmgmt-backend:v$NEW_VERSION/g" k8s/local/backend-deployment-fixed.yaml
sed -i "s/rqmtmgmt-frontend:.*/rqmtmgmt-frontend:v$NEW_VERSION/g" k8s/local/frontend-deployment.yaml
sed -i "s/rqmtmgmt-identity:.*/rqmtmgmt-identity:v$NEW_VERSION/g" k8s/local/identity-deployment.yaml

echo "Updated VERSION file and Kubernetes deployments to v$NEW_VERSION"
echo ""
echo "Next steps:"
echo "1. Build and tag your images:"
echo "   docker build -f backend/Dockerfile.k8s -t localhost:5000/rqmtmgmt-backend:v$NEW_VERSION ."
echo "   docker build -f frontend/Dockerfile -t localhost:5000/rqmtmgmt-frontend:v$NEW_VERSION ./frontend"
echo "   docker build -f identityserver/Dockerfile.k8s -t localhost:5000/rqmtmgmt-identity:v$NEW_VERSION ./identityserver"
echo ""
echo "2. Push images:"
echo "   docker push localhost:5000/rqmtmgmt-backend:v$NEW_VERSION"
echo "   docker push localhost:5000/rqmtmgmt-frontend:v$NEW_VERSION"
echo "   docker push localhost:5000/rqmtmgmt-identity:v$NEW_VERSION"
echo ""
echo "3. Deploy to Kubernetes:"
echo "   kubectl apply -f k8s/local/"
echo ""
echo "4. Commit changes:"
echo "   git add VERSION k8s/local/"
echo "   git commit -m 'Bump version to v$NEW_VERSION'"