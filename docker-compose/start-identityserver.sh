#!/bin/bash

# Install curl for health checks
apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Start the IdentityServer
exec dotnet run --urls http://0.0.0.0:5002 --project /src/identityserver.csproj