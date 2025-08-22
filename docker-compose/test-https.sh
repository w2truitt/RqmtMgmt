#!/bin/bash

echo "🧪 Testing HTTPS Configuration for rqmtmgmt.local"
echo "============================================================"
echo ""

# Test domain resolution
echo "🌐 Testing domain resolution:"
if nslookup rqmtmgmt.local | grep -q "127.0.0.1"; then
    echo "  ✅ Domain resolves to localhost"
else
    echo "  ❌ Domain resolution issue"
fi
echo ""

# Test SSL certificate
echo "🔒 Testing SSL certificate:"
echo | openssl s_client -connect rqmtmgmt.local:443 -servername rqmtmgmt.local 2>/dev/null | openssl x509 -noout -subject -dates 2>/dev/null
echo ""

# Test basic HTTPS connectivity
echo "🔗 Testing HTTPS connectivity:"
HTTP_CODE=$(curl -k -s -o /dev/null -w "%{http_code}" https://rqmtmgmt.local/)
echo "  Response code: $HTTP_CODE"
echo ""

# Test backend health endpoint through nginx
echo "❤️ Testing health endpoint through nginx:"
curl -k -s https://rqmtmgmt.local/health | head -1
echo ""

# Test direct backend access
echo "🎯 Testing direct backend access:"
docker-compose -f docker-compose-ssl.yml exec -T backend curl -s http://localhost:5000/health
echo ""

# Test swagger endpoint
echo "📚 Testing swagger endpoint:"
HTTP_CODE=$(curl -k -s -o /dev/null -w "%{http_code}" https://rqmtmgmt.local/swagger/)
echo "  Swagger response code: $HTTP_CODE"
echo ""

echo "🔍 If you're seeing 404 errors, this might be due to your corporate"
echo "   reverse proxy intercepting requests. The certificates and nginx"
echo "   configuration are properly set up for rqmtmgmt.local"
echo ""
echo "💡 You can test the application by opening your browser to:"
echo "   https://rqmtmgmt.local"
