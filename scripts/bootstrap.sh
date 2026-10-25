#!/usr/bin/env bash
# Restores and builds the backend and both Angular apps.
# Run from the ReworkSuite root: ./scripts/bootstrap.sh
set -euo pipefail

echo "== 1. .NET solution =="
if [ ! -f ReworkSuite.sln ]; then
  dotnet new sln -n ReworkSuite
  dotnet sln add src/Rework.Domain/Rework.Domain.csproj
  dotnet sln add src/Rework.Application/Rework.Application.csproj
  dotnet sln add src/Rework.Infrastructure/Rework.Infrastructure.csproj
  dotnet sln add src/Rework.WebApi/Rework.WebApi.csproj
fi
dotnet restore ReworkSuite.sln
dotnet build ReworkSuite.sln --no-restore -c Debug

echo "== 2. Settings web app =="
(cd src/rework-settings-web && npm install && npm run build)

echo "== 3. Angular panel (Angular Elements) =="
(cd src/rework-panel && npm install && npm run build)

echo "Done. Next: ./scripts/setup-desktop.sh to copy the panel into the MAUI app."
