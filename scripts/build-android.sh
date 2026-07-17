#!/usr/bin/env bash
# Builds the signed Android APK headlessly from WSL2.
# Usage: scripts/build-android.sh
# Requires signing env vars (see docs/build-and-sign.md):
#   RB_KEYSTORE_PATH  e.g. /mnt/d/reflec-beat-simulator.keystore
#   RB_KEYSTORE_PASS  keystore password
#   RB_KEYALIAS_NAME  key alias, e.g. reflecbeat
#   RB_KEYALIAS_PASS  key password
# Override the Unity binary with UNITY_EXE if your install differs.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EXE="${UNITY_EXE:-/mnt/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe}"
APK="$REPO_ROOT/Build/Android/reflec-beat-simulator.apk"

if [[ ! -f "$UNITY_EXE" ]]; then
    echo "error: Unity not found at $UNITY_EXE (set UNITY_EXE to override)" >&2
    exit 1
fi

for var in RB_KEYSTORE_PATH RB_KEYSTORE_PASS RB_KEYALIAS_NAME RB_KEYALIAS_PASS; do
    if [[ -z "${!var:-}" ]]; then
        echo "error: $var is not set. See docs/build-and-sign.md for keystore setup." >&2
        exit 1
    fi
done

# Unity is a Windows process and does not inherit WSL env vars by default;
# WSLENV forwards these four by name (values never touch disk).
export WSLENV="RB_KEYSTORE_PATH:RB_KEYSTORE_PASS:RB_KEYALIAS_NAME:RB_KEYALIAS_PASS${WSLENV:+:$WSLENV}"

"$UNITY_EXE" -batchmode -nographics -quit \
    -projectPath "$(wslpath -w "$REPO_ROOT")" \
    -executeMethod ReflecBeatSimulator.EditorTools.AndroidBuild.Build \
    -logFile -

if [[ ! -f "$APK" ]]; then
    echo "error: expected APK missing at $APK" >&2
    exit 1
fi

ls -lh "$APK"
echo "Signed APK built: $APK"
