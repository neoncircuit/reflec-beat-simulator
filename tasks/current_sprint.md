# Current sprint: none

- **Milestone:** M8 — Device verification & build hardening
- **Previous sprint:** sprint-003, closed 2026-11-04
  (`sprints/sprint-003/sprint.md`)
- **Next sprint:** sprint-004. It starts when the owner approves its tasks
  from `todo.md`.

## Tasks

None. Do not start work until the owner approves the sprint-004 tasks.

## Constraints

- On-device checks stay in the backlog until the owner accepts Editor
  playability. They are a late gate (see `docs/device-verification.md`).
  On 2026-11-04 the owner had not accepted Editor playability.

## Blockers

None.

## Handoff to a local session (2026-11-04)

The owner's cloud sessions stop on 2026-11-05 at 07:59 UTC. The work
continues in a local session.

- **State:** `main` has all the work of sprints 001 to 003. There are no
  open PRs.
- **Tests:** on 2026-11-04, 129 of 129 EditMode tests passed in a cloud
  session, on code that is the same as `main`. In a local session, run
  `./scripts/run-editmode-tests.sh`. Under WSL, the script uses the
  Windows editor (2022.3.62f3).
- **Cloud-only files:** `.claude/hooks/session-start.sh` and
  `scripts/setup-unity-linux.sh` run only when `CLAUDE_CODE_REMOTE` is
  `true`. A local session does not use them.
- **CI:** all workflows pass on `main`. The action versions are
  `actions/checkout@v7`, `codeql-action/upload-sarif@v4`,
  `super-linter/super-linter/slim@v9`, and `trufflehog@v3.99.8`.
- **Owner actions:**
  - Accept or reject Editor playability. This is the gate for the Galaxy
    Tab checks in `todo.md`.
  - Select the sprint-004 tasks from `todo.md`.
  - Delete these old branches on GitHub. Each commit on them has the same
    change on `main` (checked 2026-11-04): `claude/admiring-thompson-zbdtr6`,
    `claude/blissful-edison-72iuxb`, `claude/close-sprint-001`,
    `cursor/linux-unity-editor-71e6`,
    `dependabot/github_actions/github/codeql-action-4`. The cloud session
    branch `claude/dazzling-lamport-ahwxe8` can go after its last PR is
    merged.
