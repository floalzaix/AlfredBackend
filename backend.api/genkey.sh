#!/usr/bin/env bash
set -euo pipefail

# Default to the folder that contains this script (Backend.API).
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project="${1:-$script_dir}"

for tool in dotnet openssl; do
    if ! command -v "$tool" >/dev/null 2>&1; then
        echo "Error: $tool must be installed." >&2
        exit 1
    fi
done

if [[ -d "$project" ]]; then
    project_file="$(find "$project" -maxdepth 1 -name "*.csproj" | head -n 1)"
else
    project_file="$project"
fi

if [[ -z "${project_file:-}" || ! -f "$project_file" ]]; then
    echo "Error: unable to find a .csproj under '$project'." >&2
    exit 1
fi

echo "Using project: $project_file"

# Ensure User Secrets are initialized for this project.
if ! dotnet user-secrets list --project "$project_file" >/dev/null 2>&1; then
    echo "Initializing User Secrets ..."
    dotnet user-secrets init --project "$project_file"
fi

secrets="$(dotnet user-secrets list --project "$project_file")"

if grep -qE '^[[:space:]]*JwtConfig:Key[[:space:]]*=' <<< "$secrets"; then
    echo "JWT key already exists: no change."
else
    key="$(openssl rand -base64 32)"

    dotnet user-secrets set "JwtConfig:Key" "$key" \
        --project "$project_file" >/dev/null

    echo "Local JWT key created and stored."
fi
