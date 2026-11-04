# Task: Fix the "Secret scan" job on pushes to main

- **Sprint:** sprint-002
- **Milestone:** M8 (maintenance)
- **Status:** Done (2026-11-04)

## Goal

The "Secret scan" job in `.github/workflows/security.yml` fails on each
push to `main`. Make it scan the pushed commits and pass. Also pin the
TruffleHog action to a release, so that its code does not change without
a review.

## Done when

- [x] `security.yml` does not set the `base` and `head` inputs of TruffleHog
- [x] The TruffleHog action is pinned to a release tag (`v3.99.8`)
- [x] "Secret scan" passes on a pull request with this change (PR #8,
  2026-11-04; see `notes.md`)
- [x] "Secret scan" passes on the push to `main` after the merge, and the
  log shows a scan of the pushed commits (run 41872298135, 2026-11-04)

## Steps

- [x] Find the cause in the CI log of run 41857978439 (see the notes of
  `dependabot-actions-review`)
- [x] Read `action.yml` at `v3.99.8`. It is the same as on `main`.
- [x] Reproduce the failing check with the old inputs, and check the
  event path with the new inputs (see `notes.md`)
- [x] Remove `base` and `head`, and pin the action to `v3.99.8`
- [x] Open PR #8 with this change (2026-11-04)
- [x] Merge PR #8 (owner request, rebase merge, 2026-11-04)
- [x] Check the "Secret scan" log on `main` after the merge. Result: a
  `base` and a `head` that are not the same, 3 commits scanned, and
  success (see `notes.md`).

## Blockers

None.
