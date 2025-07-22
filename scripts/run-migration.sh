#!/usr/bin/env bash

PROVIDER="$1"
CONNECTION="$2"

# Default to MSSQL
if [ -z "$PROVIDER" ]; then
  PROVIDER="mssql"
fi

# Set values based on provider
case "$PROVIDER" in
  mssql)
    CONTEXT="FinanceAppMssqlDbContext"
    PROJECT="FinanceApp.Backend.Infrastructure.EntityFramework.Mssql"
    ;;
  sqlite)
    CONTEXT="FinanceAppSqliteDbContext"
    PROJECT="FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite"
    ;;
  *)
    echo "❌ Unknown provider: '$PROVIDER'"
    echo "Usage: ./scripts/run-migration.sh [mssql|sqlite] [connectionString]"
    exit 1
    ;;
esac

echo "📦 Applying migrations for '$PROVIDER'..."

if [ -z "$CONNECTION" ]; then
  dotnet ef database update \
    --project "$PROJECT" \
    --startup-project FinanceApp.Backend.Presentation.WebApi \
    --context "$CONTEXT"
else
  dotnet ef database update \
    --project "$PROJECT" \
    --startup-project FinanceApp.Backend.Presentation.WebApi \
    --context "$CONTEXT" \
    --connection "$CONNECTION"
fi
