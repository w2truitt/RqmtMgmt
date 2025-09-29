#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Refactors E2E test files to use the new TestLogger system instead of Output.WriteLine calls.

.DESCRIPTION
    This script automatically transforms all Output.WriteLine calls in the Workflows directory
    to use the appropriate TestLogger methods based on the content and context of the message.

.PARAMETER DryRun
    If specified, shows what changes would be made without actually modifying files.

.PARAMETER BackupFiles
    If specified, creates .bak files before making changes.

.EXAMPLE
    .\refactor-to-testlogger.ps1 -DryRun
    Shows what changes would be made without modifying files.

.EXAMPLE
    .\refactor-to-testlogger.ps1 -BackupFiles
    Makes changes and creates backup files.
#>

param(
    [switch]$DryRun,
    [switch]$BackupFiles
)

# Define the transformation rules
$transformationRules = @{
    # Authentication-related messages
    'authenticated|authentication|login|session|token|cache' = 'TestLogger.LogAuthentication'
    
    # Test step messages
    'testing|verifying|checking|validating|navigating|creating|updating|deleting|starting|completing|finished' = 'TestLogger.LogTestStep'
    
    # Debug information
    'debug|found|current|page|url|title|element|input|form|button' = 'TestLogger.LogDebug'
    
    # Error messages
    'error|failed|exception|invalid|missing' = 'TestLogger.LogError'
    
    # Default for other messages
    'default' = 'TestLogger.LogTestStep'
}

function Get-LoggerMethod {
    param([string]$message)
    
    $lowerMessage = $message.ToLower()
    
    # Check each pattern
    foreach ($pattern in $transformationRules.Keys) {
        if ($pattern -eq 'default') { continue }
        
        $keywords = $pattern -split '\|'
        foreach ($keyword in $keywords) {
            if ($lowerMessage -contains $keyword) {
                return $transformationRules[$pattern]
            }
        }
    }
    
    # Return default
    return $transformationRules['default']
}

function Transform-File {
    param(
        [string]$FilePath,
        [bool]$DryRun,
        [bool]$BackupFiles
    )
    
    Write-Host "Processing: $FilePath" -ForegroundColor Yellow
    
    $content = Get-Content -Path $FilePath -Raw
    $originalContent = $content
    $changes = @()
    
    # Check if file already has the using statement
    $hasUsingStatement = $content -match 'using frontend\.E2ETests\.Infrastructure;'
    
    # Add using statement if not present
    if (-not $hasUsingStatement) {
        # Find the last using statement
        $lines = $content -split "`n"
        $lastUsingIndex = -1
        
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match '^using\s+') {
                $lastUsingIndex = $i
            }
        }
        
        if ($lastUsingIndex -ge 0) {
            $lines = $lines[0..$lastUsingIndex] + "using frontend.E2ETests.Infrastructure;" + $lines[($lastUsingIndex + 1)..($lines.Count - 1)]
            $content = $lines -join "`n"
            $changes += "Added using statement for TestLogger"
        }
    }
    
    # Transform Output.WriteLine calls
    $outputWriteLinePattern = 'Output\.WriteLine\(([^)]+)\);'
    $matches = [regex]::Matches($content, $outputWriteLinePattern)
    
    foreach ($match in $matches) {
        $fullMatch = $match.Value
        $messageContent = $match.Groups[1].Value.Trim()
        
        # Extract the actual message content for analysis
        $messageForAnalysis = $messageContent
        if ($messageContent -match '^"([^"]*)"$') {
            $messageForAnalysis = $matches.Groups[1].Value
        } elseif ($messageContent -match '^\$"([^"]*)"$') {
            $messageForAnalysis = $matches.Groups[1].Value
        }
        
        # Determine the appropriate logger method
        $loggerMethod = Get-LoggerMethod $messageForAnalysis
        
        # Create the replacement
        $replacement = "$loggerMethod($messageContent, Output);"
        
        # Replace in content
        $content = $content -replace [regex]::Escape($fullMatch), $replacement
        
        $changes += "Changed: $fullMatch -> $replacement"
    }
    
    if ($changes.Count -gt 0) {
        Write-Host "  Found $($changes.Count) changes:" -ForegroundColor Green
        foreach ($change in $changes) {
            Write-Host "    $change" -ForegroundColor Cyan
        }
        
        if (-not $DryRun) {
            if ($BackupFiles) {
                $backupPath = "$FilePath.bak"
                Copy-Item -Path $FilePath -Destination $backupPath -Force
                Write-Host "  Created backup: $backupPath" -ForegroundColor Blue
            }
            
            Set-Content -Path $FilePath -Value $content -NoNewline
            Write-Host "  File updated successfully!" -ForegroundColor Green
        } else {
            Write-Host "  [DRY RUN] File would be updated" -ForegroundColor Magenta
        }
    } else {
        Write-Host "  No changes needed" -ForegroundColor Gray
    }
    
    Write-Host ""
}

# Main execution
Write-Host "E2E Test Logger Refactoring Script" -ForegroundColor White -BackgroundColor DarkBlue
Write-Host "=================================" -ForegroundColor White -BackgroundColor DarkBlue

if ($DryRun) {
    Write-Host "DRY RUN MODE - No files will be modified" -ForegroundColor Yellow -BackgroundColor DarkRed
}

if ($BackupFiles -and -not $DryRun) {
    Write-Host "BACKUP MODE - .bak files will be created" -ForegroundColor Yellow -BackgroundColor DarkGreen
}

Write-Host ""

# Get all C# files in the Workflows directory
$workflowsPath = "frontend.E2ETests/Workflows"
if (-not (Test-Path $workflowsPath)) {
    Write-Host "Error: Workflows directory not found at $workflowsPath" -ForegroundColor Red
    Write-Host "Please run this script from the repository root directory." -ForegroundColor Red
    exit 1
}

$files = Get-ChildItem -Path $workflowsPath -Filter "*.cs" | Where-Object { $_.Name -notlike "*TestBase*" }

Write-Host "Found $($files.Count) files to process" -ForegroundColor Green
Write-Host ""

foreach ($file in $files) {
    Transform-File -FilePath $file.FullName -DryRun $DryRun -BackupFiles $BackupFiles
}

Write-Host "Refactoring complete!" -ForegroundColor Green

if ($DryRun) {
    Write-Host ""
    Write-Host "To apply these changes, run the script without -DryRun:" -ForegroundColor Yellow
    Write-Host "  .\refactor-to-testlogger.ps1" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To apply changes with backups:" -ForegroundColor Yellow
    Write-Host "  .\refactor-to-testlogger.ps1 -BackupFiles" -ForegroundColor Cyan
}