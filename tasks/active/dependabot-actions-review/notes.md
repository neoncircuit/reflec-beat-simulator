# Notes: Review the Dependabot PRs for GitHub Actions

Review date: 2026-11-04. Dependabot opened the PRs on 2026-09-09, on base
`77a70fa`. The files in `.github/` did not change on `main` after that
commit, so the PR CI results from 2026-09-09 are still valid.

## Summary

| PR | Change | CI on the PR | Recommendation |
| --- | --- | --- | --- |
| #2 | `codeql-action/upload-sarif` v3 → v4 (1 line) | Pass | Merge first. v3 is deprecated in December 2026. |
| #1 | `actions/checkout` v4 → v7 (3 lines) | Pass | Merge. v4 runs on Node.js 20, which is deprecated. |
| #3 | `super-linter/slim` v6 → v7 (1 line) | Pass | Merge. Then look at upstream v8 (backlog). |

## PR #2: `github/codeql-action` 3 → 4

- The diff changes one line in `security.yml`:
  `github/codeql-action/upload-sarif@v3` → `@v4`.
- CI on `main` (run 41857978439, 2026-11-04) shows two warnings:
  "CodeQL Action v3 will be deprecated in December 2026", and
  `upload-sarif@v3` targets Node.js 20.
- CI on the PR (run 34307869823): `upload-sarif@v4` validated, uploaded,
  and processed `devskim-results.sarif`. The step ran. It did not skip.
- The workflow uses only the `sarif_file` input. v4 accepted it.

## PR #1: `actions/checkout` 4 → 7

- The diff changes three lines: one checkout step in `code-quality.yml`,
  and two in `security.yml`.
- CI on `main` warns that `actions/checkout@v4` targets Node.js 20. The
  runner forces Node.js 24.
- Changes from v4 to v7 (from the release notes in the PR):
  - v5.0.0: runs on Node.js 24.
  - v6.0.0: keeps the git credentials in a separate file.
  - v7.0.0 (also v6.1.0): does not check out fork PR code for
    `pull_request_target` and `workflow_run` events. Our workflows use
    `push`, `pull_request`, and `workflow_dispatch` only. This change has
    no effect on them.
  - v7.0.1: bug fixes. The `v7` tag moved to v7.0.1 after the PR CI ran.
- CI on the PR passed for both workflows (runs 34307805992 and
  34307805996). Super-linter and TruffleHog read the git history with
  `fetch-depth: 0` as before.

## PR #3: `github/super-linter` 6 → 7

- The diff changes one line in `code-quality.yml`:
  `github/super-linter/slim@v6` → `@v7`.
- CI on the PR (run 34307875472) pulled the image
  `ghcr.io/super-linter/super-linter:slim-v7.1.0`. It linted the changed
  YAML file and passed. It gave one yamllint warning (`document-start`),
  not an error.
- The workflow sets `VALIDATE_ALL_CODEBASE: false`. Super-linter lints
  only the files that a push or PR changes. New v7 rules show only on
  files that a later PR changes.
- The v7 release notes call `github/super-linter` a fork. Its `v7` tag
  uses image 7.1.0 (2024). The upstream project
  `super-linter/super-linter` is at v8.5.0 (2026-02-06), from the
  changelog in the same PR. Dependabot follows the tags of
  `github/super-linter`. It offers v8 only if that fork adds a v8 tag.
- Decision: merge PR #3, because it is green and low risk. A move to the
  upstream v8 is a separate change. It is in `todo.md`.

## Merge check

- On 2026-11-04, the three branches were merged into `origin/main`
  (`6ec8141`) in a scratch worktree, in the order #2, #1, #3. There were
  no conflicts. Both workflow files parse as YAML.
- There is no conflict, so Dependabot does not have to rebase the PRs.
  To get new CI runs before a merge, comment `@dependabot rebase` on the
  PR.

## "Security scan" fails on each push to `main`

This failure is not caused by the Dependabot PRs.

- The "Secret scan" job failed on all five pushes to `main` (runs 1, 8,
  10, 12, 14). It passes on pull requests.
- Cause (checked in run 41857978439): `security.yml` gives TruffleHog
  `base: main` and `head: HEAD`. On a push to `main`, both are the same
  commit. The action stops with "BASE and HEAD commits are the same".
- When the `base` and `head` inputs are empty, the action gets the commit
  range from the event. For a push, it scans from the `before` commit to
  the `after` commit.
- Fix: remove the `base` and `head` inputs. Also pin
  `trufflesecurity/trufflehog` to a release tag. `@main` is a moving
  branch, and Dependabot does not update it. This fix is in `todo.md`.
- You can verify the fix only with a push to `main`, because the failure
  does not occur on pull requests.
