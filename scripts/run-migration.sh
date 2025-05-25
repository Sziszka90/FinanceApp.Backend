#!/bin/bash

PROVIDER=$1

# Default to MSSQL
if [ -z "$PROVIDER" ]; then
  PROVIDER="mssql"
fi

# Set correct values based on provider
case "$PROVIDER" in
  mssql)
    CONTEXT="FinanceAppMssqlDbContext"
    PROJECT="FinanceApp.Infrastructure.EntityFramework.Mssql"
    ;;
  sqlite)
    CONTEXT="FinanceAppDesignTimeSqliteDbContext"
    PROJECT="FinanceApp.Infrastructure.EntityFramework.Sqlite"
    ;;
  *)
    echo "❌ Unknown provider: '$PROVIDER'"
    echo "Usage: ./scripts/run-migration.sh [mssql|sqlite]"
    exit 1
    ;;
esac

# Apply the migrations
echo "📦 Applying migrations for '$PROVIDER'..."
dotnet ef database update \
  --project "$PROJECT" \
  --startup-project FinanceApp.Presentation.WebApi \
  --context "$CONTEXT"
