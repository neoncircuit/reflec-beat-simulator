# Notes: Move Super-linter to the upstream project

Checked on 2026-11-04 with the upstream repository
`super-linter/super-linter` (tags, `CHANGELOG.md`,
`docs/upgrade-guide.md`, `README.md`, and `lib/` at each tag).

## Target version: v9, not v8

- The backlog item said v8. When it was written, the newest version known
  here was 8.5.0 (from the changelog in Dependabot PR #3).
- Now the upstream tags are `v8.7.0` (2026-06-19) and `v9.0.0`
  (2026-10-01). The floating tags `v8` and `v9` point to these releases.
- A move to v8 would put us one major version behind, and Dependabot would
  offer v9 at once. The v9 breaking changes do not touch our
  configuration (see below). Decision: move to v9.
- Pin: `super-linter/super-linter/slim@v9`. This is a floating major tag,
  as for `actions/checkout@v7` and `codeql-action@v4`. `slim/action.yml`
  at `v9.0.0` uses the image `ghcr.io/super-linter/super-linter:slim-v9.0.0`.

## Breaking changes from v7.1.0 to v9.0.0

None of them touch our configuration. We enable only the JSON, Markdown,
and YAML linters.

- v7.4.0: the editorconfig-checker configuration filename changed. We do
  not use it.
- v8.0.0: removed gherkin-lint, Kubeconform (added back in v8.2.0),
  JavaScript Standard, Pyink, Raku, tekton-lint, and TypeScript Standard.
  Removed some ESLint configurations and node packages. Moved to the ESLint
  flat configuration format.
- v8.4.0: removed Terrascan and `stylelint-config-sass-guidelines`.
- v9.0.0: removed NBQA. Changed the jscpd `.gitignore` handling and the
  stylelint configuration filename.

## `VALIDATE_*` variables

- The v9 README says: if some `VALIDATE_*` values are `true` and others are
  `false`, Super-linter stops with an error. The workflow had
  `VALIDATE_CSHARP: false` and three `true` values.
- This did not fail on v7.1.0, and it does not fail on v9.0.0, because of
  the slim image. `UpdateLoopsForImage` removes `CSHARP` (and the other
  .NET, PowerShell, ARM, and Rust languages) from `LANGUAGE_ARRAY` before
  `ValidateValidationVariables` runs. So `VALIDATE_CSHARP` is not read.
- On the full image, the same configuration would stop with an error. The
  move removes `VALIDATE_CSHARP: false`. C# is still not linted, because
  languages that are not set default to `false` when any value is `true`.
- `DEFAULT_BRANCH: main` stays. The v9 README says that it is not necessary
  on GitHub Actions, but it has the same value as the default.

## Linter versions (from the lockfiles at each tag)

| Linter | v7.1.0 | v9.0.0 |
| --- | --- | --- |
| markdownlint-cli (markdownlint) | 0.41.0 (0.34.0) | 0.49.1 (0.41.1) |
| yamllint | 1.35.1 | 1.38.0 |
| ESLint, eslint-plugin-jsonc (JSON) | 8.57.0, 2.16.0 | 9.39.4, 3.4.2 |

The YAML rules file (`TEMPLATES/.yaml-lint.yml`) is the same in both
versions.

## Local check on all tracked files

The linters were installed at the versions above, in a scratch directory.
They were run on all tracked files (29 Markdown, 4 YAML, 5 JSON), with the
repository configuration and the Super-linter templates. CI lints only the
changed files, so this check shows what later PRs can meet.

- Markdown, v7.1.0: 8 findings (MD040, MD031, MD012) in `DESIGN.md` and
  `docs/`. They are old. CI did not report them, because those files did
  not change.
- Markdown, v9.0.0: the same 8 findings, plus 78 MD060
  (`table-column-style`) findings in 7 files. MD060 is new in markdownlint
  0.39.0. Many tables use `|---|---|` delimiter rows with padded cells, and
  MD060 does not accept the mix.
- Decision: turn off MD060 in `.github/linters/.markdown-lint.yml`. With
  MD060 off, v9.0.0 gives the same 8 findings as v7.1.0. A clean-up of the
  tables and the 8 old findings is in `todo.md`.
- YAML: the same result on both versions. Three `document-start` warnings,
  no errors.
- JSON: no findings on both versions.
- The Super-linter image itself was not run locally (no Docker). The real
  check is the CI run.
