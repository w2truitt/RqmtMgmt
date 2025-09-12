#!/bin/bash

# Install Traefik CRDs for Rancher Desktop
# This script installs the necessary Traefik Custom Resource Definitions

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔧 Installing Traefik CRDs for Rancher Desktop...${NC}"

# Function to install Traefik CRDs
install_traefik_crds() {
    echo -e "${YELLOW}Installing Traefik CRDs...${NC}"
    
    # Install Traefik CRDs from official repository
    kubectl apply -f https://raw.githubusercontent.com/traefik/traefik/v2.10/docs/content/reference/dynamic-configuration/kubernetes-crd-definition-v1.yml
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Traefik CRDs installed successfully${NC}"
    else
        echo -e "${RED}✗ Failed to install Traefik CRDs${NC}"
        return 1
    fi
}

# Function to verify CRDs are installed
verify_crds() {
    echo -e "${YELLOW}Verifying Traefik CRDs...${NC}"
    
    # Check for Middleware CRD
    if kubectl get crd middlewares.traefik.containo.us > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Middleware CRD is available${NC}"
    else
        echo -e "${RED}✗ Middleware CRD not found${NC}"
        return 1
    fi
    
    # Check for IngressRoute CRD
    if kubectl get crd ingressroutes.traefik.containo.us > /dev/null 2>&1; then
        echo -e "${GREEN}✓ IngressRoute CRD is available${NC}"
    else
        echo -e "${YELLOW}⚠️  IngressRoute CRD not found (optional)${NC}"
    fi
}

# Main execution
main() {
    # Check if kubectl is available
    if ! command -v kubectl &> /dev/null; then
        echo -e "${RED}✗ kubectl not found${NC}"
        exit 1
    fi
    
    # Check if cluster is available
    if ! kubectl cluster-info &> /dev/null; then
        echo -e "${RED}✗ Kubernetes cluster not available${NC}"
        exit 1
    fi
    
    # Install CRDs
    if install_traefik_crds && verify_crds; then
        echo -e "${GREEN}🎉 Traefik CRDs installation completed!${NC}"
        echo -e "${BLUE}You can now run the deployment script:${NC}"
        echo -e "  ${GREEN}./scripts/deploy-local-k8s.sh${NC}"
    else
        echo -e "${RED}✗ CRD installation failed${NC}"
        exit 1
    fi
}

# Run main function
main "$@"