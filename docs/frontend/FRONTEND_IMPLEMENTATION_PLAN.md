# HAVEN Full-Stack MVC Implementation Status

The application remains ASP.NET Core MVC/Razor on the existing .NET 10 solution. Authentication forms use same-origin `/api/auth` requests with cookie and antiforgery handling; other MVC workflows call the registered Application services in-process. No SPA, loopback HTTP client, or additional persistence mechanism was introduced.

| Module | Implementation | Evidence / remaining validation |
|---|---|---|
| Public website | Implemented | Home/search use approved listings; listing cards use the primary image where available, detail shows the responsive gallery and an explicit empty state. |
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
| Listing images | Implemented | Multi-image upload, private local storage, signature/size/extension/MIME checks, owner/Admin authorization, ordering, primary image, delete, previews, cards and detail gallery. Approved owner edits return the listing to Pending. |
| Listing videos | Implemented | MP4/WebM multi-upload, local storage outside webroot, extension/MIME/container-signature/size/count checks, ownership and Admin checks, byte-range playback, delete and public availability only for Approved listings. Integration coverage checks wrong owners, invalid data and metadata-persistence cleanup. Header validation does not decode/transcode the full media stream. |
| Vietnam locations | Implemented | Bundled official code dataset (34 provinces, 3,321 wards), public dependent-select endpoints, province/ward foreign keys and code indexes. New listings use coded locations; old province/district strings remain preserved and searchable. Seeder validates an existing dataset and does not overwrite a mismatch. |
| Admin user lockout | Implemented | Admin-only user page sets/clears Identity lockout and rotates the security stamp; integration test confirms new login is rejected and an existing cookie is invalidated on the configured one-minute stamp revalidation. |
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

- The current working tree contains additive migrations for images, videos and normalized locations. Migration application is an operator action after verifying the target database; no migration was applied during the current video/location/lockout implementation.
- The integration suite uses EF Core InMemory; passing tests do not certify SQL Server connection, migrations or SQL query behavior.
- Compare IDs live in the URL and do not persist across sessions/devices.
- Email confirmation and anonymous consultation remain unsupported by the current data model/business contract.
- Video checks validate the container signature but do not decode or transcode media; browser codec compatibility depends on the uploaded MP4/WebM codecs.
- Admin CSS is responsive and scoped to the Admin layout; this implementation pass did not perform browser-based visual or assistive-technology review.

The bundled administrative dataset comes from the General Statistics Office's published 2025 code list and Decision 19/2025/QĐ-TTg. As of 2026-11-17 it matches the currently effective 34-province/3,321-commune code list. A government proposal to adjust the Hà Nội–Phú Thọ boundary was submitted to the National Assembly in November 2026 with a proposed 2027-01-01 effective date; refresh and formally reconcile the location dataset before that date if the resolution is adopted.
