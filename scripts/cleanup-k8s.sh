#!/bin/bash

# Cleanup Kubernetes deployment for RqmtMgmt
# This script removes all Kubernetes resources for the application

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
K8S_DIR="${PROJECT_ROOT}/k8s/local"

echo -e "${BLUE}🧹 Cleaning up RqmtMgmt Kubernetes deployment...${NC}"

# Function to check if kubectl is available
check_kubectl() {
    if ! command -v kubectl &> /dev/null; then
        echo -e "${RED}✗ kubectl not found. Please install kubectl.${NC}"
        exit 1
    fi
}

# Function to delete Kubernetes resources
cleanup_k8s() {
    echo -e "${YELLOW}Deleting Kubernetes resources...${NC}"
    
    # Delete in reverse order
    local manifests=(
        "ingress-traefik.yaml"
        "identity-deployment.yaml"
        "frontend-deployment.yaml"
        "backend-deployment.yaml"
        "services.yaml"
        "secrets.yaml"
        "configmap.yaml"
    )
    
    for manifest in "${manifests[@]}"; do
        if [ -f "${K8S_DIR}/${manifest}" ]; then
            echo -e "${YELLOW}Deleting resources from ${manifest}...${NC}"
            kubectl delete -f "${K8S_DIR}/${manifest}" --ignore-not-found=true
            if [ $? -eq 0 ]; then
                echo -e "${GREEN}✓ Deleted resources from ${manifest}${NC}"
            else
                echo -e "${YELLOW}⚠️  Some resources from ${manifest} may not have been deleted${NC}"
            fi
        fi
    done
    
    # Delete namespace (this will cleanup any remaining resources)
    echo -e "${YELLOW}Deleting namespace...${NC}"
    kubectl delete namespace rqmtmgmt --ignore-not-found=true
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Deleted namespace rqmtmgmt${NC}"
    fi
}

# Function to cleanup Docker images (optional)
cleanup_images() {
    read -p "Do you want to remove local Docker images? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${YELLOW}Removing local Docker images...${NC}"
        
        local images=(
            "localhost:5000/rqmtmgmt-backend:latest"
            "localhost:5000/rqmtmgmt-frontend:latest"
            "localhost:5000/rqmtmgmt-identity:latest"
        )
        
        for image in "${images[@]}"; do
            if docker image inspect "${image}" > /dev/null 2>&1; then
                docker rmi "${image}"
                echo -e "${GREEN}✓ Removed ${image}${NC}"
            else
                echo -e "${YELLOW}⚠️  Image ${image} not found${NC}"
            fi
        done
    fi
}

# Function to cleanup external database (optional)
cleanup_database() {
    read -p "Do you want to stop and remove the external SQL Server container? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${YELLOW}Stopping and removing SQL Server container...${NC}"
        
        if docker ps -a | grep -q "local-mssql"; then
            docker stop local-mssql 2>/dev/null || true
            docker rm local-mssql 2>/dev/null || true
            echo -e "${GREEN}✓ Removed SQL Server container${NC}"
        else
            echo -e "${YELLOW}⚠️  SQL Server container 'local-mssql' not found${NC}"
        fi
    fi
}

# Function to show final status
show_final_status() {
    echo -e "${BLUE}📊 Cleanup Status:${NC}"
    echo
    
    # Check if namespace still exists
    if kubectl get namespace rqmtmgmt > /dev/null 2>&1; then
        echo -e "${YELLOW}⚠️  Namespace 'rqmtmgmt' still exists (may be terminating)${NC}"
        kubectl get all -n rqmtmgmt 2>/dev/null || true
    else
        echo -e "${GREEN}✓ Namespace 'rqmtmgmt' has been removed${NC}"
    fi
    
    echo
    echo -e "${GREEN}🎉 Cleanup completed!${NC}"
    echo
    echo -e "${BLUE}To redeploy:${NC}"
    echo -e "  1. ${BLUE}./scripts/build-k8s-images.sh${NC}"
    echo -e "  2. ${BLUE}./scripts/deploy-local-k8s.sh${NC}"
}

# Main execution
main() {
    check_kubectl
    cleanup_k8s
    cleanup_images
    cleanup_database
    show_final_status
}

# Run main function
main "$@"