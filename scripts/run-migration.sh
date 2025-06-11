#!/usr/bin/env bash

PROVIDER="$1"

# Default to MSSQL
if [ -z "$PROVIDER" ]; then
  PROVIDER="mssql"
fi

# Set values based on provider
case "$PROVIDER" in
  mssql)
    CONTEXT="FinanceAppMssqlDbContext"
    PROJECT="FinanceApp.Infrastructure.EntityFramework.Mssql"
    ;;
  sqlite)
    CONTEXT="FinanceAppSqliteDbContext"
    PROJECT="FinanceApp.Infrastructure.EntityFramework.Sqlite"
    ;;
  *)
    echo "❌ Unknown provider: '$PROVIDER'"
    echo "Usage: ./scripts/run-migrations.sh [mssql|sqlite]"
    exit 1
    ;;
esac

echo "📦 Applying migrations for '$PROVIDER'..."

dotnet ef database update \
  --project "$PROJECT" \
  --startup-project FinanceApp.Presentation.WebApi \
  --context "$CONTEXT"
