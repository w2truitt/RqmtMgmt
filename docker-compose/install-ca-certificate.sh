#!/bin/bash

# Script to install the self-signed CA certificate into the system trust store
# This allows the certificate to be trusted by browsers and applications

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CA_CERT="$SCRIPT_DIR/certs/ca-cert.pem"

echo "🔐 Installing CA certificate into system trust store..."

# Check if CA certificate exists
if [ ! -f "$CA_CERT" ]; then
    echo "❌ CA certificate not found at: $CA_CERT"
    echo "📝 Please run ./create-certificates.sh first to generate certificates."
    exit 1
fi

# Install on Ubuntu/Debian
if command -v update-ca-certificates &> /dev/null; then
    echo "📦 Installing CA certificate for Ubuntu/Debian..."
    
    # Copy CA certificate to system trust store
    sudo cp "$CA_CERT" /usr/local/share/ca-certificates/rqmtmgmt-ca.crt
    
    # Update system trust store
    sudo update-ca-certificates
    
    echo "✅ CA certificate installed successfully!"
    echo "📋 Certificate installed as: /usr/local/share/ca-certificates/rqmtmgmt-ca.crt"

# Install on Red Hat/CentOS/Fedora
elif command -v update-ca-trust &> /dev/null; then
    echo "📦 Installing CA certificate for Red Hat/CentOS/Fedora..."
    
    # Copy CA certificate to system trust store
    sudo cp "$CA_CERT" /etc/pki/ca-trust/source/anchors/rqmtmgmt-ca.pem
    
    # Update system trust store
    sudo update-ca-trust
    
    echo "✅ CA certificate installed successfully!"
    echo "📋 Certificate installed as: /etc/pki/ca-trust/source/anchors/rqmtmgmt-ca.pem"

else
    echo "❌ Unknown Linux distribution. Please manually install the CA certificate."
    echo "📋 CA certificate location: $CA_CERT"
    exit 1
fi

echo ""
echo "🌐 Browser Installation:"
echo "  For additional browser support, you may need to manually import the CA certificate:"
echo "  Certificate: $CA_CERT"
echo ""
echo "  Chrome/Chromium:"
echo "    1. Go to Settings > Privacy and security > Security > Manage certificates"
echo "    2. Go to 'Authorities' tab"
echo "    3. Click 'Import' and select the CA certificate"
echo ""
echo "  Firefox:"
echo "    1. Go to Settings > Privacy & Security > Certificates > View Certificates"
echo "    2. Go to 'Authorities' tab"
echo "    3. Click 'Import' and select the CA certificate"
echo ""
echo "🎉 Installation complete! The certificate should now be trusted by most applications."
