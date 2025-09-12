#!/bin/bash

# Start Database for Kubernetes Development
# This script starts the SQL Server database using the existing docker-compose setup

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo -e "${BLUE}🗄️  Starting SQL Server database for Kubernetes development...${NC}"

# Function to start database using docker-compose
start_with_compose() {
    local compose_dir="${PROJECT_ROOT}/docker-compose"
    
    if [ -f "${compose_dir}/docker-compose.yml" ]; then
        echo -e "${YELLOW}Using existing docker-compose setup...${NC}"
        cd "${compose_dir}"
        docker-compose up -d db
        cd "${PROJECT_ROOT}"
        
        echo -e "${YELLOW}Waiting for SQL Server to be ready...${NC}"
        sleep 20
        
        # Test connection
        for i in {1..10}; do
            if docker exec docker-compose-db-1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Your_password123" -Q "SELECT 1" > /dev/null 2>&1; then
                echo -e "${GREEN}✓ SQL Server is ready${NC}"
                return 0
            fi
            echo -e "${YELLOW}Waiting for SQL Server... (attempt $i/10)${NC}"
            sleep 10
        done
        
        echo -e "${RED}✗ SQL Server failed to start properly${NC}"
        return 1
    else
        echo -e "${RED}✗ docker-compose.yml not found in ${compose_dir}${NC}"
        return 1
    fi
}

# Function to start standalone database (fallback)
start_standalone() {
    echo -e "${YELLOW}Starting standalone SQL Server container...${NC}"
    
    # Remove existing container if it exists
    docker rm -f local-mssql 2>/dev/null || true
    
    # Start new container
    docker run -d --name local-mssql \
        -e SA_PASSWORD=Your_password123 \
        -e ACCEPT_EULA=Y \
        -p 1433:1433 \
        -v sqlvolume:/var/opt/mssql \
        mcr.microsoft.com/mssql/server:2022-latest
    
    echo -e "${YELLOW}Waiting for SQL Server to be ready...${NC}"
    sleep 30
    
    # Test connection
    for i in {1..10}; do
        if docker exec local-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Your_password123" -Q "SELECT 1" > /dev/null 2>&1; then
            echo -e "${GREEN}✓ SQL Server is ready${NC}"
            return 0
        fi
        echo -e "${YELLOW}Waiting for SQL Server... (attempt $i/10)${NC}"
        sleep 10
    done
    
    echo -e "${RED}✗ SQL Server failed to start properly${NC}"
    return 1
}

# Function to check if database is already running
check_existing() {
    # Check for docker-compose database
    if docker ps --format "table {{.Names}}" | grep -q "docker-compose.*db"; then
        echo -e "${GREEN}✓ Docker Compose database is already running${NC}"
        return 0
    fi
    
    # Check for standalone database
    if docker ps --format "table {{.Names}}" | grep -q "local-mssql"; then
        echo -e "${GREEN}✓ Standalone database is already running${NC}"
        return 0
    fi
    
    return 1
}

# Main execution
main() {
    if check_existing; then
        echo -e "${BLUE}Database is already running. No action needed.${NC}"
        exit 0
    fi
    
    echo -e "${BLUE}No database found. Starting database...${NC}"
    
    # Try docker-compose first, fallback to standalone
    if start_with_compose; then
        echo -e "${GREEN}🎉 Database started successfully with docker-compose${NC}"
    elif start_standalone; then
        echo -e "${GREEN}🎉 Database started successfully as standalone container${NC}"
    else
        echo -e "${RED}✗ Failed to start database${NC}"
        exit 1
    fi
    
    echo -e "${BLUE}Database Connection Details:${NC}"
    echo -e "  Server: ${GREEN}localhost,1433${NC}"
    echo -e "  Database: ${GREEN}RqmtMgmt${NC}"
    echo -e "  User: ${GREEN}sa${NC}"
    echo -e "  Password: ${GREEN}Your_password123${NC}"
    echo
    echo -e "${YELLOW}Next steps:${NC}"
    echo -e "  1. Run database migrations: ${BLUE}cd backend && dotnet ef database update${NC}"
    echo -e "  2. Build Kubernetes images: ${BLUE}./scripts/build-k8s-images.sh${NC}"
    echo -e "  3. Deploy to Kubernetes: ${BLUE}./scripts/deploy-local-k8s.sh${NC}"
}

# Run main function
main "$@"