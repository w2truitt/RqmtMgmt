#!/bin/bash

# Script to fix switch statement values to uppercase
echo "Fixing switch statement values to match ToUpperInvariant()..."

# Fix RequirementService.cs switch statement
sed -i 's/"title"/"TITLE"/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/RequirementService.cs
sed -i 's/"status"/"STATUS"/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/RequirementService.cs
sed -i 's/"type"/"TYPE"/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/RequirementService.cs
sed -i 's/"createdat"/"CREATEDAT"/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/RequirementService.cs
sed -i 's/"updatedat"/"UPDATEDAT"/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/RequirementService.cs

# Fix UserService.cs switch statement
sed -i 's/"username"/"USERNAME"/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/UserService.cs
sed -i 's/"email"/"EMAIL"/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/UserService.cs
sed -i 's/"createdat"/"CREATEDAT"/g' /home/wtruitt/src/repos/RqmtMgmt/backend/Services/UserService.cs

echo "Switch statement values updated to uppercase!"