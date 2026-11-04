# Task: Move Super-linter to the upstream project

- **Sprint:** sprint-003
- **Milestone:** M8 (maintenance)
- **Status:** In progress. The change is on the branch. The final check is
  the CI run on a PR and on `main`.

## Goal

Replace `github/super-linter/slim@v7` (a fork that uses image 7.1.0 from
2024) with the upstream `super-linter/super-linter/slim`. Keep the lint
results the same as now.

## Done when

- [x] The v8 and v9 breaking changes are read, and their effect on our
  configuration is recorded in `notes.md`
- [x] `code-quality.yml` uses `super-linter/super-linter/slim@v9`
- [x] The local check with the v9 linter versions gives the same results as
  with the v7.1.0 versions (see `notes.md`)
- [ ] "Code quality" passes on a PR with this change, and the log shows the
  v9 image
- [ ] "Code quality" passes on `main` after the merge

## Steps

- [x] Read the upstream tags, the changelog, and the upgrade guide (v7.1.0
  to v9.0.0)
- [x] Check how v9 handles our `VALIDATE_*` variables
- [x] Compare markdownlint, yamllint, and the JSON lint (ESLint) of v7.1.0
  and v9.0.0 on all tracked files
- [x] Change the action to `super-linter/super-linter/slim@v9`, remove
  `VALIDATE_CSHARP: false`, and turn off MD060
- [ ] Open a PR, and check the "Code quality" log
- [ ] Merge, and check the run on `main`

## Blockers

None.
