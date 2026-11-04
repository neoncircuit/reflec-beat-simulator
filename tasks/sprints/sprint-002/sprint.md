# Sprint 002

- **Milestone:** M8 — Device verification & build hardening (maintenance)
- **Started:** 2026-11-04
- **Closed:** 2026-11-04
- **Goal:** Keep the CI workflows current. Review the open Dependabot PRs
  for GitHub Actions.

## Done

- Review of the Dependabot PRs for GitHub Actions —
  `tasks/dependabot-actions-review/` (PRs #1, #2, #3). The three PRs were
  merged on 2026-11-04: `actions/checkout` v7, `github/codeql-action` v4,
  `github/super-linter` v7. All push runs on `main` pass. The CodeQL
  Action v3 and Node.js 20 deprecation warnings are gone.
- Fix of the "Secret scan" job on pushes to `main` —
  `tasks/trufflehog-push-fix/` (PR #8). The job failed on all five pushes
  to `main` before the fix. It passes now and scans the pushed commits.
  TruffleHog is pinned to `v3.99.8`.

## Not done

None.

## Lessons

- A green check does not show that a scan did work. Read the job log:
  TruffleHog logs the `base`, the `head`, and `commits_scanned`.
- Do not give TruffleHog `base` and `head` in a workflow that runs on a
  push to `main`. The comment in `.github/workflows/security.yml` records
  this.
- A failure that occurs only on a push to `main` cannot be tested on a
  pull request. Plan the final check for the run after the merge.
- Dependabot turns off automatic rebases on a PR that is open for more
  than 30 days. Review Dependabot PRs within 30 days.
- This repository merges PRs with the rebase method. `main` has no merge
  commits.
