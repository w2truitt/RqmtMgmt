#!/bin/bash
# Script to clean build artifacts when switching between Docker and local builds

echo "Cleaning backend build artifacts..."
rm -rf backend/bin backend/obj

echo "Cleaning frontend build artifacts..."
rm -rf frontend/bin frontend/obj

echo "Cleaning test project artifacts..."
rm -rf backend.Tests/bin backend.Tests/obj
rm -rf backend.ApiTests/bin backend.ApiTests/obj
rm -rf frontend.E2ETests/bin frontend.E2ETests/obj
rm -rf frontend.ComponentTests/bin frontend.ComponentTests/obj

echo "Cleaning shared project artifacts..."
rm -rf RqmtMgmtShared/bin RqmtMgmtShared/obj

echo "Build artifacts cleaned successfully!"
echo "You can now run either Docker containers or local builds without conflicts."
