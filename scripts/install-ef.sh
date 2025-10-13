#!/usr/bin/env bash
set -euo pipefail

if ! command -v dotnet &> /dev/null; then
  echo "dotnet is not installed. Please install .NET 8 SDK first: https://dotnet.microsoft.com/download"
  exit 1
fi

echo "Installing dotnet-ef global tool..."
dotnet tool install --global dotnet-ef || echo "dotnet-ef may already be installed"
echo "Ensure ~/.dotnet/tools is on your PATH"
