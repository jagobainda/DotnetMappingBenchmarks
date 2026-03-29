#!/usr/bin/env bash
set -euo pipefail

LOG_DIR="/var/log/benchmarkworker"
LOG_FILE="${LOG_DIR}/version_check.log"
CSPROJ_PATH="$(cd "$(dirname "$0")/.." && pwd)/DotnetMappingBenchmarks/DotnetMappingBenchmarks.csproj"
SERVICE_NAME="benchmarkworker"

mkdir -p "${LOG_DIR}"

log() {
    local timestamp
    timestamp=$(TZ="Europe/Madrid" date '+%Y-%m-%d %H:%M:%S %Z')
    echo "[${timestamp}] $*" | tee -a "${LOG_FILE}"
}

get_nuget_latest_version() {
    local package_id="$1"
    local lower_id
    lower_id=$(echo "${package_id}" | tr '[:upper:]' '[:lower:]')
    local url="https://api.nuget.org/v3-flatcontainer/${lower_id}/index.json"
    local response
    response=$(curl -s --fail "${url}" 2>/dev/null) || { log "ERROR: Failed to query NuGet for ${package_id}"; echo ""; return; }
    echo "${response}" | grep -oP '"[0-9][^"]*"' | tail -1 | tr -d '"'
}

get_csproj_version() {
    local package_id="$1"
    grep -oP "PackageReference Include=\"${package_id}\" Version=\"\K[^\"]*" "${CSPROJ_PATH}" 2>/dev/null || echo ""
}

PACKAGES=(
    "AutoMapper"
    "Mapster"
    "Riok.Mapperly"
    "TinyMapper"
    "Microsoft.Extensions.Hosting"
)

log "=== Starting version check ==="

if [ ! -f "${CSPROJ_PATH}" ]; then
    log "ERROR: .csproj not found at ${CSPROJ_PATH}"
    exit 1
fi

needs_update=false

for package in "${PACKAGES[@]}"; do
    current_version=$(get_csproj_version "${package}")
    if [ -z "${current_version}" ]; then
        log "WARNING: Package ${package} not found in .csproj, skipping"
        continue
    fi

    latest_version=$(get_nuget_latest_version "${package}")
    if [ -z "${latest_version}" ]; then
        log "WARNING: Could not determine latest version for ${package}, skipping"
        continue
    fi

    if [ "${current_version}" = "${latest_version}" ]; then
        log "OK: ${package} is up to date (${current_version})"
    else
        log "UPDATE: ${package} ${current_version} -> ${latest_version}"
        dotnet add "${CSPROJ_PATH}" package "${package}" --version "${latest_version}" >> "${LOG_FILE}" 2>&1
        if [ $? -eq 0 ]; then
            log "SUCCESS: Updated ${package} to ${latest_version}"
            needs_update=true
        else
            log "ERROR: Failed to update ${package} to ${latest_version}"
        fi
    fi
done

if [ "${needs_update}" = true ]; then
    log "Cleaning project (bin/obj)..."
    project_dir=$(dirname "${CSPROJ_PATH}")
    rm -rf "${project_dir}/bin" "${project_dir}/obj"

    log "Restoring packages (no cache)..."
    dotnet restore "${CSPROJ_PATH}" --no-cache >> "${LOG_FILE}" 2>&1

    log "Building project to verify updates..."
    if dotnet build "${CSPROJ_PATH}" --configuration Release --no-restore >> "${LOG_FILE}" 2>&1; then
        log "SUCCESS: Build succeeded after package updates"

        log "Resolved package versions:"
        dotnet list "${CSPROJ_PATH}" package >> "${LOG_FILE}" 2>&1

        log "Restarting ${SERVICE_NAME} service..."
        if systemctl restart "${SERVICE_NAME}" >> "${LOG_FILE}" 2>&1; then
            log "SUCCESS: Service ${SERVICE_NAME} restarted"
        else
            log "ERROR: Failed to restart ${SERVICE_NAME} service"
        fi
    else
        log "ERROR: Build failed after package updates. Manual intervention required."
        exit 1
    fi
else
    log "All packages are up to date, no restart needed"
fi

log "=== Version check completed ==="