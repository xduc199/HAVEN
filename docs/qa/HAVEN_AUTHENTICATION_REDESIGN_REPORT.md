# HAVEN — Authentication redesign & live lockout investigation

**Kết luận: PARTIALLY COMPLETED.** Login/Register source redesign and automated build/tests are complete. Admin Lock/Unlock cannot be exercised safely without an authorized Admin session; local browser visual QA was denied by the browser origin permission gate.

## 1. Lock/Unlock — local investigation

### What was verified

- Branch is `develop`; HEAD remains `5fdad8cc5c3c53942ba18afd83b22260d669cf54`.
- The existing local app process (PID `26376`) is listening on `localhost:5104`. Its launch profile is `Development`.
- `appsettings.Development.json` resolves to local SQL Server `.\SQLEXPRESS`, database `HavenDb`. A read-only SQL connection succeeded; the database has 9 migration-history entries, 5 Identity users, 1 user assigned to the `Admin` role, all 5 users have `LockoutEnabled = 1`, and no user currently has a future `LockoutEnd`.
- No Admin bootstrap email/password is configured in User Secrets or environment variables. The existing `IdentityBootstrapper` returns without provisioning an Admin when both settings are absent. I did not change roles or database records.
- Source trace: the Users form posts `id` and `locked` to the existing antiforgery-protected `POST /Admin/Users/SetLockout`; the controller requires `AdminOnly`, uses `UserManager`, and Identity updates are persisted by its EF store. The view and shared Admin layout render TempData feedback. The Admin action does not reset `AccessFailedCount` when unlocking. The local schema contains the Identity lockout fields.
- Existing MVC integration tests exercise Admin lock, denied login, unlock, and temporary Identity lockout separation. They pass in the test host, but they are not proof of a successful local Admin request.

### Root cause and live status

The reported local failure is still **unconfirmed**. The available evidence rules out a missing database, absent Admin role membership, or globally disabled lockout support. It does not identify whether the selected user's action failed at authorization, antiforgery, the POST, Identity persistence, or UI refresh because no authorized Admin credentials/session are available. There is no evidence to justify another backend change, and no live lock/unlock request was sent.

**Live Lock/Unlock: BLOCKED.** No database records were changed. To continue this portion, provide access to an existing Development Admin session/credentials, or configure `Haven:AdminBootstrap:Email` and `Haven:AdminBootstrap:Password` through the project's approved local secret mechanism and sign in. Do not promote a regular account for this test.

### Browser limitation

The controlled browser rejected `http://localhost:5104/account/login` because a saved origin permission blocks local network access. The existing Chrome window was on ChatGPT; native input could not be used, and I did not switch to raw HTTP, CDP, headless automation, or another route around that policy. No live browser page, console, network log, or screenshot was inspected. Browser QA is **BLOCKED**.

## 2. Login/Register redesign

Both views now use a shared, scoped “A Place to Belong” split-screen treatment with the existing local `haven-hero.jpg` image. The login and registration messages have their requested Vietnamese copy and distinct eyebrows. The views use the existing Cormorant Garamond/Inter token stack and fallback fonts; browser font loading could not be visually verified.

- Login preserves `Email`, `Password`, `RememberMe`, `ReturnUrl`, antiforgery, MVC validation, and the existing API-enhanced submit path. No forgot-password link was added because no such route exists.
- Register preserves the existing `DisplayName`, `Email`, `Password`, `ConfirmPassword`, policy-version fields, required terms/privacy acknowledgements, validation, antiforgery, and existing API/MVC submit behavior. No extra personal fields or password-strength claims were added.
- Password visibility controls use a small page-specific script and accessible pressed state. Submit loading feedback follows the existing API form's `aria-busy` state.
- Auth-only CSS hides the global header/footer on these two screens and is scoped to `.auth-shell`; other User and Admin surfaces were not restyled. Desktop keeps the split layout; at `860px` and below, the image becomes a compact banner; at `520px` and below, spacing and type tighten for phones. Motion is subtle and disabled under `prefers-reduced-motion`.
- No screenshots are attached because browser access was blocked. Viewports 1920, 1440, 1366, 1024, 768, 430, 390, 375, and 360px remain **NOT VISUALLY VERIFIED**.

## 3. Tests

- `dotnet build HAVEN.slnx --no-restore`: **PASS**, 0 warnings, 0 errors. Output went to a temporary directory so the running app's compiled output was not replaced.
- Unit tests: **PASS 74/74** when run from the project's normal output path. A first run from a temporary path had 1 architecture-test failure because that test resolves repository files relative to its assembly; 73/74 passed in that attempt. The normal-path rerun passed all 74.
- Integration tests: **PASS 76/76** from the current build in the temporary output directory.
- `node --check src/Haven.Web/wwwroot/js/auth.js`: **PASS**.
- `git diff --check`: **PASS**. Git emitted only existing LF/CRLF conversion notices.
- No browser UI interaction tests or local database lockout mutations were performed.

## 4. Files changed in this turn

- Updated `src/Haven.Web/Views/Account/Login.cshtml` and `src/Haven.Web/Views/Account/Register.cshtml`.
- Added `src/Haven.Web/wwwroot/css/auth.css` and `src/Haven.Web/wwwroot/js/auth.js`.
- Added this report.
- No controller, API, database, Identity configuration, route, Home, User dashboard, or Admin source was changed in this turn. Earlier dirty changes remain in the working tree.

## 5. Git and remaining work

- Branch/HEAD stayed unchanged. No reset, clean, checkout, commit, or push was run.
- Existing dirty files were preserved. Before editing the two already-dirty auth views, copies were saved under `%TEMP%\haven-auth-redesign-backup-20261119`.
- Remaining acceptance blockers: an authorized Admin session to test a real QA user through lock → rejected login → unlock → successful login → restart persistence, and browser origin permission for visual Login/Register/Admin inspection. Until those are completed, the overall status remains **PARTIALLY COMPLETED**.
