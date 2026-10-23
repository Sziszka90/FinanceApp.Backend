#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_PATH="$SCRIPT_DIR/FinanceApp.Backend.AppHost/FinanceApp.Backend.AppHost.csproj"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Error: .NET SDK is required but 'dotnet' was not found in PATH." >&2
  exit 1
fi

if [[ ! -f "$PROJECT_PATH" ]]; then
  echo "Error: Aspire AppHost project not found: $PROJECT_PATH" >&2
  exit 1
fi

# Prefer the installed Homebrew Node 24 runtime when available. Angular 22
# requires Node 24.15 or newer, while the unlinked Homebrew node may be older.
if command -v brew >/dev/null 2>&1; then
  NODE24_PREFIX="$(brew --prefix node@24 2>/dev/null || true)"
  if [[ -n "$NODE24_PREFIX" && -x "$NODE24_PREFIX/bin/node" ]]; then
    export PATH="$NODE24_PREFIX/bin:$PATH"
  fi
fi

echo "Starting Aspire AppHost (Backend, Frontend, and LLM Processor)..."
export DOTNET_ENVIRONMENT=Development
export ASPNETCORE_ENVIRONMENT=Development
exec dotnet run --project "$PROJECT_PATH" -- "$@"
