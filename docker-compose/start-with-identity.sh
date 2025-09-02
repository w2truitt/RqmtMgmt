#!/bin/bash

# Requirements Management System with Identity Provider
# This script starts the complete system with Duende IdentityServer

echo "🚀 Starting Requirements Management System with Identity Provider..."
echo ""
echo "Services that will be started:"
echo "  - IdentityServer (OAuth2/OIDC) on port 5002"
echo "  - Backend API on port 5000"
echo "  - Frontend (Blazor) on port 5001"
echo "  - SQL Server Database on port 1433"
echo "  - Nginx Reverse Proxy on port 80"
echo ""
echo "Access the application at: http://localhost"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Navigate to docker-compose directory
cd "$(dirname "$0")"

echo "🔧 Building and starting services..."
echo "Note: First startup may take several minutes to download images and build..."
echo ""

# Start the services
docker compose -f docker-compose.identity.yml up --build

echo ""
echo "🎉 System startup complete!"
echo ""
echo "📋 Test Users:"
echo "  Administrator: admin@rqmtmgmt.local / Admin123!"
echo "  Project Manager: pm@rqmtmgmt.local / PM123!"
echo "  Developer: dev@rqmtmgmt.local / Dev123!"
echo "  Tester: tester@rqmtmgmt.local / Test123!"
echo "  Viewer: viewer@rqmtmgmt.local / View123!"
echo ""
echo "🔗 URLs:"
echo "  Main Application: http://localhost"
echo "  API Documentation: http://localhost/swagger"
echo "  Identity Server: http://localhost/identity"
echo "  Account Login: http://localhost/Account/Login"
echo ""