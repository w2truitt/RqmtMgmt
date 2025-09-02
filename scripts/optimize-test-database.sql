-- SQL Server configuration for testing environment
-- This script optimizes SQL Server settings for test workloads

-- Reduce memory pressure by limiting buffer pool size
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

-- Set max server memory to 80% of allocated container memory (1GB * 0.8 = 819MB)
EXEC sp_configure 'max server memory (MB)', 819;
RECONFIGURE;

-- Optimize for faster test execution
-- Reduce recovery interval for faster checkpoints
EXEC sp_configure 'recovery interval (min)', 1;
RECONFIGURE;

-- Reduce cost threshold for parallelism to improve small query performance
EXEC sp_configure 'cost threshold for parallelism', 50;
RECONFIGURE;

-- Set max degree of parallelism for test workloads
EXEC sp_configure 'max degree of parallelism', 2;
RECONFIGURE;

-- Enable snapshot isolation for better test concurrency
ALTER DATABASE [RqmtMgmtTestE2E] SET ALLOW_SNAPSHOT_ISOLATION ON;
ALTER DATABASE [RqmtMgmtTestE2E] SET READ_COMMITTED_SNAPSHOT ON;

-- Optimize tempdb for test workloads
ALTER DATABASE tempdb MODIFY FILE (NAME = tempdev, SIZE = 100MB, FILEGROWTH = 10MB);
ALTER DATABASE tempdb MODIFY FILE (NAME = templog, SIZE = 50MB, FILEGROWTH = 10MB);

PRINT 'SQL Server optimized for testing environment';
