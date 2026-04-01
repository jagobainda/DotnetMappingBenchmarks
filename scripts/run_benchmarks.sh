#!/usr/bin/env bash
set -euo pipefail

LOG_DIR="/var/log/benchmarkworker"
LOG_FILE="${LOG_DIR}/benchmarkworker.log"
WORKDIR="/opt/benchmarkworker"
DOTNET_BIN="/usr/bin/dotnet"
DLL="${WORKDIR}/DotnetMappingBenchmarks.dll"

mkdir -p "${LOG_DIR}"

log() {
    local timestamp
    timestamp=$(TZ="Europe/Madrid" date '+%Y-%m-%d %H:%M:%S %Z')
    echo "[${timestamp}] $*" | tee -a "${LOG_FILE}"
}

log "=== Starting benchmark run ==="

export DOTNET_ENVIRONMENT=Production
export TZ=Europe/Madrid

cd "${WORKDIR}"
"${DOTNET_BIN}" "${DLL}" >> "${LOG_FILE}" 2>&1
EXIT_CODE=$?

if [ ${EXIT_CODE} -eq 0 ]; then
    log "=== Benchmark run completed successfully ==="
else
    log "ERROR: Benchmark run failed with exit code ${EXIT_CODE}"
fi

exit ${EXIT_CODE}
