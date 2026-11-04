#!/usr/bin/env bash
# SessionStart hook: installs the Linux Unity editor and its license in Claude
# Code cloud sessions. It does nothing on local machines. The work is in
# scripts/setup-unity-linux.sh. Stdout becomes session context for Claude, so
# script output goes to stderr.
set -euo pipefail

if [[ "${CLAUDE_CODE_REMOTE:-}" != "true" ]]; then
    exit 0
fi

export UNITY_ROOT="${UNITY_ROOT:-/opt/unity}"
UNITY_EXE="$UNITY_ROOT/Editor/Unity"

status=0
"$CLAUDE_PROJECT_DIR/scripts/setup-unity-linux.sh" >&2 || status=$?

# Exit status 2 means that the editor is ready but the license is not.
if [[ "$status" -eq 0 || "$status" -eq 2 ]]; then
    echo "export UNITY_EXE=\"$UNITY_EXE\"" >> "$CLAUDE_ENV_FILE"
fi

case "$status" in
    0)
        echo "Unity editor is ready at $UNITY_EXE."
        if [[ -z "${UNITY_EMAIL:-}" || -z "${UNITY_PASSWORD:-}" ]]; then
            if [[ -n "${UNITY_LICENSE:-}" ]]; then
                echo "UNITY_EMAIL and UNITY_PASSWORD are not set, so the license was copied" \
                    "but not activated for this machine. Tests fail if the file is from another machine."
            else
                echo "No Unity license is configured (UNITY_EMAIL and UNITY_PASSWORD)," \
                    "so EditMode tests cannot run."
            fi
        fi
        ;;
    2)
        echo "Unity editor is ready at $UNITY_EXE, but the license setup failed" \
            "(see the SessionStart hook log). EditMode tests cannot run until it is fixed."
        ;;
    *)
        # Do not fail the session start. Tell Claude that tests cannot run.
        echo "Unity editor setup failed (see the SessionStart hook log)." \
            "EditMode tests cannot run in this session."
        ;;
esac
