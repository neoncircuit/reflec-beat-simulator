# Task: Markdown clean-up and MD060 on

- **Sprint:** sprint-003
- **Milestone:** M8 (maintenance)
- **Status:** In progress. The change is on the branch. The last check is
  the CI run on the PR.

## Goal

Fix the old markdownlint findings in the docs, and use one table style in
each table. Then turn MD060 (`table-column-style`) on again. After this,
a PR that changes any Markdown file in the repository does not fail on old
findings.

## Done when

- [x] The 8 old findings (MD040, MD031, MD012) are fixed
- [x] Each table uses one style, and MD060 is on in
  `.github/linters/.markdown-lint.yml`
- [x] markdownlint 0.49.1 (Super-linter v9) and 0.41.0 find nothing in all
  31 tracked Markdown files
- [ ] "Code quality" passes on the PR with this change

## Steps

- [x] Fix MD040: give the fenced blocks a language (`text`)
- [x] Fix MD031 and MD012: blank lines around a fenced block in a list, and
  one blank line only
- [x] Remove `MD060: false`, run `markdownlint --fix`, and fix the last table
  by hand
- [x] Check that the table edits change only spaces and dashes
- [ ] Open a PR, and merge it when it is green (owner request, 2026-11-04)

## Blockers

None.
