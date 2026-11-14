#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_PATH="$SCRIPT_DIR/FinanceApp.Backend.AppHost/FinanceApp.Backend.AppHost.csproj"
FRONTEND_PATH="$(cd "$SCRIPT_DIR/../FinanceApp.Frontend" && pwd -P)"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Error: .NET SDK is required but 'dotnet' was not found in PATH." >&2
  exit 1
fi

if [[ ! -f "$PROJECT_PATH" ]]; then
  echo "Error: Aspire AppHost project not found: $PROJECT_PATH" >&2
  exit 1
fi

# Stop an older Angular server from this repository so AppHost's newly built
# frontend is the one listening on the expected port.
if command -v lsof >/dev/null 2>&1; then
  FRONTEND_PIDS="$(lsof -tiTCP:4200 -sTCP:LISTEN 2>/dev/null || true)"
  while IFS= read -r process_id; do
    [[ -n "$process_id" ]] || continue
    PROCESS_DIR="$(lsof -a -p "$process_id" -d cwd -Fn 2>/dev/null | sed -n 's/^n//p' | head -n 1)"
    if [[ "$PROCESS_DIR" == "$FRONTEND_PATH" ]]; then
      echo "Stopping previous FinanceApp frontend server (PID $process_id)..."
      if ! kill "$process_id"; then
        echo "Error: Could not stop the previous frontend server (PID $process_id)." >&2
        exit 1
      fi
    else
      echo "Error: Port 4200 is in use by another process (PID $process_id). Stop it before starting FinanceApp." >&2
      exit 1
    fi
  done <<< "$FRONTEND_PIDS"
  if [[ -n "$FRONTEND_PIDS" ]]; then
    sleep 1
  fi
fi

# Prefer the installed Homebrew Node 24 runtime when available. Angular 22
# requires Node 24.15 or newer, while the unlinked Homebrew node may be older.
if command -v brew >/dev/null 2>&1; then
  NODE24_PREFIX="$(brew --prefix node@24 2>/dev/null || true)"
  if [[ -n "$NODE24_PREFIX" && -x "$NODE24_PREFIX/bin/node" ]]; then
    export PATH="$NODE24_PREFIX/bin:$PATH"
  fi
fi

echo "Starting Aspire AppHost (Backend, Frontend, and AI Analytics)..."
echo "The Angular frontend will compile at startup and poll for source changes."
export DOTNET_ENVIRONMENT=Development
export ASPNETCORE_ENVIRONMENT=Development
exec dotnet run --project "$PROJECT_PATH" -- "$@"
