# HAVEN GitHub Actions CI Fix Report

## Summary

- Reported PR/commit: PR #34, `8ca4f0abd1146e5be9212af4e59dc29576f2d04b`.
- Local branch and HEAD at start: `develop`, `8ca4f0abd1146e5be9212af4e59dc29576f2d04b`.
- Root cause: `LegacyAndInformationRoutesResolve` asserted the obsolete Home copy `Không chỉ tìm một bất động sản.`. The current Home renders the editorial heading in the `manifesto-title` section instead.
- Fix: replaced only that brittle marketing-copy assertion with an assertion that the stable `manifesto-title` heading is rendered.
- CI workflow unchanged.

## Evidence and scope

The current Home view (`src/Haven.Web/Views/Home/Index.cshtml`) contains the editorial section with `id="manifesto-title"` and the current heading:

> Một nơi chốn không chỉ được chọn bằng những con số. Mà bằng cách nó khiến bạn cảm thấy.

The failing test was run before the edit and failed specifically because the old substring was absent. The remaining route, redirect, search-field, listing-state, CTA, privacy-link, search-route, and compare-route assertions in the method were retained.

Changed files:

- `tests/Haven.IntegrationTests/MvcWorkflowTests.cs` — updated one Home assertion.
- `docs/qa/HAVEN_GITHUB_ACTIONS_CI_FIX_REPORT.md` — this report.

## CI workflow review

`.github/workflows/ci.yml`:

- Triggers on pushes and pull requests targeting `main` or `develop`.
- Uses `actions/setup-dotnet@v4` with `.NET 10.0.x`.
- Restores, builds, and tests `HAVEN.slnx` from the repository root.
- Runs the complete solution test suite, which includes both `Haven.UnitTests` and `Haven.IntegrationTests`.

The workflow's solution and project paths are valid. No working-directory or test-discovery issue was found, so the pipeline was not changed. The Node.js 20 deprecation warning mentioned in the supplied Actions log is a runner/action-runtime warning, not the cause of this assertion failure; it is recorded separately and was not altered without evidence that it affects this job.

## Local verification

| Check | Result |
|---|---|
| Targeted pre-fix reproduction | Failed as reported: old Home substring absent |
| Integration tests, Release | Passed: 81/81 |
| `dotnet restore HAVEN.slnx` | Passed; dependencies already restored |
| `dotnet build HAVEN.slnx --configuration Release --no-restore` | Passed; 0 warnings, 0 errors |
| `dotnet test HAVEN.slnx --configuration Release --no-build` | Passed: Unit 82/82, Integration 81/81, total 163/163 |
| `git diff --check` | Passed |

## Remaining items

Local tests now pass. GitHub Actions has not been rerun on a new commit, and no commit was created, so CI on a new revision is not claimed as passed. The Node.js 20 warning remains a maintenance notice for the action runtime and is independent of the fixed test.

No application behavior or Home copy was changed. No database operation was performed. No commit or push was made.

