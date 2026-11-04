# Task: EditMode tests in cloud sessions

- **Sprint:** sprint-001
- **Milestone:** M8
- **Status:** Blocked

## Goal

Claude Code cloud sessions install the Linux Unity editor and run
`scripts/run-editmode-tests.sh`. Then agents can run tests before they say
that work is done (`AGENTS.md` rule 5).

## Done when

- [x] PR #4 (Linux editor support in the test and build scripts) is merged
- [x] PR #5 (setup script and SessionStart hook) is merged
- [x] A new cloud session installs the editor (2026-11-04)
- [ ] A new cloud session activates a license for its machine
- [ ] `scripts/run-editmode-tests.sh` passes in a cloud session

## Steps

- [x] Write `scripts/setup-unity-linux.sh` and `.claude/hooks/session-start.sh`
- [x] Owner: allow the hosts in the cloud environment network settings:
  `download.unity3d.com`, `packages.unity.com`,
  `download.packages.unity.com`, `license.unity3d.com` (checked 2026-11-04)
- [x] Owner: add `UNITY_LICENSE` as an environment variable (2026-11-04).
  The copied file does not work: it is bound to the owner's machine.
- [x] Merge PR #4 (merged 2026-11-01)
- [x] Merge PR #5 (merged 2026-11-01)
- [x] Activate the license online in the setup script with the serial from
  `UNITY_LICENSE` and the Unity ID login (the GameCI method)
- [ ] Owner: allow these hosts too: `core.cloud.unity3d.com`,
  `activation.unity3d.com`, `api.unity.com`
- [ ] Owner: make a separate Unity ID for cloud sessions, without two-factor
  login. In Unity Hub, get a Personal license with it, and copy its
  `Unity_lic.ulf` into `UNITY_LICENSE` (the script reads the serial from it).
- [ ] Owner: add `UNITY_EMAIL` and `UNITY_PASSWORD` of that Unity ID as
  environment variables.
- [ ] Run the EditMode tests in a new cloud session

## Blockers

- Network policy: the Unity login and activation hosts are blocked
  (`core.cloud.unity3d.com`, `activation.unity3d.com`, `api.unity.com`;
  HTTP 403 from the proxy on 2026-11-04).
- No `UNITY_EMAIL` and `UNITY_PASSWORD` values in the cloud environment.
- The owner's Unity ID probably uses two-factor login (2026-11-04). The
  command-line activation cannot sign in to it.
