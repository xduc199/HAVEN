# HAVEN — Admin Account Recovery & UI Report

**Kết luận: PARTIALLY COMPLETED** — source fixes and automated MVC/Identity tests pass; live-database Admin E2E and visual responsive QA remain blocked.

## Git and safety

- Branch: `develop`; HEAD before and after: `5fdad8cc5c3c53942ba18afd83b22260d669cf54`.
- Initial state: 10 modified tracked files from previous work; no staged changes; untracked `docs/qa/` reports and `src/Haven.Web/wwwroot/css/haven-tokens.css`.
- The pre-existing dirty files were backed up before edits to `%TEMP%\haven-admin-recovery-full-backup-20261119-01`, with SHA-256 entries in `SHA256SUMS.txt`. A smaller backup of the two dirty Admin presentation files is also at `%TEMP%\haven-admin-recovery-backup-20261119-01`.
- Existing edits were preserved. No reset, clean, checkout, commit, or push was run. HEAD did not change.
- Final state: 15 modified tracked files, none staged. The original 10 dirty tracked files remain; five additional tracked files were changed in this turn (`UsersController.cs`, `Views/Users/Index.cshtml`, `AdminUserViewModels.cs`, `admin.js`, `MvcWorkflowTests.cs`). The existing untracked `haven-tokens.css` remains; `docs/qa/` also contains this report.

## Lock and unlock investigation

### Source trace

1. `Areas/Admin/Views/Users/Index.cshtml` posts `id` and `locked` to `POST /Admin/Users/SetLockout`, includes an antiforgery token, and renders TempData feedback on redirect.
2. `Areas/Admin/Controllers/UsersController.cs` is protected by `[Authorize(Policy = "AdminOnly")]`; `AdminOnly` requires the `Admin` role. The action uses the configured `UserManager<HavenUser>` backed by EF Identity.
3. Admin lock uses `LockoutEnabled=true`, `LockoutEnd=DateTimeOffset.MaxValue`, then updates the security stamp. Login uses `SignInManager.PasswordSignInAsync(... lockoutOnFailure: true)`. The configured security-stamp interval is one minute, so an existing cookie is rejected at the next validation after a stamp change.
4. Identity lockout columns are present in the existing migration/model snapshot. No new property, endpoint, migration, or API contract was needed.

The route, method, antiforgery token, Identity persistence calls, and basic lock → blocked login → unlock → successful login path were already present and are covered by an existing integration test. I could not reproduce a broken base endpoint in the test host. The concrete source defects found were that the list treated every future `LockoutEnd` as an Admin lock, the unlock path cleared `AccessFailedCount` (thereby weakening an Identity failed-login lockout), and the POST redirect discarded the current search/page. Those behaviors could make Admin think a change had not applied and could let an Admin action clear a temporary Identity lockout.

### Corrections

- Admin and Identity temporary lockouts now have separate status labels. Temporary lockout displays its expiry and offers no Admin unlock button.
- Server-side `SetLockout` rejects both locking over an active temporary lockout and unlocking one. A forged POST cannot clear or replace that state.
- Admin unlock only clears the `DateTimeOffset.MaxValue` Admin sentinel. It no longer resets `AccessFailedCount` or disables the user's later Identity lockout policy.
- Repeated lock/unlock requests produce safe, explicit feedback. Missing `locked` input is rejected; an unknown ID returns 404. Self-lock and Admin-role protections remain.
- Lockout errors log Identity error codes without logging email, password, or other account values.
- Search and page are submitted with the action and preserved after redirect, keeping the changed row visible.
- Lockout forms include confirmation and a disabled/loading button state. The server remains authoritative if JavaScript is unavailable.

## Admin UI and views

Audited all existing Admin view paths: Dashboard, Listings index/details/reject, Users index, PropertyTypes index/create/edit, Tags index/create/edit, Consultations index/details, PrivacyRequests index, Reports, shared analytics, `_AdminLayout`, `_ViewStart`, and `_ViewImports`. There is no Admin-specific login view or user-detail view in the area.

The Admin layout loaded Haven tokens and `admin.css`, but several existing Admin views used `.haven-form`, `.haven-button`, `.content-card`, `.stats-grid`, `.form-actions`, and `.status-badge` without loading their component styles. Admin body typography also bypassed the shared font token. The Admin stylesheet now supplies Admin-scoped versions of these shared components and uses the Haven tokens; it does not load or alter the public/User stylesheet.

Additional changes:

- Clarified account status and action styling on Users.
- Added coherent heading, form, card, status, table, dashboard-stat, and validation treatments using the HAVEN palette.
- Added a mobile sidebar backdrop, click-away and Escape closing, and kept table overflow inside its wrapper.
- Added shared Vietnamese-capable font loading to the Admin layout and changed Admin text to the shared font variables.

Responsive rules were reviewed in source at 800px and 520px; tables keep their own horizontal scroll region. **No responsive viewport is marked PASS** because browser rendering was unavailable.

## Automated tests

- Build: **PASS**, `dotnet build HAVEN.slnx --no-restore`, 0 warnings, 0 errors. Build output was redirected to `%TEMP%\haven-admin-recovery-build` so the running app's output files were not touched.
- Unit tests: **PASS 74/74**.
- Integration tests: **PASS 76/76**. Includes the existing lock/login/cookie-revocation/unlock flow, Admin self/peer-Admin protections, plus new checks for a real Identity temporary lockout, unauthorized User POSTs to both actions, unknown ID handling, preserved search redirect, and unchanged temporary lockout/security stamp.
- Admin JavaScript syntax: **PASS**, `node --check`.
- `git diff --check`: **PASS**.
- No database records were changed by this investigation. Integration tests use an isolated in-memory database.

## Live Admin, database, and visual QA limits

- `Haven:AdminBootstrap:Email` and `Haven:AdminBootstrap:Password` were not configured in user secrets, environment variables, or `appsettings.json`. No Admin credentials were available in the inspected configuration. I did not create or elevate a user account.
- Consequently, lock/unlock against the local SQL database, real database persistence across an application restart, and the real Admin-only browser flow were **BLOCKED**. The DB was not modified.
- The Windows browser-control runtime failed to initialize (`failed to write kernel assets`). No browser console, network, screenshot, modal, or responsive visual check was performed. Public/User pages were not changed; automated regression suites passed, but User visual regression was not inspected.

## Files changed in this turn

- `src/Haven.Web/Areas/Admin/Controllers/UsersController.cs`
- `src/Haven.Web/Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- `src/Haven.Web/Areas/Admin/Views/Users/Index.cshtml`
- `src/Haven.Web/ViewModels/Admin/AdminUserViewModels.cs`
- `src/Haven.Web/wwwroot/css/admin.css`
- `src/Haven.Web/wwwroot/js/admin.js`
- `tests/Haven.IntegrationTests/MvcWorkflowTests.cs`
- `docs/qa/HAVEN_ADMIN_RECOVERY_REPORT.md`

Pre-existing dirty user-facing files and the other QA reports were left intact. No commit or push was created.

## Remaining acceptance work

Provide an authorized Admin sign-in for the local Development database, then verify the list, lock, blocked login, unlock, preserved login-failure lockout, database values after restart, and User-role rejection in the running browser. Render the Admin pages at desktop, tablet, and mobile sizes and inspect table scroll, sidebar, focus, confirmation, and loading feedback. The current source and automated tests do not establish those live acceptance checks.
