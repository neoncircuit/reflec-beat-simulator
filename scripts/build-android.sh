#!/usr/bin/env bash
# Builds the signed Android APK headlessly.
# Usage: scripts/build-android.sh
# Requires signing env vars (see docs/build-and-sign.md):
#   RB_KEYSTORE_PATH  e.g. /mnt/d/reflec-beat-simulator.keystore
#   RB_KEYSTORE_PASS  keystore password
#   RB_KEYALIAS_NAME  key alias, e.g. reflecbeat
#   RB_KEYALIAS_PASS  key password
# Override the Unity binary with UNITY_EXE if your install differs.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APK="$REPO_ROOT/Build/Android/reflec-beat-simulator.apk"

# Prefer an explicit UNITY_EXE, then a Linux editor, then the WSL Windows editor.
if [[ -z "${UNITY_EXE:-}" ]]; then
    if [[ -x /opt/unity/Editor/Unity ]]; then
        UNITY_EXE="/opt/unity/Editor/Unity"
    elif [[ -x "$HOME/Unity/Hub/Editor/2022.3.62f3/Editor/Unity" ]]; then
        UNITY_EXE="$HOME/Unity/Hub/Editor/2022.3.62f3/Editor/Unity"
    else
        UNITY_EXE="/mnt/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe"
    fi
fi

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

# Windows Unity under WSL needs Windows paths and does not inherit WSL env vars.
# WSLENV forwards the four signing values by name (values never touch disk).
to_unity_path() {
    if [[ "$UNITY_EXE" == *.exe ]]; then
        wslpath -w "$1"
    else
        printf '%s\n' "$1"
    fi
}

if [[ "$UNITY_EXE" == *.exe ]]; then
    export WSLENV="RB_KEYSTORE_PATH:RB_KEYSTORE_PASS:RB_KEYALIAS_NAME:RB_KEYALIAS_PASS${WSLENV:+:$WSLENV}"
fi

"$UNITY_EXE" -batchmode -nographics -quit \
    -projectPath "$(to_unity_path "$REPO_ROOT")" \
    -executeMethod ReflecBeatSimulator.EditorTools.AndroidBuild.Build \
    -logFile -

if [[ ! -f "$APK" ]]; then
    echo "error: expected APK missing at $APK" >&2
    exit 1
fi

ls -lh "$APK"
echo "Signed APK built: $APK"
