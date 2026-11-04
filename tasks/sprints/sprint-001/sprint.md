# Sprint 001

- **Milestone:** M8 — Device verification & build hardening
- **Started:** 2026-10-31
- **Closed:** 2026-11-04
- **Goal:** Run the EditMode tests in Claude Code cloud sessions, and track
  work in the sprint task structure.

## Done

- Sprint task structure — `tasks/tasks-sprint-structure/` (PR #5). The
  legacy trackers are in `tasks/archive/`. Agents read
  `tasks/current_sprint.md`, and the backlog is `tasks/todo.md`.
- EditMode tests in cloud sessions — `tasks/cloud-editmode-tests/`
  (PRs #4, #5, #6). The SessionStart hook installs the Linux editor and
  activates a Unity license for the container. 129 of 129 EditMode tests
  pass in a cloud session (2026-11-04).

## Not done

None.

## Lessons

- A `Unity_lic.ulf` file from Unity Hub works only on the machine that made
  it. A cloud session or a CI machine must activate a license online with a
  Unity ID login. The setup is in `docs/build-and-sign.md`.
- Command-line activation cannot sign in to a Unity ID that uses two-factor
  login. Use a separate Unity ID without two-factor login for cloud sessions.
- Each blocked host showed only after the step before it worked: first the
  login hosts, then the package CDN. Before you ask the owner to change the
  network policy, test all the hosts of the flow, and follow redirects
  (`download.packages.unity.com` redirects to `cdn.packages.unity.com`).
  An HTTP 403 from the proxy means that the host is blocked. A status code
  from the server means that the host is allowed.
- A probe with false values is a good first check: it showed the editor
  behavior and that no secrets go into the logs. But only a real run found
  the last blocked host. Do not say that a setup works before a real run.
- A new cloud session checks out `main`. Changes to the SessionStart hook or
  the setup script have an effect in new sessions only after they are merged.
