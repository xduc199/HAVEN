# HAVEN Frontend API Contract

This contract was checked against the current controllers, request models, application response records, domain enums, and authorization attributes in `src/Haven.Web`. Endpoints are same-origin paths; all JSON uses the ASP.NET Core web JSON naming policy (camelCase).

## Conventions

- Authentication is the Identity application cookie named `Haven.Auth`; it is `HttpOnly`, `SameSite=Lax`, and `Secure` outside Development.
- An API cookie challenge returns `401`; an authenticated caller missing the required role receives `403`. API routes do not redirect to MVC login pages.
- `Admin` means `[Authorize(Policy = "AdminOnly")]`, which requires the `Admin` role. Authenticated means any signed-in role.
- `ValidationProblemDetails` is returned for model/application validation errors (`400`). State conflicts return `ProblemDetails` (`409`). Missing or non-public resources generally return `404`.
- Query-string enum values can use the declared enum names. JSON request and response enums use numeric values because no `JsonStringEnumConverter` is configured.
- JSON enum values currently map as follows: `ListingPurpose` is `1` (`Sale`) or `2` (`Rent`); `ConsultationStatus` is `0` (`New`), `1` (`InProgress`), `2` (`Completed`), or `3` (`Cancelled`). `ListingStatus` query values are the declared names (`Draft`, `Pending`, `Approved`, `Rejected`, `Hidden`).
- Dates are serialized as ISO-8601 date-time strings. Decimal fields are JSON numbers.
- Every unsafe request (`POST`, `PUT`, `DELETE`) to MVC or `/api` requires antiforgery validation. First call `GET /api/auth/antiforgery`, retain the `Haven.Antiforgery` cookie, then send the returned `requestToken` in the `RequestVerificationToken` header. Same-origin browser requests send the cookie automatically.
- There is no image/media field in the current public listing contract.
- Authentication forms call the same-origin `/api/auth` endpoints from `wwwroot/js/api-forms.js`, sending the antiforgery token and cookie. If JavaScript is unavailable, the MVC form handlers remain the fallback.
- Listing, search, favorites, consultation, and Admin Razor pages currently use Application services in-process. The API contract is available to external JSON clients, but those Razor workflows are not yet API-backed.

## Endpoints

| Feature | Method | Endpoint | Request | Response | Authentication | Role |
|---|---|---|---|---|---|---|
| Antiforgery | GET | `/api/auth/antiforgery` | No body | `200 { "requestToken": "…" }`; sets antiforgery cookie. | Public | — |
| Authentication | POST | `/api/auth/register` | JSON: `{ "displayName": "…", "email": "…", "password": "…" }` | `201`; `AccountResponse`; sets sign-in cookie. `400` validation; `500` if role setup fails. | Public | — |
| Authentication | POST | `/api/auth/login` | JSON: `{ "email": "…", "password": "…", "rememberMe": false }` | `200 AccountResponse`; sets cookie. `401` invalid/locked-out credentials. | Public | — |
| Authentication | POST | `/api/auth/logout` | No body | `204`; `401` if unauthenticated. | Required | Any |
| Profile | GET | `/api/auth/me` | — | `200 AccountResponse`; `401`. | Required | Any |
| Profile | PUT | `/api/auth/me` | JSON: `{ "displayName": "…" }` (1–100 chars) | `200 AccountResponse`; `400`; `401`. | Required | Any |
| Profile | POST | `/api/auth/change-password` | JSON: `{ "currentPassword": "…", "newPassword": "…" }` | `204`; `400` Identity validation/current-password error; `401`. New password requires 12 chars with upper/lower/digit/special. | Required | Any |
| Search | GET | `/api/listings` | Query: `keyword`, `propertyTypeId`, `purpose`, `province`, `district`, `minimumPrice`, `maximumPrice`, `minimumAreaSqm`, `maximumAreaSqm`, `bedrooms`, `sort`, `page`, `pageSize`. | `200 PublicListingSearchResults`; `400` invalid query. Only Approved listings. | Public | — |
| Property details | GET | `/api/listings/{id}` | Positive integer `id` | `200 PublicListingDetails`; `400` invalid id; `404` missing/non-Approved. Includes `ownerDisplayName` only; no owner ID/email/phone. | Public | — |
| Compare | GET | `/api/listings/compare?ids=1&ids=2` | Repeated `ids` query parameter; 1–3 positive, unique IDs | `200` ordered array of `PublicListingDetails`; `400` invalid/duplicate/count; `404` if any requested listing is missing or not Approved. | Public | — |
| PropertyType | GET | `/api/property-types` | — | `200 PropertyTypeResponse[]`; active types only. | Public | — |
| Tags | GET | `/api/tags` | Query: `page` (default 1), `pageSize` (default 50, max 100) | `200 TagPageResponse`; active tags associated with at least one Approved listing only; `400` invalid paging. | Public | — |
| Tagged listings | GET | `/api/tags/{slug}/listings` | Query: `page` (default 1), `pageSize` (default 20, max 100) | `200 PublicTagListingPageResponse`; Approved listings only; `404` inactive/unknown/non-public tag; `400` invalid paging. | Public | — |
| Owned listings | POST | `/api/my/listings` | `ListingWriteRequest` JSON below; no `ownerId` | `201 OwnedListingDetails`; `400`; `401`. Creates Draft; owner is principal-derived. | Required | Any |
| Owned listings | GET | `/api/my/listings` | Query: `page` (default 1), `pageSize` (default 20, max 100) | `200 OwnedListingPage`; `400`; `401`. Includes only the caller's listings. | Required | Any |
| Owned listings | GET | `/api/my/listings/{id}` | Positive integer `id` | `200 OwnedListingDetails`; `400`; `401`; `404` for missing or another owner's listing. | Required | Any |
| Owned listings | PUT | `/api/my/listings/{id}` | Same `ListingWriteRequest` as create | `200 OwnedListingDetails`; `400`; `401`; `404`; `409` if current state disallows edit. Editing Approved resubmits it to Pending; editing Rejected returns it to Draft. | Required | Any |
| Owned listings | POST | `/api/my/listings/{id}/submit` | No body | `200 OwnedListingDetails`; `400`; `401`; `404`; `409` unless Draft/Rejected. | Required | Any |
| Owned listings | DELETE | `/api/my/listings/{id}` | Positive integer `id` | `204`; `400`; `401`; `404`; `409` unless Draft/Rejected. | Required | Any |
| Favorites | GET | `/api/favorites` | Query: `page` (default 1), `pageSize` (default 20, max 100) | `200 FavoritePage`; `400`; `401`. Only currently Approved listings are included. | Required | Any |
| Favorites | POST | `/api/favorites/{listingId}` | Positive integer route value; no body | `201` (empty); `400`; `401`; `404` if not Approved/not found; `409` duplicate. | Required | Any |
| Favorites | DELETE | `/api/favorites/{listingId}` | Positive integer route value | `204`; `400`; `401`; `404` if this user has no matching favorite. | Required | Any |
| Consultation | POST | `/api/consultations` | JSON: `{ "contactName": "…", "email": "…", "phoneNumber": "…", "listingId": 1, "message": "…" }`; `listingId` and `message` optional | `201 ConsultationDetails`; `400` invalid contact; `401`; `404` if optional listing is not Approved. | Required | Any |
| Consultation | GET | `/api/consultations` | Query: `page` (default 1), `pageSize` (default 20, max 100) | `200 ConsultationPageResult`, limited to requester. `401`. | Required | Any |
| Consultation | GET | `/api/consultations/{id}` | Positive integer `id` | `200 ConsultationDetails`; `400`; `401`; `404` for missing or another requester. | Required | Any |
| Admin moderation | GET | `/api/admin/moderation/pending` | Query: `page` (default 1), `pageSize` (default 20, max 100) | `200 ModerationListingPage`; `400`; `401`; `403`. Pending only. | Required | Admin |
| Admin listings | GET | `/api/admin/listings` | Optional query: `search`, `status` (`Draft`, `Pending`, `Approved`, `Rejected`, `Hidden`), `propertyTypeId`, `tagId`, `fromDate`, `toDate`, `page`, `pageSize` | `200 ModerationListingPage`; `400`; `401`; `403`. All statuses when omitted. Dates are inclusive; supply both or neither. | Required | Admin |
| Admin listings | GET | `/api/admin/listings/{id}` | Positive integer `id` | `200 ModerationListingDetails`; `400`; `401`; `403`; `404`. | Required | Admin |
| Admin tag assignment | GET | `/api/admin/listings/{listingId}/tags` | Positive listing ID | `200 TagResponse[]`; `400`; `401`; `403`. | Required | Admin |
| Admin tag assignment | POST | `/api/admin/listings/{listingId}/tags/{tagId}` | No body | `204`; `400`; `401`; `403`; `404`; `409` if already assigned/inactive. | Required | Admin |
| Admin tag assignment | DELETE | `/api/admin/listings/{listingId}/tags/{tagId}` | No body | `204`; `400`; `401`; `403`; `404`. | Required | Admin |
| Admin moderation | POST | `/api/admin/moderation/{id}/approve` | No body | `200 ModerationListingDetails`; `400`; `401`; `403`; `404`; `409` unless Pending. | Required | Admin |
| Admin moderation | POST | `/api/admin/moderation/{id}/reject` | JSON: `{ "reason": "…" }` (1–1000 chars) | `200 ModerationListingDetails`; `400`; `401`; `403`; `404`; `409` unless Pending. | Required | Admin |
| Admin moderation | POST | `/api/admin/moderation/{id}/hide` | No body | `200 ModerationListingDetails`; `400`; `401`; `403`; `404`; `409` unless Approved. | Required | Admin |
| Admin PropertyType | GET | `/api/admin/property-types` | — | `200 PropertyTypeResponse[]` (active and inactive); `401`; `403`. | Required | Admin |
| Admin PropertyType | POST | `/api/admin/property-types` | JSON: `{ "name": "…", "sortOrder": 0, "isActive": true }` | `201 PropertyTypeResponse`; `400`; `401`; `403`; `409` duplicate name. | Required | Admin |
| Admin PropertyType | PUT | `/api/admin/property-types/{id}` | Same body as create | `200 PropertyTypeResponse`; `400`; `401`; `403`; `404`; `409` duplicate name. | Required | Admin |
| Admin tags | GET | `/api/admin/tags` | Optional `search`, `page` (default 1), `pageSize` (default 20, max 100) | `200 TagPageResponse`; `400`; `401`; `403`. Includes active/inactive tags. | Required | Admin |
| Admin tags | GET | `/api/admin/tags/{id}` | Positive integer `id` | `200 TagResponse`; `400`; `401`; `403`; `404`. | Required | Admin |
| Admin tags | POST | `/api/admin/tags` | JSON: `{ "name": "…", "description": "…" }` | `201 TagResponse`; `400`; `401`; `403`; `409` normalized name/slug duplicate. New tags are active. | Required | Admin |
| Admin tags | PUT | `/api/admin/tags/{id}` | JSON: `{ "name": "…", "description": "…", "isActive": true }` | `200 TagResponse`; `400`; `401`; `403`; `404`; `409` normalized name/slug duplicate. Deactivate rather than delete tags with assignments. | Required | Admin |
| Admin consultation | GET | `/api/admin/consultations` | Optional query: `search`, `status` (`New`, `InProgress`, `Completed`, `Cancelled`), `fromDate`, `toDate`, `page`, `pageSize` | `200 ConsultationPageResult`; `400`; `401`; `403`. Search matches contact name/email/phone; dates are inclusive and supplied together. | Required | Admin |
| Admin consultation | GET | `/api/admin/consultations/{id}` | Positive integer `id` | `200 ConsultationDetails`; `400`; `401`; `403`; `404`. Contact data is admin-only. | Required | Admin |
| Admin consultation | PUT | `/api/admin/consultations/{id}/status` | JSON: `{ "status": 0 }`; `ConsultationStatus`: `0` New, `1` InProgress, `2` Completed, `3` Cancelled | `200 ConsultationDetails` (enum fields are numeric); `400`; `401`; `403`; `404`. | Required | Admin |
| Admin dashboard | GET | `/api/admin/dashboard` | — | `200 AdminDashboardSummary`; `401`; `403`. | Required | Admin |
| Admin analytics/reports | GET | `/api/admin/analytics` or `/api/admin/reports` | Optional `fromDate` and `toDate` ISO dates; both or neither; max 366 inclusive days | `200 AdminAnalyticsReport`; `400` invalid range; `401`; `403`. Aggregations are database-side; daily buckets are filled by the Application service. | Required | Admin |

## Request and response shapes

All unsafe API methods require the antiforgery cookie and `RequestVerificationToken` header described above. MVC forms include antiforgery tokens automatically.

### ListingWriteRequest

`propertyTypeId` (positive integer), `title` (1–200 chars), `description` (0–5000 chars), `purpose` (`Sale` or `Rent`), `priceAmount` (>0), `areaSqm` (>0), `province` (1–100 chars), `district` (1–100 chars), `addressLine` (1–300 chars), and optional non-negative integer `bedrooms`/`bathrooms`.

### Response records

- `AccountResponse`: `{ id, displayName, email, roles[] }`.
- `PublicListingSummary`: `{ id, propertyTypeId, title, purpose, priceAmount, areaSqm, province, district, bedrooms, createdAtUtc }`.
- `PublicListingSearchResults`: `{ items[], page, pageSize, totalCount, totalPages }`.
- `PublicListingDetails`: `{ id, propertyTypeId, propertyTypeName, title, description, purpose, priceAmount, areaSqm, province, district, addressLine, bedrooms, bathrooms, createdAtUtc, ownerDisplayName }`.
- `OwnedListingPage`: `{ items[], page, pageSize, totalCount, totalPages }`; the owner is derived from the authenticated principal.
- `OwnedListingDetails`: listing fields plus `status`, `updatedAtUtc`, `submittedAtUtc`, `approvedAtUtc`, and private `rejectionReason`; no `ownerId`.
- `ModerationListingDetails`: owned listing fields plus `ownerId`; Admin-only.
- `ModerationListingPage`: `{ items[], page, pageSize, totalCount, totalPages }`.
- `FavoritePage`: `{ items[], page, pageSize, totalCount, totalPages }`; favorite items are `{ listingId, title, purpose, priceAmount, areaSqm, province, district, listingCreatedAtUtc, favoritedAtUtc }`.
- `PropertyTypeResponse`: `{ id, name, isActive, sortOrder }`.
- `ConsultationDetails`: `{ id, contactName, email, phoneNumber, listingId, message, status, createdAtUtc, updatedAtUtc }`; requester ID is not included.
- `ConsultationPageResult`: `{ items[], page, pageSize, totalCount, totalPages }`.
- `AdminDashboardSummary`: `{ totalListings, pendingListings, approvedListings, newConsultations, inProgressConsultations }`.
- `TagResponse`: `{ id, name, slug, description, isActive, createdAtUtc, updatedAtUtc }`.
- `TagPageResponse`: `{ items[], page, pageSize, totalCount, totalPages }`.
- `PublicTagListingPageResponse`: `{ items[], page, pageSize, totalCount, totalPages }`; items expose public listing summary fields only.
- `AdminAnalyticsReport`: `{ dateRange, kpis, draftListings, rejectedListings, hiddenListings, totalConsultations, completedConsultations, cancelledConsultations, totalUsers, listingsByDay[], consultationsByDay[], listingsByStatus[], listingsByPropertyType[], consultationsByStatus[], latestListings[], pendingListings[], latestConsultations[] }`. Count/listing/consultation charts and period-scoped KPIs use the requested date range; `totalUsers` is the current all-time user count. Latest/pending tables are bounded to eight records.

## Current integration limits

1. Compare selections are carried in the page URL and are not persisted between sessions or devices.
2. Listing images/media are not represented in the current domain or API, so the UI does not fabricate them.
3. Consultation submission requires authentication; the MVC page pre-fills the signed-in user's name and email and lets them enter a phone number and optional message.
4. Registration creates only the `User` role. Admin provisioning is configuration-controlled; there is no public Admin registration path.
5. Registration/profile workflows use ASP.NET Core Identity from the Web layer. MVC and API controllers currently own their respective HTTP workflows; do not add loopback HTTP calls between them.
6. SQL Server persistence and migration application must be configured and run by the deployment/operator; the automated integration suite uses EF Core InMemory and does not certify SQL Server.
