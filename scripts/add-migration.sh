#!/bin/bash

MIGRATION_NAME=$1
PROVIDER=$2

if [ -z "$MIGRATION_NAME" ]; then
  echo "❌ Migration name is required."
  echo "Usage: ./scripts/run-migration.sh MigrationName [mssql|sqlite]"
  exit 1
fi

# Default to mssql if no provider given
if [ -z "$PROVIDER" ]; then
  PROVIDER="mssql"
fi

# Configure values based on provider
if [ "$PROVIDER" == "mssql" ]; then
  CONTEXT="FinanceAppMssqlDbContext"
  PROJECT="FinanceApp.Infrastructure.EntityFramework.Mssql"
elif [ "$PROVIDER" == "sqlite" ]; then
  CONTEXT="FinanceAppSqliteDbContext"
  PROJECT="FinanceApp.Infrastructure.EntityFramework.Sqlite"
else
  echo "❌ Unknown provider: $PROVIDER"
  echo "Supported providers: mssql, sqlite"
  exit 1
fi

echo "✅ Adding migration "$MIGRATION_NAME" for "$PROVIDER...""
echo "✅ Context: $CONTEXT"
echo "✅ Project: $PROJECT..."

dotnet ef migrations add "$MIGRATION_NAME" \
  --project "$PROJECT" \
  --startup-project FinanceApp.Presentation.WebApi \
  --context "$CONTEXT"
