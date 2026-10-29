# HAVEN public listing search contract

This is the initial backend contract for `GET /api/listings`. The repository
contains listing fields for type, purpose, price, area, province, district,
address, title, description, status, and creation time. Public listing details
are exposed only for `Approved` listings; search follows that same public
visibility rule.

The assignment leaves several product choices for review. This implementation
uses the following conservative baseline so the search can be exercised end to
end. Confirm the keyword fields, text matching, sort options, page-size limit,
and total-count requirement with the product/UI owners before treating this as
the final MVP contract.

## Query parameters

| Parameter | Meaning |
|---|---|
| `keyword` | Partial match in title, description, or address line. Blank values are ignored. |
| `propertyTypeId` | Exact property-type ID. |
| `purpose` | `Sale` or `Rent`, matching `ListingPurpose` (numeric enum values `1` and `2` are also accepted by model binding). |
| `minPriceAmount`, `maxPriceAmount` | Inclusive lower/upper price bounds; either bound can be omitted. |
| `minAreaSqm`, `maxAreaSqm` | Inclusive lower/upper area bounds; either bound can be omitted. |
| `province`, `district` | Partial match against the corresponding stored location field. |
| `sortBy` | `newest` (default), `price_asc`, or `price_desc`. |
| `page` | One-based page number; default `1`. |
| `pageSize` | Results per page; default `20`, maximum `100`. |

All supplied filters combine with AND. Public status is not a user-selectable
filter: the query always restricts results to `Approved`. Price and area bounds
are inclusive. Invalid enum values, negative bounds, min greater than max,
unknown sorts, invalid page values, and over-limit page sizes return HTTP 400.

Each sort includes `Id` as a secondary key, keeping page boundaries stable.
Search, count, sort, projection, and pagination remain in the EF Core query and
execute against SQL Server. The response contains `items`, `page`, `pageSize`,
`totalCount`, and `totalPages`; `totalPages` is `0` for no matches. No schema or
index change is included because no measured search workload justified one.

## Example

```text
GET /api/listings?purpose=1&minPriceAmount=1000000000&province=Ha%20Noi&sortBy=price_asc&page=1&pageSize=20
```

The response item fields are `id`, `propertyTypeId`, `title`, `purpose`,
`priceAmount`, `areaSqm`, `province`, `district`, `bedrooms`, `bathrooms`, and
`createdAtUtc`.
