#!/bin/bash

# Independent Identity Server version management script
# Usage: ./scripts/version-identity.sh [patch|minor|major] [version]

set -e

COMPONENT="identityserver"
VERSION_FILE="identityserver/VERSION"
DEPLOYMENT_FILE="k8s/local/identity-deployment.yaml"

# Create component version file if it doesn't exist
if [ ! -f "$VERSION_FILE" ]; then
    echo "1.0.0" > "$VERSION_FILE"
    echo "Created initial $VERSION_FILE"
fi

CURRENT_VERSION=$(cat $VERSION_FILE)

# Parse version components
IFS='.' read -ra VERSION_PARTS <<< "$CURRENT_VERSION"
MAJOR=${VERSION_PARTS[0]}
MINOR=${VERSION_PARTS[1]}
PATCH=${VERSION_PARTS[2]}

# Allow specific version override
if [ -n "$2" ]; then
    NEW_VERSION="$2"
    echo "Setting Identity Server version to specified version: $NEW_VERSION"
else
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
            echo "Usage: $0 [patch|minor|major] [specific_version]"
            echo "Current Identity Server version: $CURRENT_VERSION"
            echo ""
            echo "Examples:"
            echo "  $0 patch           # 1.0.0 -> 1.0.1"
            echo "  $0 minor           # 1.0.0 -> 1.1.0"
            echo "  $0 major           # 1.0.0 -> 2.0.0"
            echo "  $0 patch 1.0.1     # Set to specific version 1.0.1"
            exit 1
            ;;
    esac
    NEW_VERSION="$MAJOR.$MINOR.$PATCH"
fi

echo "🔄 Bumping Identity Server version from $CURRENT_VERSION to $NEW_VERSION"

# Update component VERSION file
echo $NEW_VERSION > $VERSION_FILE

# Update Kubernetes deployment file for Identity Server only
sed -i "s/rqmtmgmt-identity:.*/rqmtmgmt-identity:v$NEW_VERSION/g" $DEPLOYMENT_FILE

echo "✅ Updated Identity Server version to v$NEW_VERSION"
echo ""
echo "📋 Next steps:"
echo ""
echo "1️⃣  Build and tag Identity Server image:"
echo "   docker build -f identityserver/Dockerfile.k8s -t localhost:5000/rqmtmgmt-identity:v$NEW_VERSION ./identityserver"
echo ""
echo "2️⃣  Push image:"
echo "   docker push localhost:5000/rqmtmgmt-identity:v$NEW_VERSION"
echo ""
echo "3️⃣  Deploy updated Identity Server:"
echo "   kubectl apply -f k8s/local/identity-deployment.yaml"
echo ""
echo "4️⃣  Verify deployment:"
echo "   kubectl get pods -l app=identityserver -n rqmtmgmt"
echo "   kubectl logs -l app=identityserver -n rqmtmgmt --tail=50"
echo ""
echo "5️⃣  Test the updated login flow:"
echo "   curl -k https://rqmtmgmt.local/Account/Login"
echo ""
echo "6️⃣  (Optional) Commit changes:"
echo "   git add identityserver/VERSION k8s/local/identity-deployment.yaml"
echo "   git commit -m 'Bump Identity Server to v$NEW_VERSION - OIDC ReturnUrl fix'"
echo ""
echo "🏷️  Component Versions:"
echo "   Identity Server: v$NEW_VERSION"
echo "   Backend: v$(cat VERSION 2>/dev/null || echo 'global')"
echo "   Frontend: v$(cat VERSION 2>/dev/null || echo 'global')"