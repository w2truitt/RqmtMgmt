#!/bin/bash

# Rancher Desktop Traefik Access Setup Script
# This script sets up port forwarding for Traefik to enable access to rqmtmgmt.local

echo "🚀 Setting up Traefik access for Rancher Desktop..."

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "❌ kubectl is not available. Please ensure Rancher Desktop is running."
    exit 1
fi

# Check if the rqmtmgmt namespace exists
if ! kubectl get namespace rqmtmgmt &> /dev/null; then
    echo "❌ rqmtmgmt namespace not found. Please deploy the application first."
    exit 1
fi

# Kill any existing port-forward processes
echo "🧹 Cleaning up existing port-forward processes..."
pkill -f "kubectl port-forward.*traefik" || true

# Set up port forwarding for Traefik
echo "🔗 Setting up port forwarding for Traefik..."
kubectl port-forward -n kube-system svc/traefik 80:80 443:443 &

# Wait a moment for port forwarding to establish
sleep 3

# Test the connection
echo "🧪 Testing connection..."
if curl -s -k https://rqmtmgmt.local > /dev/null; then
    echo "✅ Success! Your application is accessible at:"
    echo "   🌐 Frontend: https://rqmtmgmt.local"
    echo "   🔧 API Health: https://rqmtmgmt.local/health"  
    echo "   📖 Swagger: https://rqmtmgmt.local/swagger"
    echo "   🔐 Identity: https://rqmtmgmt.local/.well-known/openid_configuration"
    echo ""
    echo "⚠️  Note: You'll get a certificate warning - this is normal for local development"
    echo "⚠️  Note: Keep this terminal open to maintain the connection"
    echo ""
    echo "🛑 To stop the port forwarding, press Ctrl+C or run:"
    echo "   pkill -f 'kubectl port-forward.*traefik'"
else
    echo "❌ Connection test failed. Please check your deployment."
    exit 1
fi

# Keep the script running to maintain port forwarding
echo "🔄 Port forwarding is active. Press Ctrl+C to stop..."
wait