# Task: EditMode tests in cloud sessions

- **Sprint:** sprint-001
- **Milestone:** M8
- **Status:** Done

## Goal

Claude Code cloud sessions install the Linux Unity editor and run
`scripts/run-editmode-tests.sh`. Then agents can run tests before they say
that work is done (`AGENTS.md` rule 5).

## Done when

- [x] PR #4 (Linux editor support in the test and build scripts) is merged
- [x] PR #5 (setup script and SessionStart hook) is merged
- [x] A new cloud session installs the editor (2026-11-04)
- [x] A new cloud session activates a license for its machine (2026-11-04)
- [x] `scripts/run-editmode-tests.sh` passes in a cloud session (2026-11-04:
  129 of 129 tests pass)

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
- [x] Owner: allow these hosts too: `core.cloud.unity3d.com`,
  `activation.unity3d.com`, `api.unity.com` (checked 2026-11-04)
- [x] Owner: make a separate Unity ID for cloud sessions, without two-factor
  login (owner decision, 2026-11-04). Use a password that is not used
  anywhere else.
- [x] Owner: in Unity Hub, get a Personal license with that Unity ID, and
  copy it into `UNITY_LICENSE`. Not necessary: the activation worked with
  the old file, because the server sets the serial (see `notes.md`).
- [x] Owner: add `UNITY_EMAIL` and `UNITY_PASSWORD` of that Unity ID as
  environment variables (2026-11-04)
- [x] Activate the license in a cloud session (SessionStart hook, 2026-11-04)
- [x] Owner: allow `cdn.packages.unity.com`. `download.packages.unity.com`
  redirects package downloads to it. (checked 2026-11-04)
- [x] Run the EditMode tests in a cloud session (2026-11-04)

## Blockers

None.
