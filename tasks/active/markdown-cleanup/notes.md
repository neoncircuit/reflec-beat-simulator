# Notes: Markdown clean-up and MD060 on

Done on 2026-11-04 with markdownlint-cli 0.49.1 (the Super-linter v9
version) and 0.41.0 (the v7.1.0 version), with
`.github/linters/.markdown-lint.yml`.

## Old findings (8)

The sprint-003 Super-linter task found these. CI did not report them,
because CI lints only changed files.

- MD040 (no language on a fenced block), 5 blocks: `DESIGN.md` (folder
  tree), `docs/architecture.md` (class tree), `docs/authoring.md` (two
  folder layouts), `docs/preliminary-report.md` (a formula). All got
  `text`.
- MD031 (no blank lines around a fenced block), 2 findings: the `csharp`
  block in list item 2 of `DESIGN.md` §4. Added a blank line before and
  after it. The list item and the code stay the same.
- MD012 (two blank lines), 1 finding: `docs/architecture.md`, before
  "Data flow". Removed one blank line.

## Tables (MD060)

- MD060 (new in markdownlint 0.39.0) gave 78 findings in 6 files. Most
  tables had `|---|---|` delimiter rows with padded cells (`| a | b |`).
- `markdownlint --fix` changed 14 delimiter rows to the "compact" style, for
  example `|---|---|` to `| --- | --- |`. A script compared each changed
  line with the old line, without spaces and with each run of dashes as
  one dash. All 14 lines are the same in that form, so no cell text
  changed.
- One table in `docs/preliminary-report.md` ("Project | Role") was left.
  Its header and delimiter row were aligned, and its body cells were
  compact. The delimiter row was changed by hand to `| ------- | ---- |`.
- MD060 is on again with its default style `any`. Each table must use one
  style, but different tables can use different styles.

## Result

- markdownlint 0.49.1 and 0.41.0: no findings in all 31 tracked Markdown
  files.
- `git diff -w` shows only the changes above. The text of the docs did not
  change.
