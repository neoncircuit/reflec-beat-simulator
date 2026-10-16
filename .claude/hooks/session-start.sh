#!/usr/bin/env bash
# SessionStart hook: installs the Linux Unity editor in Claude Code cloud sessions.
# It does nothing on local machines. The work is in scripts/setup-unity-linux.sh.
# Stdout becomes session context for Claude, so script output goes to stderr.
set -euo pipefail

if [[ "${CLAUDE_CODE_REMOTE:-}" != "true" ]]; then
    exit 0
fi

export UNITY_ROOT="${UNITY_ROOT:-/opt/unity}"

if "$CLAUDE_PROJECT_DIR/scripts/setup-unity-linux.sh" >&2; then
    echo "export UNITY_EXE=\"$UNITY_ROOT/Editor/Unity\"" >> "$CLAUDE_ENV_FILE"
    echo "Unity editor is ready at $UNITY_ROOT/Editor/Unity."
    if [[ -z "${UNITY_LICENSE:-}" ]]; then
        echo "UNITY_LICENSE is not set, so the editor has no license and cannot run tests."
    fi
else
    # Do not fail the session start. Tell Claude that tests cannot run.
    echo "Unity editor setup failed (see the SessionStart hook log)." \
        "EditMode tests cannot run in this session."
fi
