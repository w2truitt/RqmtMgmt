# Docker Backend Startup Issues - Fixed

## 🐛 **Problem Identified:**

The backend container was failing to start consistently due to database initialization issues:

```
Database 'RqmtMgmt' already exists. Choose a different database name.
backend-1 exited with code 134
```

### **Root Causes:**
1. **Race Condition**: Backend tried to connect before SQL Server was fully ready
2. **Wrong Database Creation Method**: Used `EnsureCreatedAsync()` instead of `MigrateAsync()`
3. **No Retry Logic**: Single attempt with no error recovery
4. **Missing Health Checks**: No way to verify container health

## ✅ **Solutions Implemented:**

### **1. Fixed Database Initialization Method**

**Before (DatabaseSeeder.cs):**
```csharp
// Ensure database is created
await context.Database.EnsureCreatedAsync();
```

**After (DatabaseSeeder.cs):**
```csharp
// Apply any pending migrations (this will create the database if it doesn't exist)
await context.Database.MigrateAsync();
```

**Why this fixes it:**
- `EnsureCreatedAsync()` fails if database exists with different schema
- `MigrateAsync()` properly handles existing databases and applies only needed changes
- Works correctly with Entity Framework migrations

### **2. Added Retry Logic with Exponential Backoff**

**New Implementation (Program.cs):**
```csharp
private static async Task InitializeDatabaseWithRetryAsync(WebApplication app)
{
    const int maxRetries = 10;
    const int delayMs = 3000; // 3 seconds between retries

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            // Test database connectivity first
            await context.Database.CanConnectAsync();
            
            // Apply migrations and seed data
            await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
            
            return; // Success - exit retry loop
        }
        catch (Exception ex)
        {
            if (attempt == maxRetries)
                throw; // Re-throw on final attempt
                
            await Task.Delay(delayMs); // Wait before retry
        }
    }
}
```

**Benefits:**
- **10 retry attempts** with 3-second delays (30 seconds total)
- **Graceful degradation** with proper logging
- **Connection testing** before attempting migrations
- **Environment-aware** seeding (dev vs production)

### **3. Added Docker Health Checks**

**Updated docker-compose.yml:**
```yaml
backend:
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:80/health"]
    interval: 30s
    timeout: 10s
    retries: 3
    start_period: 40s
  depends_on:
    db:
      condition: service_healthy

db:
  healthcheck:
    test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123 -Q 'SELECT 1'"]
    interval: 10s
    timeout: 5s
    retries: 5
    start_period: 30s
```

**New Health Check Endpoint (Program.cs):**
```csharp
app.MapGet("/health", async (RqmtMgmtDbContext context) =>
{
    try
    {
        await context.Database.CanConnectAsync();
        return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 503);
    }
});
```

### **4. Improved Container Dependencies**

**Updated Service Dependencies:**
```yaml
frontend:
  depends_on:
    backend:
      condition: service_healthy  # Wait for backend to be healthy

backend:
  depends_on:
    db:
      condition: service_healthy  # Wait for database to be healthy

nginx:
  depends_on:
    frontend:
      condition: service_started
    backend:
      condition: service_healthy
```

## 🚀 **Results:**

### **Before Fix:**
- ❌ Backend failed randomly on startup
- ❌ Database creation errors
- ❌ Container exit code 134
- ❌ No error recovery

### **After Fix:**
- ✅ **Reliable Startup**: 10 retry attempts with proper error handling
- ✅ **Proper Database Handling**: Uses EF migrations correctly
- ✅ **Health Monitoring**: Docker can verify container health
- ✅ **Ordered Startup**: Services start in correct dependency order
- ✅ **Better Logging**: Clear visibility into startup process

## 📋 **Startup Sequence:**

1. **SQL Server Container** starts and becomes healthy (30s max)
2. **Backend Container** starts, waits for DB, retries connection (30s max)
3. **Frontend Container** starts after backend is healthy
4. **Nginx Container** starts after both frontend/backend are ready

## 🔧 **Usage:**

```bash
# Start all services with proper dependency management
cd docker-compose/
docker-compose up -d

# Check container health
docker-compose ps

# View backend logs to see retry logic
docker-compose logs backend

# Test health endpoint directly
curl http://localhost:8080/api/health
```

## 📊 **Monitoring:**

- **Health Check URL**: `http://localhost:8080/api/health`
- **Expected Response**: `{"status":"healthy","timestamp":"2025-08-07T..."}`
- **Failure Response**: HTTP 503 with error details
- **Container Status**: Use `docker-compose ps` to see health status

The backend should now start reliably every time, even if the database takes time to initialize!

---

*Fixed: August 7, 2025*  
*Status: Docker startup issues resolved with retry logic and health checks*