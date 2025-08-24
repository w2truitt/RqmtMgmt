# HTTPS Configuration for Requirements Management System

This directory contains the HTTPS/SSL configuration for running the Requirements Management System securely with self-signed certificates for local development.

## Overview

The HTTPS setup includes:
- Self-signed Certificate Authority (CA)
- Server certificates for the `rqmtmgmt.local` domain
- Nginx reverse proxy with SSL termination
- Automatic HTTP to HTTPS redirects
- Proper security headers

## Quick Start

1. **Generate certificates:**
   ```bash
   ./create-certificates.sh
   ```

2. **Install CA certificate (optional but recommended):**
   ```bash
   ./install-ca-certificate.sh
   ```

3. **Start the system with HTTPS:**
   ```bash
   ./start-https.sh
   ```

4. **Access the application:**
   - HTTPS: https://rqmtmgmt.local
   - The system will automatically redirect HTTP to HTTPS

## Files Generated

### Certificates
- `certs/ca-cert.pem` - Certificate Authority certificate
- `certs/ca-key.pem` - Certificate Authority private key
- `certs/server-cert.pem` - Server certificate for rqmtmgmt.local
- `certs/server-key.pem` - Server private key

### Configuration
- `nginx-ssl.conf` - Nginx configuration with SSL support
- `docker-compose-ssl.yml` - Docker Compose file with HTTPS setup

## Security Features

### SSL Configuration
- **Protocols:** TLS 1.2 and 1.3 only
- **Ciphers:** Modern, secure cipher suites
- **Perfect Forward Secrecy:** ECDHE key exchange
- **Session Management:** Shared SSL session cache

### Security Headers
- **HSTS:** HTTP Strict Transport Security with 1-year max-age
- **X-Frame-Options:** SAMEORIGIN to prevent clickjacking
- **X-Content-Type-Options:** nosniff to prevent MIME type sniffing
- **X-XSS-Protection:** XSS filtering enabled

### Proxy Configuration
- **X-Forwarded-Proto:** Set to HTTPS for backend services
- **X-Forwarded-Host:** Proper host header forwarding
- **X-Forwarded-Port:** Port 443 for HTTPS
- **WebSocket Support:** For Blazor SignalR if needed

## Certificate Details

### Subject Alternative Names (SANs)
The server certificate includes the following SANs:
- `rqmtmgmt.local`
- `localhost`
- `*.rqmtmgmt.local`
- `127.0.0.1`
- `::1`

### Certificate Validity
- **Duration:** 365 days
- **Key Size:** 4096 bits RSA
- **Signature:** SHA-256

## Browser Trust

### System Trust Store
The `install-ca-certificate.sh` script installs the CA certificate into the system trust store, which will be trusted by:
- curl
- wget
- Most system applications
- Some browsers (depending on configuration)

### Browser-Specific Installation

#### Chrome/Chromium
1. Go to **Settings** > **Privacy and security** > **Security** > **Manage certificates**
2. Click the **Authorities** tab
3. Click **Import** and select `certs/ca-cert.pem`
4. Check "Trust this certificate for identifying websites"

#### Firefox
1. Go to **Settings** > **Privacy & Security** > **Certificates** > **View Certificates**
2. Click the **Authorities** tab
3. Click **Import** and select `certs/ca-cert.pem`
4. Check "Trust this CA to identify websites"

#### Safari (macOS)
1. Double-click `certs/ca-cert.pem` to open Keychain Access
2. Select "System" keychain
3. Double-click the certificate and set trust to "Always Trust"

## Development vs Production

### Development (Current Setup)
- Self-signed certificates
- Local CA for trust
- Domain: `rqmtmgmt.local`
- Suitable for local development only

### Production Recommendations
- Purchase certificates from a trusted CA (Let's Encrypt, DigiCert, etc.)
- Use proper domain names
- Implement certificate renewal automation
- Consider using a WAF (Web Application Firewall)

## Troubleshooting

### Certificate Errors
If you see certificate errors:
1. Ensure `rqmtmgmt.local` is in your `/etc/hosts` file
2. Install the CA certificate using `./install-ca-certificate.sh`
3. Clear browser cache and restart browser
4. Check certificate validity: `openssl x509 -in certs/server-cert.pem -text -noout`

### Connection Issues
If you can't connect:
1. Verify containers are running: `docker-compose -f docker-compose-ssl.yml ps`
2. Check nginx logs: `docker-compose -f docker-compose-ssl.yml logs nginx`
3. Verify port 443 is not blocked by firewall

### IdentityServer Issues
If authentication fails:
1. Check that backend is accessible: `curl -k https://rqmtmgmt.local/api/health`
2. Verify IdentityServer discovery: `curl -k https://rqmtmgmt.local/.well-known/openid-configuration`
3. Check backend logs for any certificate validation errors

## Security Considerations

### Development vs Production
⚠️ **Important:** These are self-signed certificates for development only!

### Production Security Checklist
- [ ] Replace self-signed certificates with trusted CA certificates
- [ ] Implement certificate renewal automation
- [ ] Enable Certificate Transparency monitoring
- [ ] Configure proper firewall rules
- [ ] Set up monitoring and alerting
- [ ] Implement backup procedures for private keys
- [ ] Review and harden nginx configuration
- [ ] Enable security scanning and vulnerability assessments

## Scripts Reference

- **`create-certificates.sh`** - Generates self-signed CA and server certificates
- **`install-ca-certificate.sh`** - Installs CA certificate into system trust store
- **`start-https.sh`** - Starts the application with HTTPS support

## URLs and Endpoints

### Application URLs
- **Main Application:** https://rqmtmgmt.local
- **API Base:** https://rqmtmgmt.local/api
- **Swagger UI:** https://rqmtmgmt.local/swagger

### IdentityServer URLs
- **Discovery:** https://rqmtmgmt.local/.well-known/openid-configuration
- **Authorization:** https://rqmtmgmt.local/connect/authorize
- **Token:** https://rqmtmgmt.local/connect/token
- **UserInfo:** https://rqmtmgmt.local/connect/userinfo

All HTTP requests are automatically redirected to HTTPS.
