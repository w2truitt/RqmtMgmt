#!/bin/bash
set -e

echo "Building and publishing Blazor WebAssembly app..."
dotnet publish -c Release -o /app/publish

echo "Starting static file server for published app..."
cd /app/publish/wwwroot
python3 -m http.server 5001 --bind 0.0.0.0
