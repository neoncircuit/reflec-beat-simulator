# Notes: EditMode tests in cloud sessions

- Cloud container: Ubuntu 24.04, x86_64, root, about 30 GB free disk.
- Ubuntu 24.04 uses `t64` package names (`libasound2t64`, `libgtk-3-0t64`).
  The setup script tries both names.
- Under `set -o pipefail`, `cmd | grep -q` can fail when the match is found,
  because `grep -q` stops early and breaks the pipe. Use `grep ... > /dev/null`.
- The default hook timeout is 60 s. The hook uses 1800 s for the first
  download.
- Not verified: the editor archive name (the script reads it from Unity's
  release `.ini`), and a Personal `.ulf` license on a different machine.
  GameCI uses the same license method in CI.
