#!/bin/bash

# API functionality testing script
set -e

echo "🧪 API Functionality Test Suite"
echo "==============================="
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

BASE_URL="https://nginx"

# Common curl options
CURL_OPTS=(-s -k -H "Host: rqmtmgmt.local")

test_api_endpoint() {
    local endpoint="$1"
    local description="$2"
    local expected_code="${3:-200}"
    
    echo -e "${BLUE}Testing $description...${NC}"
    
    HTTP_CODE=$(curl "${CURL_OPTS[@]}" -o /dev/null -w "%{http_code}" "$BASE_URL$endpoint" 2>/dev/null || echo "000")
    
    if [[ "$HTTP_CODE" == "$expected_code" ]]; then
        echo -e "${GREEN}  ✅ $description (HTTP $HTTP_CODE)${NC}"
    else
        echo -e "${RED}  ❌ $description failed (HTTP $HTTP_CODE, expected $expected_code)${NC}"
    fi
    echo ""
}

test_api_with_response() {
    local endpoint="$1"
    local description="$2"
    
    echo -e "${BLUE}Testing $description...${NC}"
    
    RESPONSE=$(curl "${CURL_OPTS[@]}" "$BASE_URL$endpoint" 2>/dev/null || echo "FAILED")
    HTTP_CODE=$(curl "${CURL_OPTS[@]}" -o /dev/null -w "%{http_code}" "$BASE_URL$endpoint" 2>/dev/null || echo "000")
    
    echo "  HTTP Code: $HTTP_CODE"
    echo "  Response: ${RESPONSE:0:100}..."
    echo ""
}

echo "🔍 Testing Core API Endpoints"
echo "-----------------------------"

# Test health endpoint
test_api_with_response "/health" "Health Check"

# Test swagger endpoints
test_api_endpoint "/swagger/" "Swagger UI" "200"
test_api_endpoint "/swagger/v1/swagger.json" "Swagger JSON" "200"

# Test IdentityServer endpoints
test_api_endpoint "/.well-known/openid_configuration" "OIDC Discovery" "200"

# Test API endpoints (these may require authentication, so 401 is expected)
test_api_endpoint "/api/Dashboard/statistics" "Dashboard Statistics" "401"
test_api_endpoint "/api/Requirements" "Requirements List" "401"
test_api_endpoint "/api/TestCases" "Test Cases List" "401"

echo "🌐 Testing Frontend"
echo "-------------------"
test_api_endpoint "/" "Frontend Root" "200"

echo "✅ API functionality tests completed!"
echo ""
echo "Note: 401 responses are expected for protected API endpoints since we're not authenticated."
