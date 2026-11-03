# HAVEN Full-Stack MVC Implementation Status

The application remains ASP.NET Core MVC/Razor on the existing .NET 10 solution. Authentication forms use same-origin `/api/auth` requests with cookie and antiforgery handling; other MVC workflows call the registered Application services in-process. No SPA, loopback HTTP client, or additional persistence mechanism was introduced.

| Module | Implementation | Evidence / remaining validation |
|---|---|---|
| Public website | Implemented | Home and search use approved listings from Application services; details, compare, Contact, Privacy and Projects routes resolve. No listing media field exists, so images are not fabricated. |
| Authentication/profile | Implemented | Login, registration, logout, profile update and password change submit to the existing `/api/auth` endpoints with antiforgery handling; MVC form handlers provide a no-JavaScript fallback. Admin redirects are based on the returned role claims. Registration assigns User only; Admin is configuration-bootstrapped. |
| User listing management | Implemented | Owner-scoped, paginated list; create/edit/details/submit/delete. Owner ID comes from the authenticated principal; Domain state rules and positive price/area validation apply. |
| Favorites | Implemented | Persistent service/repository-backed list and antiforgery-protected add/remove actions; only approved public listings are eligible. |
| Compare | Implemented | Approved-only details, max three, duplicate IDs normalized, remove links, responsive horizontal table and empty/error state. Selection is carried in the URL, not persisted. |
| Consultation | Implemented | Authenticated create, own-list paginated list/details and Admin status management; phone is required and ListingId is optional and checked against public availability. |
| Admin dashboard, analytics and reports | Implemented | Dedicated Admin layout; period presets/custom dates; real KPI, daily, status and Property Type aggregates; bounded latest/pending tables; reports page and Admin-protected API. |
| Admin listings/moderation | Implemented | Search/status/Property Type/Tag/date filters, pagination, details, tag assignment, approve, reject-with-reason and hide. |
| Admin PropertyTypes | Implemented | Admin-only list/create/edit with active/inactive lifecycle. |
| Admin tags | Implemented | Admin-only paginated search/create/edit/deactivate and listing assignment; unique normalized name/slug; public approved-only tag/listing endpoints. |
| Admin consultations | Implemented | Admin-only search/status/date filters, pagination, details/status update; no delete action. |
| Security and integration | Implemented; deployment verification remains | Cookie authentication, role guards, owner isolation, antiforgery and API validation have automated coverage. SQL Server/migration application and complete manual device-browser testing remain environment-dependent. |

## Architecture and security decisions

- Keep MVC/Razor and the current Clean Architecture boundaries.
- Do not call the same application's `/api` endpoints server-to-server from MVC. Browser-side calls may use same-origin APIs with the configured cookie and antiforgery protocol; workflows not yet migrated continue to reuse Application services in-process.
- Apply `[Authorize]` and the `AdminOnly` policy on the server; navigation visibility is only a usability aid.
- Use the server principal for owner/requester identity. Do not bind owner IDs from forms.
- Use antiforgery tokens on every MVC/API state-changing request. JSON clients get a request token from `GET /api/auth/antiforgery`, retain its cookie and send `RequestVerificationToken`.
- Public search/details/compare expose only Approved listings. Admin-only workflows own moderation and private consultation access.
- Use ViewModels for HTML forms and keep API DTOs separate from EF entities.

## Current operational limitations

- Migrations are present but were not applied to a SQL Server during this work. Configure the database and explicitly run the EF migration update before deployment.
- The integration suite uses EF Core InMemory; passing tests do not certify SQL Server connection, migrations or SQL query behavior.
- Compare IDs live in the URL and do not persist across sessions/devices.
- Listing images, email confirmation and a public/anonymous consultation workflow are not supported by the current data model/business contract.
- Admin CSS is responsive and scoped to the Admin layout; automated browser viewport/assistive-technology review remains to be performed.
