#!/bin/bash

# Script to create a self-signed CA and server certificates for local development
# This creates certificates for rqmtmgmt.local domain with proper key usage

set -e

CERTS_DIR="./certs"
CA_KEY="$CERTS_DIR/ca-key.pem"
CA_CERT="$CERTS_DIR/ca-cert.pem"
SERVER_KEY="$CERTS_DIR/server-key.pem"
SERVER_CERT="$CERTS_DIR/server-cert.pem"
SERVER_CSR="$CERTS_DIR/server.csr"

# Create certificates directory
mkdir -p "$CERTS_DIR"

echo "🔐 Creating self-signed Certificate Authority and server certificates..."

# Generate CA private key
echo "📝 Generating CA private key..."
openssl genrsa -out "$CA_KEY" 4096

# Generate CA certificate
echo "📝 Generating CA certificate..."
openssl req -new -x509 -days 365 -key "$CA_KEY" -out "$CA_CERT" \
    -subj "/C=US/ST=Development/L=Local/O=RqmtMgmt Development/OU=IT Department/CN=RqmtMgmt CA"

# Generate server private key
echo "📝 Generating server private key..."
openssl genrsa -out "$SERVER_KEY" 4096

# Create server certificate configuration with CORRECT key usage
echo "📝 Creating server certificate configuration..."
cat > "$CERTS_DIR/server.conf" << EOF
[req]
distinguished_name = req_distinguished_name
req_extensions = v3_req
prompt = no

[req_distinguished_name]
C=US
ST=Development
L=Local
O=RqmtMgmt Development
OU=IT Department
CN=rqmtmgmt.local

[v3_req]
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names
basicConstraints = CA:FALSE

[alt_names]
DNS.1 = rqmtmgmt.local
DNS.2 = localhost
DNS.3 = *.rqmtmgmt.local
IP.1 = 127.0.0.1
IP.2 = ::1
EOF

# Generate server certificate signing request
echo "📝 Generating server certificate signing request..."
openssl req -new -key "$SERVER_KEY" -out "$SERVER_CSR" -config "$CERTS_DIR/server.conf"

# Sign server certificate with CA
echo "📝 Signing server certificate with CA..."
openssl x509 -req -in "$SERVER_CSR" -CA "$CA_CERT" -CAkey "$CA_KEY" -CAcreateserial \
    -out "$SERVER_CERT" -days 365 -extensions v3_req -extfile "$CERTS_DIR/server.conf"

# Set appropriate permissions
chmod 600 "$CA_KEY" "$SERVER_KEY"
chmod 644 "$CA_CERT" "$SERVER_CERT"

# Display certificate information
echo "✅ Certificates created successfully!"
echo ""
echo "📋 Certificate Information:"
echo "  CA Certificate: $CA_CERT"
echo "  Server Certificate: $SERVER_CERT"
echo "  Server Private Key: $SERVER_KEY"
echo ""
echo "🔍 Server Certificate Details:"
openssl x509 -in "$SERVER_CERT" -text -noout | grep -E "(Subject:|DNS:|IP Address:|Key Usage)"
echo ""
echo "📝 Next Steps:"
echo "  1. Add 'rqmtmgmt.local' to your /etc/hosts file pointing to 127.0.0.1"
echo "  2. Import the CA certificate ($CA_CERT) into your browser's trusted root certificates"
echo "  3. Restart docker-compose services"
echo ""
echo "🏠 To add to /etc/hosts, run:"
echo "  echo '127.0.0.1 rqmtmgmt.local' | sudo tee -a /etc/hosts"
echo ""
echo "🔒 To install CA certificate (Ubuntu/Debian):"
echo "  sudo cp $CA_CERT /usr/local/share/ca-certificates/rqmtmgmt-ca.crt"
echo "  sudo update-ca-certificates"

# Clean up temporary files
rm -f "$SERVER_CSR" "$CERTS_DIR/server.conf" "$CERTS_DIR/ca-cert.srl"

echo ""
echo "🎉 Certificate generation complete!"