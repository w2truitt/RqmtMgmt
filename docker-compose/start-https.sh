#!/bin/bash

# Script to start the Requirements Management system with HTTPS support
# Uses self-signed certificates for local development

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERTS_DIR="$SCRIPT_DIR/certs"

echo "🚀 Starting Requirements Management System with HTTPS..."

# Check if certificates exist
if [ ! -f "$CERTS_DIR/server-cert.pem" ] || [ ! -f "$CERTS_DIR/server-key.pem" ]; then
    echo "❌ SSL certificates not found!"
    echo "📝 Please run ./create-certificates.sh first to generate certificates."
    exit 1
fi

# Check if domain is in hosts file
if ! grep -q "rqmtmgmt.local" /etc/hosts; then
    echo "❌ Domain not found in /etc/hosts!"
    echo "📝 Please add 'rqmtmgmt.local' to your hosts file:"
    echo "   echo '127.0.0.1 rqmtmgmt.local' | sudo tee -a /etc/hosts"
    exit 1
fi

# Start services with SSL configuration
echo "📦 Starting containers with HTTPS support..."
docker-compose -f docker-compose-ssl.yml up --build

echo ""
echo "✅ Requirements Management System started with HTTPS!"
echo ""
echo "🌐 Application URLs:"
echo "  HTTPS: https://rqmtmgmt.local"
echo "  HTTP:  http://rqmtmgmt.local (redirects to HTTPS)"
echo ""
echo "🔐 IdentityServer URLs:"
echo "  Discovery: https://rqmtmgmt.local/.well-known/openid-configuration"
echo "  Authorize: https://rqmtmgmt.local/connect/authorize"
echo ""
echo "🔗 API URLs:"
echo "  Swagger: https://rqmtmgmt.local/swagger"
echo "  API:     https://rqmtmgmt.local/api"
echo ""
echo "📋 Note: You'll need to import the CA certificate into your browser"
echo "   CA Certificate: $CERTS_DIR/ca-cert.pem"
