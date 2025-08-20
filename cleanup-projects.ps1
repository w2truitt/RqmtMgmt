# Script to clean up test projects, keeping only 4 for E2E testing

$baseUrl = "http://localhost:8080/api/projects"

# Projects to keep (Legacy Requirements + 3 test projects for E2E testing)
$projectsToKeep = @(1, 2, 3, 4)  # Legacy Requirements (1) + first 3 test projects

Write-Host "Fetching all projects..." -ForegroundColor Green

try {
    $uri = $baseUrl + "?pageSize=100"
    $response = Invoke-RestMethod -Uri $uri -Method GET -UseBasicParsing
    $allProjects = $response.items
    
    Write-Host "Found $($allProjects.Count) total projects" -ForegroundColor Yellow
    
    # Filter projects to delete (exclude the ones we want to keep)
    $projectsToDelete = $allProjects | Where-Object { $_.id -notin $projectsToKeep }
    
    Write-Host "Will delete $($projectsToDelete.Count) projects, keeping $($projectsToKeep.Count) projects" -ForegroundColor Yellow
    
    # Show projects that will be kept
    Write-Host "`nProjects to keep:" -ForegroundColor Green
    foreach ($id in $projectsToKeep) {
        $project = $allProjects | Where-Object { $_.id -eq $id }
        if ($project) {
            Write-Host "  - ID $($project.id): $($project.name)" -ForegroundColor Cyan
        }
    }
    
    Write-Host "`nStarting deletion process..." -ForegroundColor Yellow
    
    $deletedCount = 0
    $errorCount = 0
    
    foreach ($project in $projectsToDelete) {
        try {
            Write-Host "Deleting project $($project.id): $($project.name)" -ForegroundColor Gray
            $deleteUri = $baseUrl + "/" + $project.id
            Invoke-RestMethod -Uri $deleteUri -Method DELETE -UseBasicParsing | Out-Null
            $deletedCount++
            
            # Add small delay to avoid overwhelming the API
            Start-Sleep -Milliseconds 100
        }
        catch {
            Write-Host "Failed to delete project $($project.id): $($_.Exception.Message)" -ForegroundColor Red
            $errorCount++
        }
    }
    
    Write-Host "`nCleanup complete!" -ForegroundColor Green
    Write-Host "Successfully deleted: $deletedCount projects" -ForegroundColor Green
    Write-Host "Errors: $errorCount projects" -ForegroundColor Yellow
    
    # Verify final count
    Write-Host "`nVerifying remaining projects..." -ForegroundColor Yellow
    $finalUri = $baseUrl + "?pageSize=100"
    $finalResponse = Invoke-RestMethod -Uri $finalUri -Method GET -UseBasicParsing
    Write-Host "Remaining projects: $($finalResponse.totalItems)" -ForegroundColor Cyan
    
    foreach ($project in $finalResponse.items) {
        Write-Host "  - ID $($project.id): $($project.name) ($($project.code))" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "Error occurred: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
}
