#!/usr/bin/env bash
# Runs the Unity EditMode test suite headlessly from WSL2 and verifies results.
# Usage: scripts/run-editmode-tests.sh
# Override the Unity binary with UNITY_EXE if your install differs.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EXE="${UNITY_EXE:-/mnt/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe}"
RESULTS="$REPO_ROOT/TestResults.xml"
LOG="$REPO_ROOT/unity-test.log"

if [[ ! -f "$UNITY_EXE" ]]; then
    echo "error: Unity not found at $UNITY_EXE (set UNITY_EXE to override)" >&2
    exit 1
fi

rm -f "$RESULTS"

# Unity is a Windows process: project, results, and log paths must be Windows-style.
set +e
"$UNITY_EXE" -batchmode -nographics \
    -runTests -testPlatform EditMode \
    -projectPath "$(wslpath -w "$REPO_ROOT")" \
    -testResults "$(wslpath -w "$RESULTS")" \
    -logFile "$(wslpath -w "$LOG")"
UNITY_EXIT=$?
set -e

if [[ ! -f "$RESULTS" ]]; then
    echo "error: TestResults.xml was not produced (unity exit $UNITY_EXIT). See $LOG" >&2
    exit 1
fi

TOTAL=$(grep -oP 'total="\K[0-9]+' "$RESULTS" | head -1)
PASSED=$(grep -oP 'passed="\K[0-9]+' "$RESULTS" | head -1)
FAILED=$(grep -oP 'failed="\K[0-9]+' "$RESULTS" | head -1)
RESULT=$(grep -oP '^<test-run[^>]*result="\K[^"]+' "$RESULTS" | head -1)

echo "EditMode tests: total=${TOTAL:-?} passed=${PASSED:-?} failed=${FAILED:-?} result=${RESULT:-?}"

if [[ "$UNITY_EXIT" -ne 0 || "${FAILED:-1}" != "0" || "${RESULT:-Failed}" != "Passed" ]]; then
    echo "error: test run failed (unity exit $UNITY_EXIT). See $LOG and $RESULTS" >&2
    exit 1
fi

echo "All EditMode tests passed."
