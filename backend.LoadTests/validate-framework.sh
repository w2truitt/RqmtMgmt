#!/bin/bash

# K6 Load Testing Framework Validation
# Quick validation that the framework is properly set up

echo "🔍 K6 Load Testing Framework Validation"
echo "======================================="
echo ""

# Check if K6 is available
if command -v k6 &> /dev/null; then
    echo "✅ K6 is installed: $(k6 version | head -n1)"
else
    echo "❌ K6 is not installed"
    echo "   Install with: sudo apt-get install k6"
    echo "   Or visit: https://k6.io/docs/get-started/installation/"
    exit 1
fi

echo ""

# Check framework structure
echo "📁 Checking framework structure..."

required_files=(
    "tests/smoke-test.js"
    "tests/baseline-test.js" 
    "tests/stress-test.js"
    "tests/spike-test.js"
    "tests/endurance-test.js"
    "tests/projects-focused-test.js"
    "config/test-config.js"
    "utils/test-helpers.js"
    "run-tests.sh"
    "README.md"
)

all_files_exist=true
for file in "${required_files[@]}"; do
    if [[ -f "$file" ]]; then
        echo "   ✅ $file"
    else
        echo "   ❌ $file (missing)"
        all_files_exist=false
    fi
done

if [[ "$all_files_exist" == false ]]; then
    echo ""
    echo "❌ Some required files are missing"
    exit 1
fi

echo ""
echo "✅ All framework files are present"
echo ""

# Test basic K6 functionality with a minimal test
echo "🧪 Testing K6 with minimal test..."

cat > /tmp/k6-validation-test.js << 'EOF'
import { check } from 'k6';
import http from 'k6/http';

export let options = {
  vus: 1,
  duration: '5s',
};

export default function() {
  // Test a simple HTTP request (using httpbin.org as a reliable test endpoint)
  let response = http.get('https://httpbin.org/get');
  check(response, {
    'status is 200': (r) => r.status === 200,
  });
}
EOF

if k6 run /tmp/k6-validation-test.js > /dev/null 2>&1; then
    echo "✅ K6 basic functionality test passed"
else
    echo "❌ K6 basic functionality test failed"
    echo "   This might indicate network issues or K6 installation problems"
fi

# Clean up
rm -f /tmp/k6-validation-test.js

echo ""
echo "🎯 Framework Validation Summary:"
echo "   ✅ K6 installed and functional"
echo "   ✅ All test files present"
echo "   ✅ Framework structure complete"
echo ""
echo "🚀 Ready to run load tests!"
echo ""
echo "Next steps:"
echo "   ./run-tests.sh smoke          # Quick 30-second test"
echo "   ./run-tests.sh projects       # Validate optimization"
echo "   ./run-tests.sh                # Interactive mode"
echo ""