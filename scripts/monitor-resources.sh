#!/bin/bash

# Resource monitoring script for E2E testing
# This script monitors system resources during test execution

CONTAINER_PREFIX="docker-compose"
LOG_FILE="resource-usage.log"
INTERVAL=5

echo "🔍 Starting resource monitoring for E2E tests..."
echo "Logging to: $LOG_FILE"
echo "Monitor interval: ${INTERVAL}s"

# Create log file with headers
cat > "$LOG_FILE" << EOF
timestamp,container,cpu_percent,memory_usage_mb,memory_limit_mb,memory_percent
EOF

monitor_resources() {
    while true; do
        timestamp=$(date '+%Y-%m-%d %H:%M:%S')
        
        # Get Docker stats for all containers
        docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}" | grep -E "(frontend|backend|db-test|nginx)" | while read line; do
            if [[ "$line" != *"CONTAINER"* ]]; then
                container=$(echo "$line" | awk '{print $1}')
                cpu_percent=$(echo "$line" | awk '{print $2}' | sed 's/%//')
                memory_usage=$(echo "$line" | awk '{print $3}' | sed 's/MiB.*//')
                memory_limit=$(echo "$line" | awk '{print $4}' | sed 's/.*\///' | sed 's/GiB/000/' | sed 's/MiB//')
                memory_percent=$(echo "$line" | awk '{print $5}' | sed 's/%//')
                
                echo "$timestamp,$container,$cpu_percent,$memory_usage,$memory_limit,$memory_percent" >> "$LOG_FILE"
            fi
        done
        
        # Check for memory pressure
        available_mem=$(free -m | awk 'NR==2{printf "%.0f", $7}')
        if [ "$available_mem" -lt 1024 ]; then
            echo "⚠️  WARNING: Low system memory detected: ${available_mem}MB available"
        fi
        
        sleep $INTERVAL
    done
}

# Start monitoring in background
monitor_resources &
MONITOR_PID=$!

echo "📊 Resource monitoring started (PID: $MONITOR_PID)"
echo "To stop monitoring: kill $MONITOR_PID"
echo "To view live usage: tail -f $LOG_FILE"

# Wait for interrupt
trap "echo '🛑 Stopping resource monitoring...'; kill $MONITOR_PID; exit 0" INT TERM

wait $MONITOR_PID
