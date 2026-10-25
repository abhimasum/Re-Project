#!/usr/bin/env bash
# Starts the API on https://localhost:7180.
# First time only, create the initial migration and apply it (see README step 3).
# Set the connection string outside source control:
#   export ConnectionStrings__ReworkDb="Server=localhost,1433;Database=ReworkSuite;User Id=sa;Password=...;TrustServerCertificate=True;"
set -euo pipefail

if [ ! -d src/Rework.Infrastructure/Migrations ]; then
  echo "No migrations found. Create them first:" >&2
  echo "  dotnet ef migrations add InitialCreate --project src/Rework.Infrastructure --startup-project src/Rework.WebApi" >&2
  exit 1
fi

dotnet ef database update --project src/Rework.Infrastructure --startup-project src/Rework.WebApi
dotnet run --project src/Rework.WebApi --urls https://localhost:7180
