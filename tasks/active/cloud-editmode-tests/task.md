# Task: EditMode tests in cloud sessions

- **Sprint:** sprint-001
- **Milestone:** M8
- **Status:** Blocked

## Goal

Claude Code cloud sessions install the Linux Unity editor and run
`scripts/run-editmode-tests.sh`. Then agents can run tests before they say
that work is done (`AGENTS.md` rule 5).

## Done when

- [ ] PR #4 (Linux editor support in the test and build scripts) is merged
- [ ] PR #5 (setup script and SessionStart hook) is merged
- [ ] A new cloud session installs the editor and the license
- [ ] `scripts/run-editmode-tests.sh` passes in a cloud session

## Steps

- [x] Write `scripts/setup-unity-linux.sh` and `.claude/hooks/session-start.sh`
- [ ] Owner: allow the hosts in the cloud environment network settings:
  `download.unity3d.com`, `packages.unity.com`,
  `download.packages.unity.com`, `license.unity3d.com`
- [ ] Owner: add `UNITY_LICENSE` (contents of `Unity_lic.ulf`, raw or base64)
  as an environment variable
- [ ] Merge PR #4, then PR #5
- [ ] Run the EditMode tests in a new cloud session

## Blockers

- Network policy: the Unity hosts are blocked (HTTP 403 from the proxy,
  checked again on 2026-10-31).
- No `UNITY_LICENSE` value in the cloud environment.
