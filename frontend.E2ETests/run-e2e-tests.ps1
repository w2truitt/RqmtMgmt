# E2E Test Runner with Logging Control
# Usage: .\run-e2e-tests.ps1 [LogLevel] [TestFilter]
# Log levels: Silent, Minimal, Normal, Detailed, Verbose
# Examples:
#   .\run-e2e-tests.ps1 Minimal                    # Run all tests with minimal logging
#   .\run-e2e-tests.ps1 Normal "*Authentication*"  # Run authentication tests with normal logging
#   .\run-e2e-tests.ps1 Detailed                   # Run all tests with detailed logging

param(
    [Parameter(Position=0)]
    [ValidateSet("Silent", "Minimal", "Normal", "Detailed", "Verbose")]
    [string]$LogLevel = "Normal",
    
    [Parameter(Position=1)]
    [string]$TestFilter = ""
)

Write-Host "Running E2E tests with log level: $LogLevel" -ForegroundColor Green

# Set environment variable for test logger
$env:XUNIT_LOG_LEVEL = $LogLevel

# Build the dotnet test command
$cmd = @("dotnet", "test")

# Add test filter if provided
if ($TestFilter) {
    $cmd += "--filter"
    $cmd += $TestFilter
}

# Add logger configuration based on log level
switch ($LogLevel) {
    "Silent" {
        $cmd += "--logger=console;verbosity=quiet"
        $cmd += "--verbosity=quiet"
    }
    "Minimal" {
        $cmd += "--logger=console;verbosity=minimal"
        $cmd += "--verbosity=minimal"
    }
    "Normal" {
        $cmd += "--logger=console;verbosity=normal"
        $cmd += "--verbosity=normal"
    }
    "Detailed" {
        $cmd += "--logger=console;verbosity=detailed"
        $cmd += "--verbosity=detailed"
    }
    "Verbose" {
        $cmd += "--logger=console;verbosity=diagnostic"
        $cmd += "--verbosity=diagnostic"
    }
}

Write-Host "Executing: $($cmd -join ' ')" -ForegroundColor Yellow
& $cmd[0] $cmd[1..($cmd.Length-1)]