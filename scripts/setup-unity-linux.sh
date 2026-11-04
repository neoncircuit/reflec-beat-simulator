#!/usr/bin/env bash
# Installs the Linux Unity editor for headless EditMode tests (cloud sessions, CI).
# Usage: scripts/setup-unity-linux.sh   (run as root to install)
# The editor version comes from ProjectSettings/ProjectVersion.txt.
# The editor goes to $UNITY_ROOT/Editor/Unity (default: /opt/unity/Editor/Unity).
# The script is idempotent: it skips the download when that version is present,
# and it skips the license activation when this machine has a current license.
#
# License (optional; the script never prints these values):
#   UNITY_EMAIL     Unity ID email.
#   UNITY_PASSWORD  Unity ID password. The account must not use two-factor login.
#   UNITY_LICENSE   Personal license file (Unity_lic.ulf) contents, raw XML or base64.
#                   Activate once in Unity Hub, then copy the file from
#                   ~/.local/share/unity3d/Unity/ (Linux) or C:\ProgramData\Unity\ (Windows).
#   UNITY_SERIAL    Serial number. Optional: the script reads it from UNITY_LICENSE.
# With UNITY_EMAIL and UNITY_PASSWORD, the script activates a new license for
# this machine online (the GameCI method). Without them, it only copies
# UNITY_LICENSE. A copied file works only on the machine that made it, because
# Unity binds the file to that machine.
#
# Exit status: 0 = editor ready (license activated, copied, or not configured),
# 1 = editor setup failed, 2 = editor ready but the license setup failed.
#
# Network: the download needs download.unity3d.com. When the editor opens the
# project, it also needs packages.unity.com, download.packages.unity.com,
# cdn.packages.unity.com (package files), and license.unity3d.com. The
# activation also needs core.cloud.unity3d.com, activation.unity3d.com, and
# api.unity.com.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_ROOT="${UNITY_ROOT:-/opt/unity}"
UNITY_EXE="$UNITY_ROOT/Editor/Unity"
MARKER="$UNITY_ROOT/.installed-version"
DOWNLOAD_BASE="https://download.unity3d.com/download_unity"
LICENSE_DIR="$HOME/.local/share/unity3d/Unity"
LICENSE_FILE="$LICENSE_DIR/Unity_lic.ulf"
# Written after a successful activation, so the next run can skip it.
ACTIVATED_MARKER="$LICENSE_DIR/.activated-by-setup"
# Activate again when the license expires in less than this many seconds.
LICENSE_MIN_REMAINING_S=86400

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

# license_xml
# Prints $UNITY_LICENSE as XML. Decodes it first if it is base64.
license_xml() {
    if [[ "$UNITY_LICENSE" == "<"* ]]; then
        printf '%s\n' "$UNITY_LICENSE"
    else
        printf '%s' "$UNITY_LICENSE" | base64 -d
    fi
}

# license_serial
# Prints the serial: $UNITY_SERIAL, or the serial in the DeveloperData field of
# $UNITY_LICENSE. That field is base64, and its first 4 bytes are a header
# (the same method as GameCI). Returns 1 if there is no valid serial.
license_serial() {
    local data serial=""
    if [[ -n "${UNITY_SERIAL:-}" ]]; then
        serial="$UNITY_SERIAL"
    elif [[ -n "${UNITY_LICENSE:-}" ]]; then
        data="$(license_xml | sed -n 's/.*<DeveloperData Value="\([^"]*\)".*/\1/p' | sed -n 1p)"
        if [[ -n "$data" ]]; then
            serial="$(printf '%s' "$data" | base64 -d 2>/dev/null | tail -c +5 | tr -d '\0')"
        fi
    fi
    [[ "$serial" =~ ^[A-Za-z0-9]{2}(-[A-Za-z0-9]{4}){5}$ ]] || return 1
    printf '%s\n' "$serial"
}

# license_is_current
# Returns 0 if this script activated the license in this container and the
# license does not expire within $LICENSE_MIN_REMAINING_S seconds.
license_is_current() {
    local stop stop_s
    [[ -f "$ACTIVATED_MARKER" && -f "$LICENSE_FILE" ]] || return 1
    stop="$(sed -n 's/.*<StopDate Value="\([^"]*\)".*/\1/p' "$LICENSE_FILE" | sed -n 1p)"
    # An empty StopDate means that the license does not expire.
    [[ -z "$stop" ]] && return 0
    stop_s="$(date -u -d "$stop" +%s 2>/dev/null)" || return 1
    (( stop_s - $(date -u +%s) > LICENSE_MIN_REMAINING_S ))
}

# print_activation_errors LOG SERIAL
# Prints the last error lines of the editor LOG to stderr. Replaces the email,
# password, and SERIAL with placeholders first.
print_activation_errors() {
    local line serial="$2"
    grep -iE 'error|fail|timed out|denied|invalid' "$1" 2>/dev/null | tail -n 15 |
        while IFS= read -r line; do
            line="${line//"$UNITY_PASSWORD"/<password>}"
            line="${line//"$UNITY_EMAIL"/<email>}"
            line="${line//"$serial"/<serial>}"
            printf '  %s\n' "$line" >&2
        done || true
}

# activate_license
# Activates a license for this machine with the Unity ID login and the serial.
# The licensing client then writes a new Unity_lic.ulf for this machine.
# Returns 1 if the activation fails.
activate_license() {
    local serial log
    if ! serial="$(license_serial)"; then
        echo "error: no valid serial. Set UNITY_SERIAL, or set UNITY_LICENSE to a Unity_lic.ulf file." >&2
        return 1
    fi
    log="$(umask 077 && mktemp)"

    # Start from no license file, as on a new CI machine. A file from another
    # machine makes the licensing client fail with "TimeStamp validation failed".
    # Keep the old file, and put it back if the activation fails.
    mkdir -p "$LICENSE_DIR"
    rm -f "$ACTIVATED_MARKER"
    if [[ -f "$LICENSE_FILE" ]]; then
        mv "$LICENSE_FILE" "$LICENSE_FILE.previous"
    fi

    echo "Activating a Unity license for this machine..."
    # The editor does not open a project for this command. The values are not
    # written to the editor log or to the licensing client log.
    if "$UNITY_EXE" -batchmode -nographics -quit -logFile "$log" \
        -serial "$serial" -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD"; then
        rm -f "$log" "$LICENSE_FILE.previous"
    else
        echo "error: the Unity license activation failed. Log extract:" >&2
        print_activation_errors "$log" "$serial"
        rm -f "$log"
        if [[ -f "$LICENSE_FILE.previous" && ! -f "$LICENSE_FILE" ]]; then
            mv "$LICENSE_FILE.previous" "$LICENSE_FILE"
        fi
        return 1
    fi

    if [[ -f "$LICENSE_FILE" ]]; then
        date -u +%Y-%m-%dT%H:%M:%SZ > "$ACTIVATED_MARKER"
        echo "Activated a Unity license for this machine ($LICENSE_FILE)."
    else
        # Not verified: the licensing client can keep the license as an
        # entitlement in ~/.config/unity3d/Unity/licenses/ and write no file.
        # Without the marker, the next run activates again.
        echo "Activated a Unity license for this machine." \
            "There is no $LICENSE_FILE, so the next run activates again."
    fi
}

# copy_license
# Writes $UNITY_LICENSE to the path where the Linux editor reads it.
copy_license() {
    mkdir -p "$LICENSE_DIR"
    rm -f "$ACTIVATED_MARKER"
    if ! (umask 077 && license_xml > "$LICENSE_FILE"); then
        echo "error: UNITY_LICENSE is not XML or valid base64." >&2
        return 1
    fi
    echo "Wrote the Unity license to $LICENSE_FILE."
    echo "warning: UNITY_EMAIL and UNITY_PASSWORD are not set, so the license was not" \
        "activated for this machine. A file from another machine does not work here." >&2
}

# install_license
# Activates a license when the login is set, or copies $UNITY_LICENSE.
# Returns 1 if the activation or the copy fails.
install_license() {
    if [[ -n "${UNITY_EMAIL:-}" && -n "${UNITY_PASSWORD:-}" ]]; then
        if license_is_current; then
            echo "This machine already has a current Unity license ($LICENSE_FILE)."
            return 0
        fi
        activate_license
    elif [[ -n "${UNITY_LICENSE:-}" ]]; then
        copy_license
    else
        echo "warning: no Unity license is configured (UNITY_EMAIL and UNITY_PASSWORD)." \
            "The editor cannot run tests without a license." >&2
    fi
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

install_license || exit 2
