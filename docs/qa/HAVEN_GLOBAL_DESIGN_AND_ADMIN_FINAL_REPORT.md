# HAVEN — Global Design and Admin Lockout Report

**Status: PARTIALLY COMPLETED**  
**Branch:** `develop`  
**HEAD:** `5fdad8cc5c3c53942ba18afd83b22260d669cf54` before and after this work  
**Scope:** shared visual refinement, Home and authentication presentation, source review of Admin lockout, and regression checks.

## Summary

This pass reduced repeated decorative labels and motion while preserving the existing Home copy, account fields, routes, authentication behavior, and Admin workflow. The shared token sheet now defines section spacing and control height; public and Admin button/card motion was made more restrained. This is a source-level refinement, not a visual acceptance: browser rendering was not available under the current local-origin restriction.

Admin lock and unlock were reviewed in source and the local Development database was queried read-only. The actual Admin POST and a login attempt for a locked account were not performed because no Admin credentials were available in the configured local User Secrets or environment, and no permitted live Admin browser session was available. The user-reported live failure therefore remains without a confirmed root cause.

## Design audit and changes

### Decorative details reduced

- Home no longer repeats a `HAVEN / 01` hero label, a separate image caption, numbered search/section eyebrows, category ordinals, category arrows, decorative CTA watermark, or arrows on the search and empty-state links.
- Forced line breaks were removed from Home headings so wrapping follows available width. Existing product copy, user-facing actions, property content, and workflow steps were preserved. Step numbers `01–03` remain in each ordered workflow because they communicate sequence.
- Login and Register retain one HAVEN mark on the image panel and their concise account-specific eyebrow. The duplicate image slogan/caption and `HAVEN / TÀI KHOẢN` label were removed. The page headings now wrap naturally; fields, validation, anti-forgery, ReturnUrl, RememberMe, policy acknowledgements, and routes remain in place.
- Removed the repeating image drift on authentication pages, removed staggered Home text entrance motion, and removed the property-card lift/zoom. One-time image reveal, menu/accordion transitions, focus states, and reduced-motion handling remain.
- Removed the duplicate tagline paragraph from the shared footer while keeping its descriptive copy and navigation links.

### Shared system refinement

- Added `--haven-section-space` and `--haven-control-height` to the shared token sheet.
- Aligned shared page-heading text with charcoal for contrast, normalized primary button text to charcoal on HAVEN gold, and reduced property-card hover to a border cue.
- Reused the shared control-height token for Admin and form buttons. Admin keeps its data-oriented workspace layout and table scrolling.
- Home section columns now use `minmax(0, ...)` where they need to shrink; the Home CTA uses the cream surface with a single gold action.

### Files changed in this pass

- `src/Haven.Web/Views/Home/Index.cshtml`
- `src/Haven.Web/Views/Account/Login.cshtml`
- `src/Haven.Web/Views/Account/Register.cshtml`
- `src/Haven.Web/Views/Shared/_Layout.cshtml`
- `src/Haven.Web/wwwroot/css/haven-tokens.css`
- `src/Haven.Web/wwwroot/css/frontend.css`
- `src/Haven.Web/wwwroot/css/home.css`
- `src/Haven.Web/wwwroot/css/auth.css`
- `src/Haven.Web/wwwroot/css/admin.css`
- `docs/qa/HAVEN_GLOBAL_DESIGN_AND_ADMIN_FINAL_REPORT.md`

The other dirty source and test files shown in Git status predated this pass; they were preserved. No backend, API, route, schema, migration, or authentication logic was changed in this pass.

## Admin Lock/Unlock investigation

### Source trace

The current user-management form posts `id`, `locked`, search, page, and an antiforgery token to the existing `POST /Admin/Users/SetLockout` action. The controller is protected by `AdminOnly`, resolves the user through `UserManager`, rejects missing input, self-targeting and Admin targets, and handles Identity results before showing TempData feedback.

For an Admin lock, the code enables Identity lockout and sets `LockoutEnd` to `DateTimeOffset.MaxValue`. For an unlock, it clears that sentinel value. It does not clear `AccessFailedCount`; a temporary future lockout from failed sign-ins is treated separately and cannot be overwritten by the Admin operation. The security stamp is updated after a successful state change, so an existing cookie is revoked when the cookie security-stamp validation runs; it is not an immediate client-side logout guarantee.

These source checks describe the intended implementation. They do not establish that the browser POST reached the action or that the live database persisted either operation. No confirmed root cause for the reported live failure was found in this pass.

### Local database snapshot (read-only)

Queried `.\SQLEXPRESS`, database `HavenDb`, using a read-only aggregate query:

| Check | Observed |
|---|---:|
| Identity users | 5 |
| Users with `LockoutEnabled = 1` | 5 |
| Users with a future `LockoutEnd` at query time | 0 |
| Members of the `Admin` role | 1 |
| Database writes in this pass | 0 |

The prior QA snapshot recorded the same counts. This is not a before/after lockout test: no user was locked or unlocked, so persistence, login rejection, restart persistence, or post-unlock login remain **BLOCKED / NOT TESTED**.

### Live test status

| Case | Status | Evidence / blocker |
|---|---|---|
| Admin login and role claim | BLOCKED | No Admin credential was present in the configured User Secrets file or environment; no live authenticated session was available. |
| Admin opens User Management | BLOCKED | Requires the unavailable authenticated session. |
| Lock QA user and inspect Identity result/SQL | NOT TESTED | No POST was sent; no database mutation was made. |
| Locked-user login rejection | NOT TESTED | No QA user was locked. |
| Unlock and login again | NOT TESTED | No QA user was locked. |
| Ordinary-user authorization denial | Covered only by existing automated test suite/source policy; no live request this pass. |
| Antiforgery and nonexistent ID | Source/automated coverage only; not live-tested this pass. |

Admin credentials were not copied into source, logs, or this report. To finish live verification, make an authorized local Admin session available and identify a dedicated QA user that can safely be locked and unlocked.

## Browser and responsive QA

The local application was already running at `http://localhost:5104`; it was not stopped or restarted. The authorized browser tool could not access the local origin, and the prior task instructions prohibit using HTTP, CDP, headless browser, or another surface to bypass that restriction.

No new browser screenshots were captured. All responsive viewports remain **NOT TESTED** in this pass: Home 1920/1600/1440/1366/1280/1024/768/430/390/375/360 px; Login/Register at desktop, tablet, and mobile; and Admin at desktop, tablet, and mobile. Consequently, font loading, Vietnamese glyph rendering, layout, console/network errors, menu behavior, form interaction, and visual regression are not claimed as passing.

## Build and regression checks

| Check | Result |
|---|---|
| `dotnet build HAVEN.slnx --no-restore` (output redirected to a temporary folder so the running site was not disturbed) | PASS — 0 warnings, 0 errors |
| Unit tests | PASS — 74/74 |
| Integration tests | PASS — 76/76 |
| JavaScript syntax (`site.js`, `auth.js`, `admin.js`) | PASS — `node --check` |
| `git diff --check` | PASS |
| Live authentication / lockout | BLOCKED / NOT TESTED |
| Browser visual QA | BLOCKED / NOT TESTED |

The first unit-test invocation from the temporary output directory could not locate repository files used by an architecture test; rerunning from the repository’s normal test output passed 74/74. An initial integration run caught a Home heading assertion after a copy change; the prior copy was retained, and the final integration run passed 76/76.

## Git and data safety

- Branch remains `develop`; HEAD is unchanged.
- At entry: 15 modified tracked paths, no staged changes, and four untracked status entries (`docs/qa/`, `auth.css`, `auth.js`, `haven-tokens.css`). At exit, the same counts remain; this report was added inside the already-untracked `docs/qa/` directory.
- No staged changes were present before work; none were staged during this pass.
- The pre-existing working-tree edits and untracked QA/auth/token files were retained. A backup of dirty frontend/Admin files was created under the local temporary directory before editing.
- No reset, clean, checkout, commit, push, merge, or branch change was performed.
- The SQL query was read-only; no user or listing data was changed.
- The local web process was not stopped or restarted.

## Remaining work

1. Provide an authorized local Admin session and a dedicated QA user to execute lock, locked-login, unlock, post-unlock login, and persistence checks against `HavenDb`.
2. Allow the approved browser session to access the running local origin, then inspect the target pages at the required desktop/tablet/mobile widths, verify fonts and interactions, and save real screenshots.
3. If live behavior still fails, capture the POST response, Identity result codes, cookie/auth state, and before/after `LockoutEnd` for the QA user to establish the actual root cause.

**Conclusion: PARTIALLY COMPLETED.** Source-level design refinements and automated regressions pass. The required live Admin lockout proof and visual browser acceptance remain unverified.
