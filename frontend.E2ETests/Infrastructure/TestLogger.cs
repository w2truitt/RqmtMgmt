using Xunit.Abstractions;

namespace frontend.E2ETests.Infrastructure;

/// <summary>
/// Centralized logging for E2E tests with configurable verbosity levels.
/// Provides consistent logging across all test components while respecting verbosity settings.
/// </summary>
public static class TestLogger
{
    public enum LogLevel
    {
        Silent = 0,    // No output
        Minimal = 1,   // Only test results and failures
        Normal = 2,    // Include authentication and major operations
        Detailed = 3,  // Include debug information
        Verbose = 4    // All logging including internal operations
    }

    private static LogLevel _currentLevel = LogLevel.Normal;
    private static readonly object _lock = new object();

    /// <summary>
    /// Sets the global log level for all test logging.
    /// Can be controlled via environment variable XUNIT_LOG_LEVEL or programmatically.
    /// </summary>
    public static void SetLogLevel(LogLevel level)
    {
        lock (_lock)
        {
            _currentLevel = level;
        }
    }

    /// <summary>
    /// Initialize logging level from environment or configuration.
    /// Call this in test setup or static constructor.
    /// </summary>
    public static void Initialize()
    {
        var envLevel = Environment.GetEnvironmentVariable("XUNIT_LOG_LEVEL");
        if (!string.IsNullOrEmpty(envLevel) && Enum.TryParse<LogLevel>(envLevel, true, out var level))
        {
            SetLogLevel(level);
        }
    }

    /// <summary>
    /// Log authentication-related messages (session caching, login attempts).
    /// These are typically the most verbose and can be suppressed in CI environments.
    /// </summary>
    public static void LogAuthentication(string message, ITestOutputHelper? output = null)
    {
        if (_currentLevel >= LogLevel.Normal)
        {
            WriteMessage($"[AUTH] {message}", output);
        }
    }

    /// <summary>
    /// Log test execution steps and major operations.
    /// Visible at Normal level and above.
    /// </summary>
    public static void LogTestStep(string message, ITestOutputHelper? output = null)
    {
        if (_currentLevel >= LogLevel.Normal)
        {
            WriteMessage($"[TEST] {message}", output);
        }
    }

    /// <summary>
    /// Log debug information for troubleshooting.
    /// Only visible at Detailed level and above.
    /// </summary>
    public static void LogDebug(string message, ITestOutputHelper? output = null)
    {
        if (_currentLevel >= LogLevel.Detailed)
        {
            WriteMessage($"[DEBUG] {message}", output);
        }
    }

    /// <summary>
    /// Log verbose internal operations.
    /// Only visible at Verbose level.
    /// </summary>
    public static void LogVerbose(string message, ITestOutputHelper? output = null)
    {
        if (_currentLevel >= LogLevel.Verbose)
        {
            WriteMessage($"[VERBOSE] {message}", output);
        }
    }

    /// <summary>
    /// Log errors and important information.
    /// Always visible unless Silent level is set.
    /// </summary>
    public static void LogError(string message, ITestOutputHelper? output = null)
    {
        if (_currentLevel >= LogLevel.Minimal)
        {
            WriteMessage($"[ERROR] {message}", output);
        }
    }

    /// <summary>
    /// Log important information that should be visible at minimal logging.
    /// </summary>
    public static void LogInfo(string message, ITestOutputHelper? output = null)
    {
        if (_currentLevel >= LogLevel.Minimal)
        {
            WriteMessage($"[INFO] {message}", output);
        }
    }

    private static void WriteMessage(string message, ITestOutputHelper? output)
    {
        // If we have a test output helper, use it (appears in test results)
        output?.WriteLine(message);
        
        // Also write to console for immediate visibility during test runs
        // This respects the logger configuration you're using
        if (_currentLevel > LogLevel.Silent)
        {
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Get current log level for conditional logging in complex scenarios.
    /// </summary>
    public static LogLevel GetCurrentLevel() => _currentLevel;

    /// <summary>
    /// Check if a specific log level is enabled.
    /// </summary>
    public static bool IsEnabled(LogLevel level) => _currentLevel >= level;
}