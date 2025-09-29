# Database Performance Investigation & Monitoring Specification

## Document Status
- **Created**: September 24, 2025
- **Version**: 1.0
- **Status**: Phase 1 - Foundation Implementation
- **Last Updated**: September 24, 2025

## Executive Summary

This specification provides a comprehensive framework for investigating database memory exhaustion and performance issues in the RqmtMgmt system, incorporating baseline performance metrics from load testing and establishing proactive monitoring capabilities.

**Background**: Following a critical database memory exhaustion incident that caused E2E test failures, this specification establishes systematic approaches for investigation, monitoring, and prevention of similar issues.

## Implementation Progress

### Phase 1: Foundation (Weeks 1-2) - IN PROGRESS

#### ✅ Completed Tasks
- [x] **Database Memory Configuration Fix** (Sept 24, 2025)
  - Increased SQL Server memory limit from 2GB to 4GB in docker-compose.yml
  - Resolved immediate memory exhaustion causing E2E test failures
  - Verified container restart and health status
  - **Result**: E2E tests now passing (5/5 authenticated workflow tests successful)

#### 🔄 In Progress Tasks
- [ ] **Load Test Baseline Capture**
  - **Status**: Ready to execute
  - **Command**: `cd /home/wtruitt/src/repos/RqmtMgmt && dotnet test backend.LoadTests --logger "console;verbosity=detailed"`
  - **Goal**: Establish performance baselines for monitoring thresholds
  - **Expected Metrics**: Response times, throughput, resource utilization

#### 📋 Pending Tasks
- [ ] **Container Resource Monitoring Setup**
  - Implement Docker stats collection script
  - Create monitoring log files
  - Establish baseline resource usage patterns

- [ ] **Application Logging Enhancement**
  - Add performance counters to backend API
  - Implement structured logging for performance metrics
  - Configure log aggregation for analysis

- [ ] **Incident Documentation**
  - Document recent memory exhaustion incident as case study
  - Create post-mortem analysis
  - Extract lessons learned for future prevention

## Current System State

### Database Configuration
```yaml
# docker-compose.yml - Database Service
db:
  image: mcr.microsoft.com/mssql/server:2022-latest
  environment:
    SA_PASSWORD: "Your_password123"
    ACCEPT_EULA: "Y"
    MSSQL_MEMORY_LIMIT_MB: "4096"  # ← Updated from 2048MB
  deploy:
    resources:
      limits:
        cpus: '2.0'
        memory: 8G
      reservations:
        memory: 2G
```

### Container Status (as of Sept 24, 2025)
```bash
# Current container health
CONTAINER ID   IMAGE                                        STATUS
ed38b168e3ea   mcr.microsoft.com/mssql/server:2022-latest   Up 36 seconds (healthy)
```

## Key Performance Indicators (KPIs) Framework

### Database Layer Targets
| Metric | Target | Warning | Critical |
|--------|--------|---------|----------|
| Memory Usage | < 3GB | > 3.5GB | > 3.8GB |
| CPU Utilization | < 70% | > 85% | > 95% |
| Query Response Time | < 100ms | > 500ms | > 1000ms |
| Connection Pool Usage | < 80% | > 90% | > 95% |
| Lock Wait Time | < 10ms | > 50ms | > 100ms |

### Application Layer Targets
| Metric | Target | Warning | Critical |
|--------|--------|---------|----------|
| API Response Time | < 200ms | > 1000ms | > 2000ms |
| Throughput | > 100 req/sec | < 50 req/sec | < 25 req/sec |
| Error Rate | < 1% | > 5% | > 10% |
| GC Pressure | < 10 MB/sec | > 50 MB/sec | > 100 MB/sec |

## Emergency Response Procedures

### Immediate Actions for Memory Exhaustion
1. **Check Container Status**
   ```bash
   docker ps -a | grep db
   docker stats docker-compose-db-1 --no-stream
   ```

2. **Analyze SQL Server Logs**
   ```bash
   docker logs docker-compose-db-1 --tail 50
   ```

3. **Quick Memory Analysis**
   ```sql
   -- Connect to SQL Server and run
   SELECT 
       type AS memory_type,
       SUM(pages_kb)/1024 AS memory_usage_mb
   FROM sys.dm_os_memory_clerks
   GROUP BY type
   ORDER BY SUM(pages_kb) DESC;
   ```

4. **Apply Immediate Mitigation**
   - Restart database container if necessary
   - Increase memory limits if safe to do so
   - Clear connection pools if connection exhaustion detected

## Diagnostic Queries

### Memory Usage Analysis
```sql
-- Current Memory Usage by Component
SELECT 
    type AS memory_type,
    SUM(pages_kb) AS memory_usage_kb,
    SUM(pages_kb)/1024 AS memory_usage_mb
FROM sys.dm_os_memory_clerks
GROUP BY type
ORDER BY SUM(pages_kb) DESC;

-- Memory Pressure Indicators
SELECT 
    memory_state_desc,
    last_notification,
    memory_utilization_percentage
FROM sys.dm_os_memory_clerks
WHERE type = 'MEMORYCLERK_SQLBUFFERPOOL';
```

### Performance Bottleneck Detection
```sql
-- Top Resource-Consuming Queries
SELECT TOP 10
    qs.execution_count,
    qs.total_worker_time / qs.execution_count AS avg_cpu_time,
    qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time,
    qs.total_logical_reads / qs.execution_count AS avg_logical_reads,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2)+1) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
ORDER BY qs.total_worker_time DESC;
```

## Incident History

### September 24, 2025 - Memory Exhaustion Incident
- **Issue**: SQL Server container experiencing memory exhaustion
- **Symptoms**: E2E tests failing with authentication redirects, database connection errors
- **Root Cause**: MSSQL_MEMORY_LIMIT_MB set to 2048MB insufficient for workload
- **Resolution**: Increased memory limit to 4096MB
- **Impact**: 46 failed E2E tests → All critical tests now passing
- **Prevention**: Implement proactive memory monitoring (this specification)

## Next Steps

### Phase 1 Completion Tasks
1. **Execute Load Test Baseline** (Priority: High)
   - Run comprehensive backend.LoadTests
   - Document performance baselines
   - Establish monitoring thresholds

2. **Implement Basic Monitoring** (Priority: High)
   - Deploy container stats collection
   - Create performance logging
   - Set up basic alerting

3. **Document Lessons Learned** (Priority: Medium)
   - Complete incident post-mortem
   - Update troubleshooting guides
   - Share knowledge with team

### Phase 2 Preview: Monitoring Infrastructure
- Prometheus/Grafana deployment
- Advanced alerting configuration
- Dashboard creation
- Integration with existing systems

---

## Appendix

### Useful Commands
```bash
# Check database container health
docker ps -a | grep db

# Monitor container resources
docker stats docker-compose-db-1 --no-stream

# View recent database logs
docker logs docker-compose-db-1 --tail 20

# Restart database with new configuration
cd /home/wtruitt/src/repos/RqmtMgmt/docker-compose
docker-compose up -d db

# Run E2E tests to verify functionality
cd /home/wtruitt/src/repos/RqmtMgmt
dotnet test frontend.E2ETests --filter "AuthenticatedWorkflow"
```

### Contact Information
- **Document Owner**: System Operations Team
- **Technical Lead**: [To be assigned]
- **Review Schedule**: Weekly during Phase 1, Monthly thereafter

---

*This document is part of the RqmtMgmt system operational documentation. For updates or questions, please follow the established change management process.*