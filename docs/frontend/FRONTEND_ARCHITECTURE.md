# HAVEN MVC/Razor Architecture

## Application shape

- `Haven.Web` is the ASP.NET Core MVC host for both Razor pages and JSON APIs.
- `Program.cs` registers ASP.NET Core Identity, the `Haven.Auth` cookie, the `AdminOnly` role policy, EF Core SQL Server, Application services and antiforgery validation.
- MVC pages live under `Views/` and `Areas/Admin/Views/`; their page-specific types live under `ViewModels/`.
- Authentication forms use same-origin browser requests to `/api/auth`; `wwwroot/js/api-forms.js` obtains an antiforgery token, sends the antiforgery header, and relies on the shared Identity cookie.
- Public search, details, compare, Home, owned listings, favorites, consultations and Admin pages currently call Application services in-process. MVC must not call its own `/api` endpoints over HTTP or use `ApplicationDbContext` directly.
- `/api/...` remains the JSON contract for external clients and is listed in [API_CONTRACT.md](./API_CONTRACT.md). Except for authentication, current Razor workflows do not yet consume those endpoints.

## Request flow

```text
Browser
  -> MVC route + antiforgery-protected form or same-origin Authentication API request
  -> MVC controller + page ViewModel
  -> Application service / ASP.NET Core Identity
  -> repository + EF Core
  -> Razor response
```

For JSON consumers, API controllers map request DTOs to the same Application/Identity operations and return API response shapes. Domain state rules remain in Domain/Application; views and JavaScript must not reimplement them. Authentication JavaScript is limited to transport and safe role-based redirection; MVC form handlers remain available when JavaScript is disabled.

## UI structure

- `_Layout.cshtml` owns the shared navigation, footer, skip link and shared feedback region.
- Admin Area views use a separate `_AdminLayout.cshtml` shell with role-protected sidebar navigation, identity/logout, breadcrumb, responsive drawer, and its own namespaced styles. Admin MVC pages call Application services directly; APIs remain for external clients.
- `_Feedback.cshtml` renders HTML-encoded TempData messages in accessible live regions.
- `_PropertyCards.cshtml` is reused by Home and public search.
- `wwwroot/css/frontend.css` owns the responsive shell and feature-page styling. Its border token is namespaced (`--haven-ui-border`) and link styles are scoped to `body.app-shell`; legacy `site.css` remains loaded first.
- `wwwroot/js/site.js` is limited to progressive enhancement. Business workflows remain server-rendered and form-backed.
- Server-rendered page requests do not need a browser loading spinner. Data pages have server-rendered empty/error/validation states, and mutations report success or failure through feedback messages.

## Authentication and authorization

- Identity uses the `Haven.Auth` HttpOnly cookie. API challenges return `401`/`403`; MVC challenges redirect to `/account/login` or `/account/access-denied`.
- User-scoped IDs are derived from the authenticated principal; client-supplied owner/requester IDs are not trusted.
- Admin Area controllers apply `[Authorize(Policy = "AdminOnly")]`. Hiding the Admin link is not an authorization boundary.
- Admin dashboard/reports use the analytics repository for database-side status/date/type aggregates and bounded latest-item queries. Tags use an explicit many-to-many `ListingTag` entity; public tag queries expose only active tags connected to Approved listings.
- All unsafe MVC/API actions require antiforgery validation. MVC forms render tokens; JSON clients obtain a request token from `GET /api/auth/antiforgery`, retain the antiforgery cookie and send `RequestVerificationToken`.
- Public listing service queries expose only Approved listings. User listing/favorite/consultation services scope reads and writes to the authenticated user. Admin services own moderation and management operations.
- Razor output is HTML-encoded by default. Do not use `Html.Raw` for user-provided content. Known validation/state errors are shown to users; unexpected exceptions remain behind the application exception handler.

## Current constraints

- Compare selections are URL-based, not persisted across sessions.
- The listing model has no images/media field, so the UI does not show fabricated images.
- Consultation submission requires authentication; the MVC form pre-fills the current user's display name and email.
- Creating a listing requires an active PropertyType. When none exist, the User form explains why submission is unavailable and the Admin dashboard links to PropertyType management; no sample property types or listings are inserted automatically.
- Admin accounts are provisioned only through configured bootstrap credentials; public registration assigns only the User role.
- Integration tests use EF Core InMemory. They do not prove SQL Server connectivity or that migrations have been applied.
