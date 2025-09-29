# Kubernetes Deployment with Trusted Certificates

This document explains how the Kubernetes deployment has been configured to use your existing trusted certificates from the `docker-compose/certs` directory instead of generating new self-signed certificates.

## What Changed

### 1. Updated Deployment Script
- Modified `scripts/deploy-local-k8s.sh` to use existing certificates
- Added `configure_tls_cert()` function to replace `generate_tls_cert()`
- The script now reads your trusted certificates from `docker-compose/certs/`

### 2. New TLS Secret Template
- Created `k8s/local/tls-secret.yaml` as a template for the TLS secret
- The deployment script populates this template with your actual certificates
- Generates `tls-secret-configured.yaml` during deployment (automatically cleaned up)

### 3. Certificate Files Used
The deployment now uses these files from your `docker-compose/certs/` directory:
- `server-cert.pem` - Your trusted certificate for rqmtmgmt.local
- `server-key.pem` - The corresponding private key

## How It Works

1. **Certificate Validation**: The script checks that both certificate files exist
2. **Base64 Encoding**: Certificates are base64-encoded for Kubernetes secrets
3. **Secret Creation**: A TLS secret named `rqmtmgmt-tls` is created in the `rqmtmgmt` namespace
4. **Traefik Integration**: The existing ingress configuration uses this secret for HTTPS

## Usage

Simply run the deployment script as before:

```bash
./scripts/deploy-local-k8s.sh
```

The script will:
- ✅ Verify your certificate files exist
- ✅ Configure TLS using your trusted certificates
- ✅ Deploy all services with HTTPS enabled
- ✅ Clean up temporary files automatically

## Benefits

- **No Certificate Warnings**: Your browser will trust the certificates since they're in your CA store
- **Consistent Experience**: Same certificates used in both Docker Compose and Kubernetes
- **Automatic Cleanup**: Temporary certificate files are removed after deployment
- **Error Handling**: Clear error messages if certificates are missing

## Troubleshooting

### Certificate Not Found
If you see an error like:
```
✗ Certificate file not found: /path/to/docker-compose/certs/server-cert.pem
```

Ensure your certificates are in the correct location:
```
docker-compose/certs/
├── server-cert.pem
└── server-key.pem
```

### Verify Certificate Content
You can verify your certificate is for the correct domain:
```bash
openssl x509 -in docker-compose/certs/server-cert.pem -text -noout | grep -A1 "Subject Alternative Name"
```

Should show `rqmtmgmt.local` in the SAN list.

### Check Kubernetes Secret
After deployment, verify the TLS secret was created:
```bash
kubectl get secret rqmtmgmt-tls -n rqmtmgmt
kubectl describe secret rqmtmgmt-tls -n rqmtmgmt
```

## Files Modified/Created

- ✏️ `scripts/deploy-local-k8s.sh` - Updated to use trusted certificates
- 🆕 `k8s/local/tls-secret.yaml` - TLS secret template
- 🆕 `scripts/deploy-local-k8s-with-trusted-certs.sh` - Backup of new script
- 🆕 `KUBERNETES_TRUSTED_CERTS.md` - This documentation

The ingress configuration (`k8s/local/ingress-traefik.yaml`) remains unchanged and continues to reference the `rqmtmgmt-tls` secret.