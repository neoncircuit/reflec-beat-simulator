# Task: Review the Dependabot PRs for GitHub Actions

- **Sprint:** sprint-002
- **Milestone:** M8 (maintenance)
- **Status:** Done (2026-11-04)

## Goal

Decide which of the three open Dependabot PRs to merge. They bump the
actions in `.github/workflows/`. The current versions of two actions show
deprecation warnings in CI.

- PR #1: `actions/checkout` 4 → 7
- PR #2: `github/codeql-action` 3 → 4
- PR #3: `github/super-linter` 6 → 7

## Done when

- [x] Each PR has a review result and a recommendation in `notes.md`
  (2026-11-04)
- [x] PR #2 is merged (2026-11-04)
- [x] PR #1 is merged (2026-11-04)
- [x] PR #3 is merged (2026-11-04)
- [x] After the merges, "Code quality" and the "Static security analysis"
  job pass on `main`. The "Secret scan" job on `main` is the task
  `trufflehog-push-fix`.

## Steps

- [x] Read the diff, the release notes, and the CI runs of each PR
- [x] Merge the three branches into the current `main` in a scratch
  worktree. Result: no conflicts, and the workflow files parse as YAML.
- [x] Find why "Security scan" fails on each push to `main`. Result: the
  cause is in `security.yml`, not in these PRs. The fix is the sprint-002
  task `trufflehog-push-fix`.
- [x] Merge PR #2, then PR #1, then PR #3 (owner request, rebase merge,
  2026-11-04)
- [x] Check the CI runs on `main` after the merges (see `notes.md`)

## Blockers

None.
