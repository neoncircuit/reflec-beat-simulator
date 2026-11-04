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

- The cloud network policy blocks `cdn.packages.unity.com`. The Package
  Manager downloads packages from it, so the editor cannot open the project.
  The owner must allow it (see `active/cloud-editmode-tests/task.md`).
