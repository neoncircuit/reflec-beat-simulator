# Notes: EditMode tests in cloud sessions

- Cloud container: Ubuntu 24.04, x86_64, root, about 30 GB free disk.
- Ubuntu 24.04 uses `t64` package names (`libasound2t64`, `libgtk-3-0t64`).
  The setup script tries both names.
- Under `set -o pipefail`, `cmd | grep -q` can fail when the match is found,
  because `grep -q` stops early and breaks the pipe. Use `grep ... > /dev/null`.
- The default hook timeout is 60 s. The hook uses 1800 s for the first
  download.
- Verified on 2026-11-04: the setup script downloads and installs the editor
  in a cloud session.

## License

- A `Unity_lic.ulf` file from Unity Hub does not work on another machine. The
  licensing client (1.16.2) compares the machine bindings in the file with the
  current machine and fails with "Legacy MachineBinding validation failed" and
  "TimeStamp validation failed". A new file from Unity Hub has the same
  problem. Unity does not do manual (offline) activation of Personal licenses.
- A Personal `.ulf` file also expires after some days (`StopDate`). The
  licensing client must refresh it online.
- The working method is the GameCI method: read the serial from the
  `DeveloperData` field of the `.ulf` file (base64, skip the first 4 bytes),
  then activate on the new machine:
  `Unity -batchmode -nographics -quit -serial S -username E -password P`.
  The extracted serial agrees with `SerialMasked` in the owner's file.
- Probe with false values (2026-11-04):
  - The editor does not open or make a project for this command, so
    `-projectPath` is not necessary.
  - The email, password, and serial are not in the editor log or in
    `~/.config/unity3d/Unity/Unity.Licensing.Client.log`.
  - The editor exits with code 1 when the activation fails.
  - The login goes to `https://core.cloud.unity3d.com/api/login`. The proxy
    blocks it, and the editor reports "Request timed out".
- The licensing client also uses `activation.unity3d.com` and `api.unity.com`.
  `public-cdn.cloud.unity3d.com` (remote config) and `cdp.cloud.unity3d.com`
  (analytics) are blocked too, but their failures are only warnings.
- The password is in the editor command line while the activation runs, so
  other processes in the container can see it. GameCI has the same risk.
- Decision (owner, 2026-11-04): a separate Unity ID without two-factor
  login, used only for cloud sessions. The main Unity ID keeps two-factor
  login. Command-line activation cannot ask for a two-factor code.
- Real activation (2026-11-04, SessionStart hook at 13:17 UTC): it worked.
  - The hosts that the activation needs are `license.unity3d.com`,
    `core.cloud.unity3d.com`, `activation.unity3d.com`, and `api.unity.com`.
    `public-cdn.cloud.unity3d.com` stays blocked; that is only a warning.
  - The licensing client got an access token, then logged "ULF license
    activated successfully". The new `Unity_lic.ulf` has machine bindings
    1 and 2 equal to `/etc/machine-id`, and an empty `StopDate`.
  - The serial in the new file is not the serial that the script sent. The
    server sets the serial. `UNITY_LICENSE` still held the old file of the
    main Unity ID, and the activation worked. The script needs only a
    serial that has the correct format.
  - The licensing client writes the license directory and file with mode
    777. The file holds no password.
- Package downloads (2026-11-04): `download.packages.unity.com` answers a
  package `.tgz` request with HTTP 302 to `cdn.packages.unity.com`. The
  proxy blocks that host, and the Package Manager logs "Access to ... was
  denied" (see `~/.config/unity3d/upm.log`). The packages that fail are the
  ones that do not ship with the editor: `com.unity.inputsystem`,
  `com.unity.textmeshpro`, and `com.unity.package-manager.metadata-2022.3`.
- Test run (2026-11-04, after `cdn.packages.unity.com` was allowed):
  `scripts/run-editmode-tests.sh` passed, 129 of 129 tests in 13 fixtures,
  no compiler errors or warnings. The run took 60 s, with the package
  install and the import of the project. A run before that had made a part
  of `Library/`, so the time for a new container can be longer.
- Hosts that a cloud session needs, all checked on 2026-11-04:
  `download.unity3d.com`, `packages.unity.com`,
  `download.packages.unity.com`, `cdn.packages.unity.com`,
  `license.unity3d.com`, `core.cloud.unity3d.com`, `activation.unity3d.com`,
  `api.unity.com`.
- Not verified:
  - Activation limits. Each new container is a new machine, and the script
    does not return the license (`-returnlicense`). If Unity limits the
    number of activations, old activations must be removed in the Unity ID
    account.
