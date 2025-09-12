#!/bin/bash

# Deploy RqmtMgmt to Local Rancher Desktop Kubernetes
# This script deploys the application to local Kubernetes cluster

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
K8S_DIR="${PROJECT_ROOT}/k8s/local"

echo -e "${BLUE}🚀 Deploying RqmtMgmt to local Rancher Desktop Kubernetes...${NC}"

# Function to check if kubectl is available
check_kubectl() {
    if ! command -v kubectl &> /dev/null; then
        echo -e "${RED}✗ kubectl not found. Please install kubectl.${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ kubectl is available${NC}"
}

# Function to check if Kubernetes cluster is available
check_cluster() {
    if ! kubectl cluster-info &> /dev/null; then
        echo -e "${RED}✗ Kubernetes cluster not available. Make sure Rancher Desktop is running.${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ Kubernetes cluster is available${NC}"
}

# Function to generate TLS certificate for local development
generate_tls_cert() {
    local cert_dir="${PROJECT_ROOT}/temp-certs"
    mkdir -p "${cert_dir}"
    
    echo -e "${YELLOW}Generating self-signed certificate for rqmtmgmt.local...${NC}"
    
    # Generate certificate
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout "${cert_dir}/tls.key" \
        -out "${cert_dir}/tls.crt" \
        -subj "/CN=rqmtmgmt.local" \
        -addext "subjectAltName=DNS:rqmtmgmt.local,DNS:*.rqmtmgmt.local" 2>/dev/null
    
    # Base64 encode for Kubernetes secret
    local cert_b64=$(cat "${cert_dir}/tls.crt" | base64 -w 0)
    local key_b64=$(cat "${cert_dir}/tls.key" | base64 -w 0)
    
    # Update the ingress file with real certificates
    sed -i.bak "s|tls.crt: LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0t.*|tls.crt: ${cert_b64}|" "${K8S_DIR}/ingress-traefik.yaml"
    sed -i.bak "s|tls.key: LS0tLS1CRUdJTiBQUklWQVRFIEtFWS0tLS0t.*|tls.key: ${key_b64}|" "${K8S_DIR}/ingress-traefik.yaml"
    
    # Cleanup
    rm -rf "${cert_dir}"
    rm -f "${K8S_DIR}/ingress-traefik.yaml.bak"
    
    echo -e "${GREEN}✓ TLS certificate generated and configured${NC}"
}

# Function to check external database
check_database() {
    echo -e "${BLUE}Checking external SQL Server database...${NC}"
    
    # Check if SQL Server container is running
    if ! docker ps | grep -q "mcr.microsoft.com/mssql/server"; then
        echo -e "${YELLOW}SQL Server container not found. Starting external database...${NC}"
        docker run -d --name local-mssql \
            -e SA_PASSWORD=Your_password123 \
            -e ACCEPT_EULA=Y \
            -p 1433:1433 \
            mcr.microsoft.com/mssql/server:2022-latest
        
        echo -e "${YELLOW}Waiting for SQL Server to start...${NC}"
        sleep 30
    fi
    
    echo -e "${GREEN}✓ SQL Server database is available${NC}"
}

# Function to check /etc/hosts entry
check_hosts_file() {
    if ! grep -q "rqmtmgmt.local" /etc/hosts; then
        echo -e "${YELLOW}⚠️  rqmtmgmt.local not found in /etc/hosts${NC}"
        echo -e "${BLUE}Please add the following line to your /etc/hosts file:${NC}"
        echo -e "${GREEN}127.0.0.1    rqmtmgmt.local${NC}"
        echo -e "${BLUE}Run: sudo echo '127.0.0.1    rqmtmgmt.local' >> /etc/hosts${NC}"
        read -p "Press Enter after updating /etc/hosts..."
    else
        echo -e "${GREEN}✓ rqmtmgmt.local found in /etc/hosts${NC}"
    fi
}

# Function to apply Kubernetes manifests
apply_manifests() {
    echo -e "${BLUE}Applying Kubernetes manifests...${NC}"
    
    # Apply in order
    local manifests=(
        "namespace.yaml"
        "configmap.yaml"
        "secrets.yaml"
        "services.yaml"
        "backend-deployment.yaml"
        "frontend-deployment.yaml"
        "identity-deployment.yaml"
        "ingress-traefik.yaml"
    )
    
    for manifest in "${manifests[@]}"; do
        echo -e "${YELLOW}Applying ${manifest}...${NC}"
        kubectl apply -f "${K8S_DIR}/${manifest}"
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}✓ Applied ${manifest}${NC}"
        else
            echo -e "${RED}✗ Failed to apply ${manifest}${NC}"
            exit 1
        fi
    done
}

# Function to wait for deployments
wait_for_deployments() {
    echo -e "${BLUE}Waiting for deployments to be ready...${NC}"
    
    local deployments=("backend" "frontend" "identityserver")
    
    for deployment in "${deployments[@]}"; do
        echo -e "${YELLOW}Waiting for ${deployment} deployment...${NC}"
        kubectl wait --for=condition=available --timeout=300s deployment/${deployment} -n rqmtmgmt
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}✓ ${deployment} is ready${NC}"
        else
            echo -e "${RED}✗ ${deployment} failed to become ready${NC}"
            kubectl describe deployment/${deployment} -n rqmtmgmt
            exit 1
        fi
    done
}

# Function to show deployment status
show_status() {
    echo -e "${BLUE}📊 Deployment Status:${NC}"
    echo
    kubectl get all -n rqmtmgmt
    echo
    echo -e "${GREEN}🎉 Deployment completed successfully!${NC}"
    echo
    echo -e "${BLUE}Access the application:${NC}"
    echo -e "  • Frontend: ${GREEN}https://rqmtmgmt.local${NC}"
    echo -e "  • Backend API: ${GREEN}https://rqmtmgmt.local/api${NC}"
    echo -e "  • Swagger: ${GREEN}https://rqmtmgmt.local/swagger${NC}"
    echo -e "  • Health Check: ${GREEN}https://rqmtmgmt.local/health${NC}"
    echo
    echo -e "${YELLOW}Useful commands:${NC}"
    echo -e "  • View logs: ${BLUE}kubectl logs -f deployment/backend -n rqmtmgmt${NC}"
    echo -e "  • Scale deployment: ${BLUE}kubectl scale deployment/backend --replicas=2 -n rqmtmgmt${NC}"
    echo -e "  • Port forward: ${BLUE}kubectl port-forward service/backend-service 8080:80 -n rqmtmgmt${NC}"
    echo -e "  • Cleanup: ${BLUE}./scripts/cleanup-k8s.sh${NC}"
}

# Main execution
main() {
    check_kubectl
    check_cluster
    check_database
    check_hosts_file
    generate_tls_cert
    apply_manifests
    wait_for_deployments
    show_status
}

# Run main function
main "$@"