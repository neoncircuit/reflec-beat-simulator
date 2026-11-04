# Sprint 003

- **Milestone:** M8 — Device verification & build hardening (maintenance)
- **Started:** 2026-11-04
- **Closed:** 2026-11-04
- **Goal:** Move Super-linter from the `github/super-linter` fork to the
  upstream project, and clean up the Markdown lint findings.

## Done

- Move Super-linter to the upstream project —
  `tasks/super-linter-upstream/` (PR #10). The workflow uses
  `super-linter/super-linter/slim@v9` (image `slim-v9.0.0`). The lint
  results did not change. `VALIDATE_CSHARP: false` and the new PR summary
  comment were removed.
- Markdown clean-up and MD060 on — `tasks/markdown-cleanup/` (PR #11). The
  8 old findings are fixed, each table uses one style, and MD060 is on.
  All 31 tracked Markdown files pass markdownlint 0.49.1.

## Not done

None.

## Lessons

- A backlog item can be out of date when you start it. The item said v8,
  but upstream had v9. Read the upstream tags first.
- Before a linter version move, run the old and the new linter versions on
  all tracked files. CI lints only changed files, so new rules (here MD060)
  fail only a later PR that changes an old file.
- A job that passes can still log errors. Read the log: v9 logged
  `[ERROR]` for the PR summary comment, and the job passed.
- The slim image hid a configuration error: it removes `CSHARP` before the
  check for mixed `true` and `false` `VALIDATE_*` values.
