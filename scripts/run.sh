#!/bin/bash

# Path to Aspire AppHost project
PROJECT_PATH="./FinanceApp.Backend.AppHost"

echo "Starting Aspire AppHost (Backend, Frontend, and LLM Processor)..."

dotnet run --project "$PROJECT_PATH"
