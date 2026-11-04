# Current sprint: sprint-001

- **Milestone:** M8 — Device verification & build hardening
- **Started:** 2026-10-31
- **Ends:** when the tasks below are done, or when the owner closes it
- **Goal:** Run the EditMode tests in Claude Code cloud sessions, and track
  work in the sprint task structure.

## Tasks

- [x] Sprint task structure — `active/tasks-sprint-structure/`
- [ ] EditMode tests in cloud sessions — `active/cloud-editmode-tests/`

## Constraints

- Do not change gameplay or timing code in this sprint.
- On-device checks stay in the backlog. They are a late gate (see
  `docs/device-verification.md`).

## Blockers

- The cloud network policy blocks the Unity login and activation hosts:
  `core.cloud.unity3d.com`, `activation.unity3d.com`, `api.unity.com`. The
  owner must allow them (see `active/cloud-editmode-tests/task.md`).
- The cloud environment has no `UNITY_EMAIL` and `UNITY_PASSWORD` values. A
  copied `UNITY_LICENSE` file alone does not work, because Unity binds it to
  the machine that made it.
- The owner's Unity ID uses two-factor login. The command-line activation
  cannot sign in to it. The owner will make a separate Unity ID without
  two-factor login for cloud sessions.
