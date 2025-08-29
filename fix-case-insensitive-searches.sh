#!/bin/bash

# Script to fix case-insensitive search performance issues
# Replace ToLower() with ToUpperInvariant() for better performance

echo "Fixing case-insensitive search performance issues..."

# Fix RequirementService.cs
sed -i 's/parameters\.SortBy?\.ToLower()/parameters.SortBy?.ToUpperInvariant()/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/RequirementService.cs

# Fix UserService.cs  
sed -i 's/parameters\.SortBy\.ToLower()/parameters.SortBy.ToUpperInvariant()/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/UserService.cs

# Fix RoleService.cs
sed -i 's/r\.Name\.ToLower() == roleName\.ToLower()/string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase)/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/RoleService.cs

# Fix EnhancedDashboardService.cs
sed -i 's/trs\.Status\.ToString()\.ToLower()/trs.Status.ToString().ToLowerInvariant()/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/EnhancedDashboardService.cs

echo "Case-insensitive search fixes applied!"