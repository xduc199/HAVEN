# HAVEN Admin Lockout — Root Cause Report

**Status: PARTIALLY COMPLETED**

## Summary

The lockout error was caused by the Razor view rendering the `locked` Boolean as the literal HTML attribute value `value`. MVC therefore could not bind it as `true` or `false`, and `SetLockout` returned its invalid-request message before calling ASP.NET Core Identity.

The view now emits explicit lowercase Boolean strings. A new integration test renders the actual MVC view, checks its form fields and antiforgery token, exercises malformed requests, and verifies both lock and unlock against the test Identity store. Browser Network capture and the running local SQL Server database were not verified in this run.

## Repository safety

- Branch: `develop`
- HEAD before and after: `5fdad8cc5c3c53942ba18afd83b22260d669cf54`
- The working tree was already dirty before this task: 16 tracked paths were modified and untracked QA/assets paths were present.
- Existing changes were preserved. Backups of the already-dirty test file and Admin user view were saved outside the repository under `%TEMP%\haven-admin-lockout-investigation\` before editing.
- No reset, clean, checkout, commit, push, merge, or database change was performed.

## Root cause and evidence

### Before the fix

The Admin Users Razor view used:

```razor
<input type="hidden" name="locked" value="@(!user.IsAdminLocked)" />
```

An integration test rendered this view through the MVC test host and observed the resulting element as:

```html
<input type="hidden" name="locked" value="value" />
```

The same rendered form contained the expected `id` field and an antiforgery token. This establishes that the form had the target ID, while the `locked` value was not a parseable Boolean. That explains why the action reached its invalid-request branch before `FindByIdAsync` or any Identity update.

The test host is direct evidence of Razor rendering, not a capture of the user’s Chrome Network request. The latter was not available to inspect.

### After the fix

The view now writes the value explicitly as a string:

```razor
<input type="hidden" name="locked" value="@(user.IsAdminLocked ? "false" : "true")" />
```

The new integration test verifies that the rendered value parses as `true` for an active user and `false` after locking. It posts both values to `/Admin/Users/SetLockout` and verifies `LockoutEnd` changes to `DateTimeOffset.MaxValue` and then back to `null` in the test Identity store.

## Request, routing, JavaScript, and security findings

- Route: `POST /Admin/Users/SetLockout` through the Admin area route.
- The route resolves to the single `SetLockout` action in `Areas/Admin/Controllers/UsersController.cs`.
- The action remains protected by `[Authorize(Policy = "AdminOnly")]` and `[ValidateAntiForgeryToken]`; MVC also applies the global antiforgery filter.
- The view emits a POST form, hidden `id`, hidden `locked`, search/page fields, and antiforgery token.
- `admin.js` only confirms submission, cancels when the user declines, and disables the submit button when confirmed. It does not mutate the fields or construct another payload.
- Identity lock/unlock behavior, self-lock protection, Admin protection, security stamp update, and temporary failed-login lockout handling were not changed in this task.
- No controller, routing, authorization, authentication, API, schema, migration, or database code was changed.

## Files changed in this task

- `src/Haven.Web/Areas/Admin/Views/Users/Index.cshtml` — serialize the hidden Boolean as literal `true` or `false`.
- `tests/Haven.IntegrationTests/MvcWorkflowTests.cs` — add a regression test for rendered form values, antiforgery field, missing/invalid parameters, and lock/unlock round trip. This test file was already modified before this task; its existing edits were preserved.
- `docs/qa/HAVEN_ADMIN_LOCKOUT_ROOT_CAUSE_REPORT.md` — this report.

## Test results

- Targeted Admin lockout integration tests: **3 passed, 0 failed**.
- Full solution build: **passed, 0 warnings, 0 errors**, using a temporary output directory because the local app process held the normal `Haven.Web.exe` and `.dll` output files open.
- Full integration suite: **77 passed, 0 failed**.
- Unit suite from the normal project output: **74 passed, 0 failed**.
- An initial full test invocation from the temporary output directory had **73 unit tests pass and 1 architecture test fail** because that test locates repository source relative to its output directory. Re-running the unit suite from its standard output location passed all 74; the failure was caused by the alternate test output path, not the application change.
- `node --check src/Haven.Web/wwwroot/js/admin.js`: passed.
- `git diff --check`: passed (Git printed existing LF-to-CRLF working-copy warnings for unrelated dirty files).

## Live environment and remaining verification

- The local listener on port `5104` belongs to PID `12032`, running `src/Haven.Web/bin/Debug/net10.0/Haven.Web.exe`; its parent `dotnet run` process is PID `41972` and uses the `http` launch profile. No second HAVEN listener was found on that port.
- Chrome was available, but the foreground tab was ChatGPT. Although a tab titled “Quản lý người dùng” existed, the Admin page and its Network request were not brought into an inspectable browser state. No browser payload or screenshot is claimed.
- The integration tests use EF Core `UseInMemoryDatabase`; they do not prove that a write reached the local SQL Server database.
- The running app process was left untouched and predates this view edit. The build was written to a temporary output folder to avoid the locked executable, so the current process has not loaded the fix yet.
- Consequently, live Admin login, browser POST, local SQL persistence, and post-restart behavior remain **NOT VERIFIED**.

## How to verify locally

1. Stop the currently running HAVEN process from the terminal or IDE that launched it, then run the `http` launch profile again so the updated Razor view is loaded.
2. Sign in using the normal authorized Admin account and open **Admin → Người dùng**.
3. In browser DevTools Network, submit a lock action and confirm Form Data contains the target `id` and `locked=true`; unlock should send `locked=false`. Do not share the antiforgery token.
4. Confirm the page reports success and the account state changes in the local Development database. Then try login with the QA user and verify unlock restores it, subject to any other lockout policy.

## Final assessment

The source-level root cause is confirmed, fixed, and covered by MVC integration tests. The lock and unlock flow passes against the test Identity store while the app’s authorization and antiforgery controls remain enabled. Browser Network capture and live SQL Server persistence have not been verified, so the overall result is **PARTIALLY COMPLETED**, not a live-environment acceptance.
