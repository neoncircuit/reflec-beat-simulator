# Notes: Fix the "Secret scan" job on pushes to main

## Cause

- The cause is in the notes of `dependabot-actions-review`, section
  "Security scan fails on each push to `main`".
- Short form: when `base` or `head` is set, the action compares the two
  commits. If they are the same, it stops with an error. On a push to
  `main`, `base: main` and `head: HEAD` are the same commit. A manual run
  (`workflow_dispatch`) on `main` fails for the same cause.

## Action code at `v3.99.8`

- `v3.99.8` (2026-11-02) is the latest release tag. There is no floating
  `v3` tag. The pin must be a full version.
- `action.yml` at `v3.99.8` is the same as on `main` (checked
  2026-11-04). The pin does not change the behavior.
- Without `base` and `head`, the action selects the commits from the
  event:
  - `push`: from `github.event.before` to `github.event.after`. If there
    are no commits in the push, it exits with code 0.
  - `pull_request`: from the PR base SHA to the PR head SHA.
  - `workflow_dispatch`: the full history.
- The `version` input selects the Docker image tag of the scanner. Its
  default is `latest`. The pin covers the action script only. The scanner
  image still uses `latest`, so new detectors arrive without a PR.
- TruffleHog releases often. Expect a Dependabot PR for this action in
  most weeks.

## Check (2026-11-04)

- The base and head check of the action was run with `git` on commit
  `6ec8141`, with a local branch for `main`, as in the CI checkout:
  - Old inputs (`base: main`, `head: HEAD`): both resolve to `6ec8141`.
    This is the error of run 41857978439.
  - New inputs (not set): the action takes the event path. For that push,
    the range is `f909ae0..6ec8141` (2 commits).
- The TruffleHog scanner was not run locally. The session did not get
  permission to download and run the release binary, and Docker is not
  available. The real check is the CI run on `main` after the merge.
- PR #8, "Secret scan" run 41866264973 (head `f6303e4`): the action took
  the `pull_request` event path. TruffleHog 3.99.8 scanned from `6ec8141`
  (PR base) to `f6303e4` (PR head): 2 commits, 0 verified and 0
  unverified secrets. Result: success.
- `main`, "Security scan" run 41872298135 (push of `84af0d5`, the merge
  of PR #8): the "Secret scan" job took the `push` event path.
  TruffleHog 3.99.8 scanned from `6ec8141` (`before`) to `84af0d5`
  (`after`): 3 commits, 0 verified and 0 unverified secrets. Result:
  success. This is the first push to `main` on which the job passes.
  "Code quality" run 41872298441 on the same push passed too.
