#!/bin/bash

# Setup Local Docker Registry for Rancher Desktop
# This script starts a local Docker registry that works with Rancher Desktop

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

REGISTRY_NAME="local-registry"
REGISTRY_PORT="5000"

echo -e "${BLUE}🐳 Setting up local Docker registry for Rancher Desktop...${NC}"

# Function to check if registry is running
check_registry() {
    if docker ps | grep -q "${REGISTRY_NAME}"; then
        echo -e "${GREEN}✓ Registry is already running${NC}"
        return 0
    else
        return 1
    fi
}

# Function to start registry
start_registry() {
    echo -e "${YELLOW}Starting local Docker registry...${NC}"
    
    # Remove existing registry container if it exists
    docker rm -f ${REGISTRY_NAME} 2>/dev/null || true
    
    # Start new registry
    docker run -d \
        --name ${REGISTRY_NAME} \
        --restart=always \
        -p ${REGISTRY_PORT}:5000 \
        -v registry-data:/var/lib/registry \
        registry:2
    
    # Wait for registry to start
    sleep 5
    
    # Test registry
    if curl -f http://localhost:${REGISTRY_PORT}/v2/ > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Registry started successfully${NC}"
        return 0
    else
        echo -e "${RED}✗ Registry failed to start${NC}"
        return 1
    fi
}

# Function to configure Rancher Desktop
configure_rancher_desktop() {
    echo -e "${YELLOW}Configuring Rancher Desktop for local registry...${NC}"
    
    # Create config directory if it doesn't exist
    local config_dir
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        config_dir="$HOME/.config/rancher-desktop"
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        config_dir="$HOME/Library/Application Support/rancher-desktop"
    else
        echo -e "${YELLOW}⚠️  Manual configuration may be needed for Windows${NC}"
        config_dir="$HOME/.config/rancher-desktop"
    fi
    
    mkdir -p "${config_dir}"
    
    # Create or update registries configuration
    cat > "${config_dir}/registries.yaml" << EOF
mirrors:
  "localhost:${REGISTRY_PORT}":
    endpoint:
      - "http://localhost:${REGISTRY_PORT}"
configs:
  "localhost:${REGISTRY_PORT}":
    insecure: true
EOF
    
    echo -e "${GREEN}✓ Created registries configuration${NC}"
    echo -e "${YELLOW}⚠️  You may need to restart Rancher Desktop for changes to take effect${NC}"
}

# Function to test registry
test_registry() {
    echo -e "${BLUE}Testing registry functionality...${NC}"
    
    # Test basic connectivity
    if curl -f http://localhost:${REGISTRY_PORT}/v2/ > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Registry API is accessible${NC}"
    else
        echo -e "${RED}✗ Registry API is not accessible${NC}"
        return 1
    fi
    
    # Test with a simple image
    echo -e "${YELLOW}Testing image push/pull...${NC}"
    
    # Pull a small test image
    docker pull hello-world:latest
    
    # Tag for local registry
    docker tag hello-world:latest localhost:${REGISTRY_PORT}/hello-world:test
    
    # Push to local registry
    if docker push localhost:${REGISTRY_PORT}/hello-world:test; then
        echo -e "${GREEN}✓ Successfully pushed test image to registry${NC}"
        
        # Clean up test image
        docker rmi localhost:${REGISTRY_PORT}/hello-world:test
        
        # Try to pull it back
        if docker pull localhost:${REGISTRY_PORT}/hello-world:test; then
            echo -e "${GREEN}✓ Successfully pulled test image from registry${NC}"
            docker rmi localhost:${REGISTRY_PORT}/hello-world:test
            return 0
        else
            echo -e "${RED}✗ Failed to pull test image from registry${NC}"
            return 1
        fi
    else
        echo -e "${RED}✗ Failed to push test image to registry${NC}"
        return 1
    fi
}

# Function to show status
show_status() {
    echo -e "${BLUE}📊 Registry Status:${NC}"
    echo -e "  Registry URL: ${GREEN}localhost:${REGISTRY_PORT}${NC}"
    echo -e "  Container Name: ${GREEN}${REGISTRY_NAME}${NC}"
    echo -e "  Data Volume: ${GREEN}registry-data${NC}"
    echo
    echo -e "${BLUE}Registry Contents:${NC}"
    curl -s http://localhost:${REGISTRY_PORT}/v2/_catalog | jq . 2>/dev/null || curl -s http://localhost:${REGISTRY_PORT}/v2/_catalog
    echo
    echo -e "${YELLOW}Usage:${NC}"
    echo -e "  • Tag images: ${BLUE}docker tag myimage localhost:${REGISTRY_PORT}/myimage${NC}"
    echo -e "  • Push images: ${BLUE}docker push localhost:${REGISTRY_PORT}/myimage${NC}"
    echo -e "  • Pull images: ${BLUE}docker pull localhost:${REGISTRY_PORT}/myimage${NC}"
    echo
    echo -e "${YELLOW}Next steps:${NC}"
    echo -e "  1. Build images: ${BLUE}./scripts/build-k8s-images.sh${NC}"
    echo -e "  2. Deploy to Kubernetes: ${BLUE}./scripts/deploy-local-k8s.sh${NC}"
}

# Main execution
main() {
    if check_registry; then
        echo -e "${BLUE}Registry is already running. Testing functionality...${NC}"
        if test_registry; then
            show_status
            exit 0
        else
            echo -e "${YELLOW}Registry test failed. Restarting registry...${NC}"
        fi
    fi
    
    if start_registry; then
        configure_rancher_desktop
        if test_registry; then
            echo -e "${GREEN}🎉 Local registry setup completed successfully!${NC}"
            show_status
        else
            echo -e "${RED}✗ Registry setup completed but tests failed${NC}"
            exit 1
        fi
    else
        echo -e "${RED}✗ Failed to start registry${NC}"
        exit 1
    fi
}

# Run main function
main "$@"