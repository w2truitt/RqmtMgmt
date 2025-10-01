# E2E Test Data Seeding and Verification PowerShell Script
# This script helps diagnose and fix missing test data issues

param(
    [string]$Command = "help"
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
Write-Host "🔍 E2E Test Data Seeding and Verification Tool" -ForegroundColor Cyan
Write-Host "=============================================="
Write-Host ""

function Invoke-Test {
    param(
        [string]$TestName,
        [string]$Description
    )
    
    Write-Host "📋 $Description" -ForegroundColor Yellow
    Write-Host "   Running: $TestName"
    Write-Host ""
    
    Set-Location $ProjectRoot
    & dotnet test frontend.E2ETests --filter "Name~$TestName" --logger "console;verbosity=detailed"
    Write-Host ""
}

function Restart-Services {
    Write-Host "🔄 Restarting Identity Server and Backend services..." -ForegroundColor Yellow
    Write-Host ""
    
    & kubectl delete pod -l app=identityserver --ignore-not-found=true
    & kubectl delete pod -l app=backend --ignore-not-found=true
    
    Write-Host "⏳ Waiting for services to restart..."
    Start-Sleep 10
    
    & kubectl wait --for=condition=ready pod -l app=identityserver --timeout=60s
    & kubectl wait --for=condition=ready pod -l app=backend --timeout=60s
    
    Write-Host "✅ Services restarted successfully" -ForegroundColor Green
    Write-Host ""
}

switch ($Command) {
    "verify-projects" {
        Invoke-Test "DataSeeding_VerifyRequiredProjects_ReportsStatus" "Verifying required static projects exist"
    }
    
    "seed-projects" {
        Invoke-Test "DataSeeding_SeedRequiredProjects_CreatesStaticProjects" "Seeding missing static projects"
    }
    
    "verify-users" {
        Invoke-Test "DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication" "Verifying Identity Server users can authenticate"
    }
    
    "restart-services" {
        Restart-Services
    }
    
    "show-instructions" {
        Invoke-Test "DataSeeding_ShowRecoveryInstructions_DisplaysActions" "Displaying data seeding recovery instructions"
    }
    
    "full-diagnosis" {
        Write-Host "🩺 Running full data diagnosis..." -ForegroundColor Cyan
        Write-Host ""
        
        Invoke-Test "DataSeeding_VerifyRequiredProjects_ReportsStatus" "Step 1: Verifying projects"
        Invoke-Test "DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication" "Step 2: Verifying users"
        Invoke-Test "DataSeeding_ShowRecoveryInstructions_DisplaysActions" "Step 3: Showing recovery instructions"
    }
    
    "fix-all" {
        Write-Host "🔧 Running complete data seeding fix..." -ForegroundColor Cyan
        Write-Host ""
        
        Write-Host "Step 1: Seeding missing projects..."
        Invoke-Test "DataSeeding_SeedRequiredProjects_CreatesStaticProjects" "Seeding projects"
        
        Write-Host "Step 2: Restarting services to trigger user seeding..."
        Restart-Services
        
        Write-Host "Step 3: Verifying everything is fixed..."
        Invoke-Test "DataSeeding_VerifyRequiredProjects_ReportsStatus" "Verifying projects"
        Invoke-Test "DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication" "Verifying users"
        
        Write-Host "✅ Data seeding fix completed!" -ForegroundColor Green
    }
    
    "pm-test" {
        Write-Host "🔍 Testing PM User Authentication..." -ForegroundColor Cyan
        Invoke-Test "UserVerification_CheckPMUser_SpecificCheck" "Testing PM user login"
    }
    
    default {
        Write-Host "Usage: .\fix-e2e-data.ps1 <command>" -ForegroundColor White
        Write-Host ""
        Write-Host "Commands:" -ForegroundColor Yellow
        Write-Host "  verify-projects    - Check if required static projects exist"
        Write-Host "  seed-projects      - Create missing static projects"
        Write-Host "  verify-users       - Check if Identity Server users can authenticate"
        Write-Host "  restart-services   - Restart Identity Server and Backend pods"
        Write-Host "  show-instructions  - Display detailed recovery instructions"
        Write-Host "  full-diagnosis     - Run complete diagnosis (recommended first step)"
        Write-Host "  fix-all           - Attempt to fix all missing data automatically"
        Write-Host "  pm-test           - Test PM user authentication specifically"
        Write-Host ""
        Write-Host "Recommended workflow:" -ForegroundColor Green
        Write-Host "1. .\fix-e2e-data.ps1 pm-test        # Test the failing PM user"
        Write-Host "2. .\fix-e2e-data.ps1 full-diagnosis  # Identify what's missing"
        Write-Host "3. .\fix-e2e-data.ps1 fix-all        # Attempt to fix everything"
        Write-Host "4. Run regular E2E tests to verify"
        Write-Host ""
    }
}