#!/bin/bash

# Comprehensive HTTPS testing script for internal Docker network
set -e

echo "🧪 Internal HTTPS Testing Suite"
echo "==============================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test functions
test_ssl_connection() {
    echo -e "${BLUE}🔒 Testing SSL Connection to nginx...${NC}"
    if echo | openssl s_client -connect nginx:443 -servername rqmtmgmt.local 2>/dev/null | grep -q "Verify return code: 0"; then
        echo -e "${GREEN}  ✅ SSL connection successful${NC}"
    else
        echo -e "${RED}  ❌ SSL connection failed${NC}"
    fi
    echo ""
}

test_certificate_details() {
    echo -e "${BLUE}🔍 Certificate Details:${NC}"
    echo | openssl s_client -connect nginx:443 -servername rqmtmgmt.local 2>/dev/null | openssl x509 -noout -subject -dates 2>/dev/null
    echo ""
}

test_health_endpoint() {
    echo -e "${BLUE}❤️ Testing /health endpoint...${NC}"
    
    # Test through nginx (should work)
    RESPONSE=$(curl -s -k -H "Host: rqmtmgmt.local" https://nginx/health 2>/dev/null || echo "FAILED")
    
    if [[ "$RESPONSE" == *"healthy"* ]]; then
        echo -e "${GREEN}  ✅ Health endpoint accessible through nginx${NC}"
        echo "  Response: $RESPONSE"
    else
        echo -e "${RED}  ❌ Health endpoint failed through nginx${NC}"
        echo "  Response: $RESPONSE"
    fi
    echo ""
}

test_api_endpoints() {
    echo -e "${BLUE}🚀 Testing API endpoints...${NC}"
    
    # Test swagger endpoint
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -k \
        -H "Host: rqmtmgmt.local" \
        https://nginx/swagger/ 2>/dev/null || echo "000")
    
    if [[ "$HTTP_CODE" == "200" ]]; then
        echo -e "${GREEN}  ✅ Swagger endpoint accessible (HTTP $HTTP_CODE)${NC}"
    else
        echo -e "${YELLOW}  ⚠️ Swagger returned HTTP $HTTP_CODE${NC}"
    fi
    
    # Test API root
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -k \
        -H "Host: rqmtmgmt.local" \
        https://nginx/api/ 2>/dev/null || echo "000")
    
    if [[ "$HTTP_CODE" == "404" ]]; then
        echo -e "${GREEN}  ✅ API root accessible (HTTP $HTTP_CODE - expected)${NC}"
    else
        echo -e "${YELLOW}  ⚠️ API root returned HTTP $HTTP_CODE${NC}"
    fi
    echo ""
}

test_frontend_access() {
    echo -e "${BLUE}🌐 Testing frontend access...${NC}"
    
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -k \
        -H "Host: rqmtmgmt.local" \
        https://nginx/ 2>/dev/null || echo "000")
    
    if [[ "$HTTP_CODE" == "200" ]]; then
        echo -e "${GREEN}  ✅ Frontend accessible (HTTP $HTTP_CODE)${NC}"
    else
        echo -e "${YELLOW}  ⚠️ Frontend returned HTTP $HTTP_CODE${NC}"
    fi
    echo ""
}

test_direct_backend() {
    echo -e "${BLUE}🎯 Testing direct backend access...${NC}"
    
    RESPONSE=$(curl -s http://backend:5000/health 2>/dev/null || echo "FAILED")
    
    if [[ "$RESPONSE" == *"healthy"* ]]; then
        echo -e "${GREEN}  ✅ Direct backend accessible${NC}"
        echo "  Response: $RESPONSE"
    else
        echo -e "${RED}  ❌ Direct backend failed${NC}"
        echo "  Response: $RESPONSE"
    fi
    echo ""
}

test_identity_server() {
    echo -e "${BLUE}🔐 Testing IdentityServer endpoints...${NC}"
    
    # Test .well-known/openid_configuration
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -k \
        -H "Host: rqmtmgmt.local" \
        https://nginx/.well-known/openid_configuration 2>/dev/null || echo "000")
    
    if [[ "$HTTP_CODE" == "200" ]]; then
        echo -e "${GREEN}  ✅ IdentityServer discovery accessible (HTTP $HTTP_CODE)${NC}"
    else
        echo -e "${YELLOW}  ⚠️ IdentityServer discovery returned HTTP $HTTP_CODE${NC}"
    fi
    echo ""
}

# Run all tests
echo -e "${YELLOW}Running comprehensive HTTPS tests within Docker network...${NC}"
echo ""

test_ssl_connection
test_certificate_details
test_health_endpoint
test_api_endpoints
test_frontend_access
test_direct_backend
test_identity_server

echo -e "${BLUE}📋 Test Summary${NC}"
echo "==============="
echo "All tests completed. Check the results above for any issues."
echo ""
echo -e "${GREEN}💡 This test runs entirely within the Docker network, bypassing any external proxy issues.${NC}"
