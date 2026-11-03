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
4. Compare selections are URL-based and not persisted. Listing images/media and email confirmation are unsupported by the current model.

## Database state

The local SQL Express `HavenDb` has all six migrations recorded, including `20261103142053_AddTags`. That migration was applied to this local development database only. The database currently contains zero PropertyTypes, zero Listings, zero Tags and zero ListingTags; integration tests use EF Core InMemory and do not verify SQL Server-backed MVC behavior.
