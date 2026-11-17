# HAVEN Full-Stack Integration Test Report

Review date: 2026-11-03

## Result summary

| Area | Result | Evidence |
|---|---|---|
| Public workflows | PASS (automated route coverage) | TestServer returned 200 for Home, Search, Compare, Contact, Privacy and Projects. `/Property` redirects to Search. Home/Search query real Application services and return only approved public listings. |
| User workflows | PASS (InMemory/TestServer) | User can create a listing with an active PropertyType; the persisted listing is Draft and owned by the authenticated principal even when an OwnerId form field is supplied. Missing/inactive type and invalid form data are rejected. |
| Empty property-type state | PASS (InMemory/TestServer + local DB query) | With no active PropertyTypes, the create form explains the block and disables submission; the Admin dashboard shows a link to PropertyType management. Local `HavenDb` currently has 0 PropertyTypes and 0 Listings. |
| Admin workflows | PASS (InMemory/TestServer route/auth coverage) | Anonymous is redirected to Login and User to Access Denied across Admin routes; Admin renders Dashboard, Listings, PropertyTypes, Tags, Consultations and Reports and can create a PropertyType. |
| Antiforgery | PASS (InMemory/TestServer) | Listing creation with missing or invalid antiforgery tokens returns 400 and persists no listing. |
| Navigation and placeholder links | PASS (source scan + route tests) | No `href="#"` or `javascript:` links remain in Razor views. Key destination routes were exercised through TestServer. |
| CSS integration | PARTIAL | Listing form sections and empty-state alerts use the existing design tokens; mobile registration page had no horizontal overflow at 1440, 1280, 1024, 768 and 375px. Full visual review of all authenticated/Admin pages remains outstanding. |
| Release build | PASS | `dotnet build HAVEN.slnx --configuration Release` — 0 warnings, 0 errors. |
| Full tests | PASS | `dotnet test HAVEN.slnx --configuration Release` — 126 passed, 0 failed (all projects; workflow tests use InMemory). |
| SQL Server and migrations | PARTIAL | Confirmed local SQL Express database `HavenDb`; `AddTags` was the only pending migration and was applied. Rechecked migration history. No SQL-backed listing/admin page workflow was run. |

## Commands and outcomes

| Exact command | Result |
|---|---|
| `dotnet build HAVEN.slnx --configuration Release` | **PASS** — 0 warnings, 0 errors. |
| `dotnet test HAVEN.slnx --configuration Release` | **PASS** — 126 passed, 0 failed. |
| `git diff --check` | **PASS** — no whitespace errors in the Git diff. |

## Route and authorization coverage

`MvcWorkflowTests` verifies these route groups:

- Public: `/`, `/properties`, `/compare`, `/Home/Contact`, `/Home/DuAn`, `/Home/Privacy`, and legacy `/Property` redirect.
- Anonymous: protected User and Admin routes redirect to `/account/login`.
- User: profile, password change, owned listings, Favorites, and consultation pages render; all covered Admin routes redirect to `/account/access-denied`.
- Admin: Dashboard, Listings, PropertyTypes (including Create), Tags, Consultations, and Reports render for an Admin role; PropertyType creation is covered.
- Listing creation: valid form persists a Draft with principal-derived ownership; missing/invalid antiforgery, invalid model, and invalid PropertyType do not persist a listing.

The controllers apply User/Admin authorization and service-level ownership scoping. The integration suite uses an InMemory database, not SQL Server.

## Browser checks

The local browser served `/account/register` and `/` on port 5104. The registration page had no horizontal overflow at 1440, 1280, 1024, 768 and 375px; the primary navigation CTA text/background contrast was white on navy. No authenticated Admin session was available in the browser, so Admin pages were not manually exercised there.

## Remaining verification

1. Reproduce the user's authenticated Admin page failures in the local browser; current-source TestServer route tests pass, but authenticated SQL Server-backed Admin pages were not manually verified.
2. Verify creating and editing listings against SQL Server after an Admin creates/activates a PropertyType; the current local database has none.
3. Perform keyboard/screen-reader review across the full Home, Search, User and Admin experience.
4. Compare selections are URL-based and not persisted. Email confirmation is unsupported. Location remains free-text because no verified administrative-boundary dataset is included in the repository.

## Database state

The local SQL Express `HavenDb` has all six migrations recorded, including `20261103142053_AddTags`. That migration was applied to this local development database only. The database currently contains zero PropertyTypes, zero Listings, zero Tags and zero ListingTags; integration tests use EF Core InMemory and do not verify SQL Server-backed MVC behavior.

## Verification on 2026-11-17

The current checkout was revalidated on branch `develop` at HEAD `a3baab7f01cc9ddd1ffb708ff03e5dcbca772aa9`:

| Exact command | Result |
|---|---|
| `dotnet restore HAVEN.slnx` | **PASS** — all projects were up to date. |
| `dotnet build HAVEN.slnx --no-restore` | **PASS** — 0 warnings, 0 errors. |
| `dotnet test HAVEN.slnx --no-build` | **PASS** — 74 unit tests and 52 integration tests; 126 passed, 0 failed, 0 skipped. |
| `dotnet build HAVEN.slnx --configuration Release` | **PASS** — 0 warnings, 0 errors. |
| `dotnet test HAVEN.slnx --configuration Release --no-build` | **PASS** — 126 passed, 0 failed, 0 skipped. |
| `dotnet ef migrations list --project src/Haven.Infrastructure/Haven.Infrastructure.csproj --startup-project src/Haven.Web/Haven.Web.csproj` | **PASS** — connected to the configured SQL Server and listed all six migrations as applied. This was read-only; no migration was applied. |
| `git diff --check` | **PASS** — no whitespace errors. |

The GitHub Actions workflow was reviewed locally, but no remote workflow run was triggered or inspected. The automated integration tests still use EF Core InMemory, so SQL Server connection and migration history are verified while SQL-backed application workflows are not.

## Verification after media implementation (2026-11-17)

| Exact command / check | Result |
|---|---|
| `dotnet build HAVEN.slnx --configuration Debug --no-restore -p:BaseOutputPath="$env:TEMP\haven-solution-debug-final\"` | **PASS** â€” all six projects compiled with 0 warnings/errors. Temporary output avoids the running app's locked default Debug assemblies. |
| `dotnet build HAVEN.slnx --configuration Release --no-restore -p:BaseOutputPath="$env:TEMP\haven-solution-release-final\"` | **PASS** â€” all six projects compiled with 0 warnings/errors. |
| `dotnet test tests/Haven.UnitTests/Haven.UnitTests.csproj --configuration Debug --no-restore` | **PASS** â€” 74 passed, 0 failed, 0 skipped. |
| `dotnet test tests/Haven.UnitTests/Haven.UnitTests.csproj --configuration Release --no-restore` | **PASS** â€” 74 passed, 0 failed, 0 skipped. |
| `dotnet test HAVEN.slnx --configuration Debug --no-build --no-restore` | **PASS** â€” 74 unit tests and 60 integration tests; 134 passed, 0 failed, 0 skipped. |
| `dotnet test HAVEN.slnx --configuration Release --no-build --no-restore` | **PASS** â€” 74 unit tests and 60 integration tests; 134 passed, 0 failed, 0 skipped. |
| `dotnet ef migrations list ... --no-build` | **PARTIAL** â€” all seven migration definitions, including `20261117093013_AddListingImages`, were listed. SQL Server was unreachable at the time, so EF could not determine applied/pending status. The migration update had returned success earlier in this task, but applied state could not be rechecked. |
| `git diff --check` | **PASS** â€” no whitespace errors. |

The integration suite uses EF Core InMemory and temporary local file storage, not SQL Server. It covers valid/invalid image uploads, size and count limits, ownership, public access, choosing and deleting a primary image, failed file/metadata persistence cleanup, admin media management, and gallery/card rendering. Test image payloads exercise format signatures and MVC flows; they do not prove full raster decoding by a browser. A local browser showed the home page at a narrow viewport, but authenticated routes and media upload were not manually exercised in the browser. No remote GitHub Actions run was started. The whole-solution build with default output was blocked by a running app's locked files, so the final successful builds used temporary `BaseOutputPath` directories.

The Admin users page is part of the current route coverage. Its integration scenario searches for a User, locks and unlocks the account, verifies state persistence, clamps an extreme page value, and confirms an Admin cannot lock their own or another Admin account. The server enforces these rules independently of the UI.

## Verification after video, lockout and location completion (2026-11-17)

Latest verification supersedes earlier totals and the earlier note that location remained free text:

| Exact command / check | Result |
|---|---|
| `dotnet restore HAVEN.slnx` | **PASS** — all projects were up to date. |
| `dotnet build HAVEN.slnx --configuration Debug --no-restore -p:BaseOutputPath="$env:TEMP\haven-final-debug\"` | **PASS** — all six projects, 0 warnings and 0 errors. |
| `dotnet build HAVEN.slnx --configuration Release --no-restore -p:BaseOutputPath="$env:TEMP\haven-final-release\"` | **PASS** — all six projects, 0 warnings and 0 errors. |
| `$env:HAVEN_REPOSITORY_ROOT='D:\Study\ĐỒ ÁN\LAPTRINHWEB\HAVEN'; dotnet test HAVEN.slnx --configuration Debug --no-build --no-restore -p:BaseOutputPath="$env:TEMP\haven-final-debug\"` | **PASS** — 74 unit + 72 integration; 146 passed, 0 failed, 0 skipped. |
| `$env:HAVEN_REPOSITORY_ROOT='D:\Study\ĐỒ ÁN\LAPTRINHWEB\HAVEN'; dotnet test HAVEN.slnx --configuration Release --no-build --no-restore -p:BaseOutputPath="$env:TEMP\haven-final-release\"` | **PASS** — 74 unit + 72 integration; 146 passed, 0 failed, 0 skipped. |
| `dotnet ef migrations has-pending-model-changes --project src/Haven.Infrastructure/Haven.Infrastructure.csproj --startup-project src/Haven.Web/Haven.Web.csproj --no-build` | **PASS** — EF reports no model changes after the latest migration. Read-only; migration was not applied. |
| `git diff --check` | **PASS** — no whitespace errors. |

All integration tests use EF Core InMemory plus temporary local media storage; they are not SQL Server integration tests. The current migration was not applied to any SQL Server database, so SQL Server schema upgrade and SQL-backed workflows remain unverified. Full browser-based visual testing was not performed in this pass. Video tests verify MP4 `ftyp` and WebM EBML signatures, empty files, missing stored files, routing, authorization, cleanup, simulated storage/metadata failures, Admin upload/delete, form redisplay and byte ranges; they do not decode codecs or prove playback for every browser/device. App binaries were built under the system temporary directory to avoid overwriting files held by a running local app. The integration fixture for Public Listings was corrected to use InMemory, preventing accidental use of the configured SQL Server.
