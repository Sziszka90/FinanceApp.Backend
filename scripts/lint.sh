#!/bin/bash

# Simple code analysis script for FinanceApp Backend
# Uses the linting setup from Directory.Build.props and .editorconfig

set -e

SOLUTION_PATH="FinanceApp.Backend.sln"

echo "🔍 Running code analysis for FinanceApp Backend..."

# Check if solution exists
if [[ ! -f "$SOLUTION_PATH" ]]; then
    echo "❌ Solution file not found: $SOLUTION_PATH"
    exit 1
fi

# 1. Restore packages
echo "📦 Restoring packages..."
dotnet restore "$SOLUTION_PATH"

# 2. Build with analysis enabled
echo "🔨 Building with code analysis..."
dotnet build "$SOLUTION_PATH" --no-restore --verbosity normal

# 3. Format check (optional - shows what would be changed)
echo "🎨 Checking code formatting..."
if dotnet format "$SOLUTION_PATH" --verify-no-changes --verbosity normal; then
    echo "✅ Code formatting is good!"
else
    echo "⚠️  Code formatting issues found. Run 'dotnet format' to fix them."
fi

echo "✨ Code analysis complete!"
