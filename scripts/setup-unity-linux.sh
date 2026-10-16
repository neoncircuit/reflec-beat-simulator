#!/usr/bin/env bash
# Installs the Linux Unity editor for headless EditMode tests (cloud sessions, CI).
# Usage: scripts/setup-unity-linux.sh   (run as root to install)
# The editor version comes from ProjectSettings/ProjectVersion.txt.
# The editor goes to $UNITY_ROOT/Editor/Unity (default: /opt/unity/Editor/Unity).
# The script is idempotent: it skips the download when that version is present.
#
# License (optional, the value is never printed):
#   UNITY_LICENSE  Personal license file (Unity_lic.ulf) contents, raw XML or base64.
#                  Activate once in Unity Hub, then copy the file from
#                  ~/.local/share/unity3d/Unity/ (Linux) or C:\ProgramData\Unity\ (Windows).
#
# Network: the download needs download.unity3d.com. When the editor opens the
# project, it also needs packages.unity.com, download.packages.unity.com, and
# license.unity3d.com.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_ROOT="${UNITY_ROOT:-/opt/unity}"
UNITY_EXE="$UNITY_ROOT/Editor/Unity"
MARKER="$UNITY_ROOT/.installed-version"
DOWNLOAD_BASE="https://download.unity3d.com/download_unity"
LICENSE_DIR="$HOME/.local/share/unity3d/Unity"

# Read "2022.3.62f3 (96770f904ca7)" so the editor always matches the project.
VERSION_LINE="$(tr -d '\r' < "$REPO_ROOT/ProjectSettings/ProjectVersion.txt" |
    sed -n 's/^m_EditorVersionWithRevision: //p')"
if [[ "$VERSION_LINE" =~ ^([^ ]+)\ \(([0-9a-f]+)\)$ ]]; then
    UNITY_VERSION="${BASH_REMATCH[1]}"
    UNITY_CHANGESET="${BASH_REMATCH[2]}"
else
    echo "error: cannot read the editor version from ProjectSettings/ProjectVersion.txt" >&2
    exit 1
fi

# first_available NAME...
# Prints the first package NAME that apt can install, so the script works with
# the Ubuntu 24.04 "t64" package names and the older names. Returns 1 if none.
first_available() {
    local name
    for name in "$@"; do
        # No grep -q: it can stop early and break the pipe under pipefail.
        if apt-cache policy "$name" 2>/dev/null | grep 'Candidate: [^(]' > /dev/null; then
            echo "$name"
            return 0
        fi
    done
    echo "error: apt cannot install any of: $*" >&2
    return 1
}

# install_dependencies
# Installs the shared libraries that the editor loads, also in batch mode.
install_dependencies() {
    local alsa gtk
    apt-get update -qq
    alsa="$(first_available libasound2t64 libasound2)"
    gtk="$(first_available libgtk-3-0t64 libgtk-3-0)"
    DEBIAN_FRONTEND=noninteractive apt-get install -y -qq --no-install-recommends \
        "$alsa" "$gtk" libglu1-mesa libnss3 libxtst6 libxss1 libnotify4 libcap2 \
        ca-certificates curl xz-utils
}

# editor_archive_path
# Prints the editor archive path below $DOWNLOAD_BASE/<changeset>/. Reads the
# [Unity] url from the release .ini; uses the usual file name if that fails.
editor_archive_path() {
    local ini path=""
    if ini="$(curl -fsSL --retry 3 \
        "$DOWNLOAD_BASE/$UNITY_CHANGESET/unity-$UNITY_VERSION-linux.ini" 2>/dev/null)"; then
        path="$(printf '%s\n' "$ini" | tr -d '\r' |
            awk -F= '/^\[/ { s = ($0 == "[Unity]") } s && $1 == "url" { print $2; exit }')"
    fi
    echo "${path:-LinuxEditorInstaller/Unity-$UNITY_VERSION.tar.xz}"
}

# install_editor
# Downloads the editor into a staging directory, then moves it to $UNITY_ROOT.
# The archive streams into tar, so the multi-GB file never touches the disk.
install_editor() {
    local url staging
    url="$DOWNLOAD_BASE/$UNITY_CHANGESET/$(editor_archive_path)"
    staging="$UNITY_ROOT.partial"
    rm -rf "$staging"
    mkdir -p "$staging"

    echo "Downloading Unity $UNITY_VERSION from $url (several GB)..."
    if ! curl -fSL --retry 3 "$url" | tar -xJ -C "$staging"; then
        rm -rf "$staging"
        echo "error: download or unpack failed: $url" >&2
        return 1
    fi
    if [[ ! -x "$staging/Editor/Unity" ]]; then
        rm -rf "$staging"
        echo "error: the archive has no Editor/Unity: $url" >&2
        return 1
    fi

    echo "$UNITY_VERSION" > "$staging/.installed-version"
    rm -rf "$UNITY_ROOT"
    mv "$staging" "$UNITY_ROOT"
}

# install_license
# Writes $UNITY_LICENSE to the path where the Linux editor reads it.
install_license() {
    if [[ -z "${UNITY_LICENSE:-}" ]]; then
        echo "warning: UNITY_LICENSE is not set. The editor cannot run tests without a license." >&2
        return 0
    fi
    mkdir -p "$LICENSE_DIR"
    (
        umask 077
        if [[ "$UNITY_LICENSE" == "<"* ]]; then
            printf '%s\n' "$UNITY_LICENSE" > "$LICENSE_DIR/Unity_lic.ulf"
        else
            printf '%s' "$UNITY_LICENSE" | base64 -d > "$LICENSE_DIR/Unity_lic.ulf"
        fi
    )
    echo "Wrote the Unity license to $LICENSE_DIR/Unity_lic.ulf."
}

if [[ -x "$UNITY_EXE" && "$(cat "$MARKER" 2>/dev/null)" == "$UNITY_VERSION" ]]; then
    echo "Unity $UNITY_VERSION is already installed at $UNITY_EXE."
else
    if [[ "$EUID" -ne 0 ]]; then
        echo "error: run as root to install packages and write $UNITY_ROOT" >&2
        exit 1
    fi
    install_dependencies
    install_editor
    echo "Installed Unity $UNITY_VERSION at $UNITY_EXE."
fi

install_license
