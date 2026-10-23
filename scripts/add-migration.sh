#!/bin/bash

MIGRATION_NAME=$1
PROVIDER=$2

if [ -z "$MIGRATION_NAME" ]; then
  echo "❌ Migration name is required."
  echo "Usage: ./scripts/add-migration.sh MigrationName [mssql|sqlite]"
  exit 1
fi

# Default to mssql if no provider given
if [ -z "$PROVIDER" ]; then
  PROVIDER="mssql"
fi

# Configure values based on provider
if [ "$PROVIDER" == "mssql" ]; then
  PROJECT="FinanceApp.Backend.Infrastructure.EntityFramework.Mssql"
  CONTEXT="FinanceAppMssqlDbContext";
elif [ "$PROVIDER" == "sqlite" ]; then
  PROJECT="FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite"
  CONTEXT="FinanceAppSqliteDbContext";
else
  echo "❌ Unknown provider: $PROVIDER"
  echo "Supported providers: mssql, sqlite"
  exit 1
fi

echo "✅ Adding migration "$MIGRATION_NAME" for "$PROVIDER...""

dotnet ef migrations add "$MIGRATION_NAME" \
  --project "$PROJECT" \
  --startup-project "FinanceApp.Backend.Presentation.WebApi" \
  --context "$CONTEXT"
