# API Design

This is the contract between backend and frontend. It is a *living* 
document: an endpoint change updates this file in the same branch 
(rule API-1 in [dev/ENGINEERING-STANDARDS.md](dev/ENGINEERING-STANDARDS.md)). 
Implementation state per endpoint is tracked in 
[dev/STATUS.md](dev/STATUS.md).

## Conventions

- Base path: `/api`
- Resources are plural, lowercase, kebab-case for multi-word names
- HTTP verbs express the action (GET, POST, PUT, PATCH, DELETE) — never 
  in the URL
- Nested resources express ownership (e.g. images belong to a listing)
- Requests and responses are JSON (except image upload)
- Timestamps are ISO 8601 in UTC (e.g. `2026-11-11T10:15:00Z`); ids are 
  UUIDs
- Paginated endpoints accept `page` (1–10,000, default 1) and `pageSize` 
  (1–100, default 20). Values out of range return `400`; they are not 
  silently corrected

### Status codes

| Code | Used for |
|---|---|
| `200 OK` | Successful GET, PUT, login/register |
| `201 Created` | Resource created (POST). Includes a `Location` header pointing to the new resource and the resource in the body |
| `204 No Content` | Successful DELETE, and PATCH without a response body |
| `400 Bad Request` | Validation error or business rule violation (e.g. 6th image, email already registered) |
| `401 Unauthorized` | Missing/invalid/expired token, or wrong credentials at login |
| `403 Forbidden` | Authenticated but not allowed (not owner, not admin) |
| `404 Not Found` | Resource does not exist |
| `429 Too Many Requests` | Rate limit exceeded (auth endpoints, NFR-11) |
| `500` | Unexpected server error — never used on purpose |

### Error responses

Every error body is an RFC 9457 **`ProblemDetails`** 
(content type `application/problem+json`), decided in 
[ADR-009](adr/ADR-009-Error-Handling.md) and 
[ADR-010](adr/ADR-010-Input-Validation.md).

Business and auth errors (`400`, `401`, `403`, `404`, `500`), produced by 
the global exception handler from the service's custom exceptions:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4",
  "title": "Forbidden",
  "status": 403,
  "detail": "You can only modify your own listings.",
  "instance": "/api/listings/3f2a…"
}
```

Validation errors (`400`), produced automatically by `[ApiController]` 
from the DataAnnotations on the request DTO, with one entry per invalid 
field:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Price": [ "The field Price must be between 0.01 and 1000000." ],
    "Title": [ "The Title field is required." ]
  }
}
```

The client reads `status` and `detail` (or `errors` for validation). 
Unexpected errors return `500` with a generic `detail`; the real cause 
is only logged.

> **Implementation state**: the auth endpoints still return the old 
> `{ "error": "..." }` shape until they are migrated (technical debt E1 
> in [dev/STATUS.md](dev/STATUS.md)). New endpoints use `ProblemDetails` 
> from the start.

Error messages never contain stack traces, SQL or internal details 
(rule ERR-4).

## Field constraints

Request validation targets, aligned with the column limits in 
[05-Database.md](05-Database.md). They are the contract, implemented with 
DataAnnotations on the request DTOs ([ADR-010](adr/ADR-010-Input-Validation.md)); 
business-rule checks (allowed lists, ownership) stay in the services.

| Field | Rule |
|---|---|
| `email` | required, valid email format, max 255 chars; compared case-insensitively (stored normalized) |
| `password` | required, 8–128 chars |
| `displayName` | required, 1–100 chars after trimming |
| `title` | required, 1–150 chars after trimming |
| `description` | required, 1–5,000 chars |
| `price` | required, > 0, ≤ 1,000,000, at most 2 decimals |
| `category` | required, one of the categories in [04-DomainModel.md](04-DomainModel.md) |
| `condition` | required, one of the conditions in 04-DomainModel.md |
| `brand` | optional, max 60 chars after trimming |
| `status` | one of `available`, `sold` |

## Authentication

| Method | Endpoint           | Auth | Success | Description                              |
| ------ | ------------------ | ---- | ------- | ---------------------------------------- |
| POST   | /api/auth/register | No   | 200     | Create a new user account (rate limited) |
| POST   | /api/auth/login    | No   | 200     | Authenticate, returns JWT (rate limited) |

**POST /api/auth/register** — body:
```json
{ "email": "user@example.com", "password": "...", "displayName": "..." }
```
Errors: `400` on invalid fields or if the email is already registered 
(in any letter case); `429` when rate limited.

**POST /api/auth/login** — body:
```json
{ "email": "user@example.com", "password": "..." }
```
Errors: `401` on unknown email or wrong password (same message for both, 
so the API doesn't reveal which emails are registered); `429` when rate 
limited.

Both endpoints return `200` with the same body (`AuthResponseDto`), so a 
user is logged in immediately after registering:
```json
{ "token": "eyJ...", "userId": "...", "displayName": "...", "role": "user" }
```

Logout has no endpoint: the client discards the token (FR-3).

## Users

| Method | Endpoint      | Auth | Success | Description                                  |
| ------ | ------------- | ---- | ------- | -------------------------------------------- |
| GET    | /api/users/me | Yes  | 200     | Get current user's profile                   |
| PUT    | /api/users/me | Yes  | 200     | Update current user's profile (display name) |

Bodies are defined when the endpoints are designed (FR-4).

## Listings

| Method | Endpoint                  | Auth            | Success | Description |
|--------|---------------------------|-----------------|---------|-------------|
| GET    | /api/listings             | No              | 200     | List/search listings (paginated, filterable, sortable) |
| GET    | /api/listings/{id}        | No              | 200     | Get a single listing's details |
| POST   | /api/listings             | Yes             | 201     | Create a new listing |
| PUT    | /api/listings/{id}        | Owner or admin  | 200     | Update a listing's editable content |
| DELETE | /api/listings/{id}        | Owner or admin  | 204     | Delete a listing and its images |
| PATCH  | /api/listings/{id}/status | Owner or admin  | 204     | Change a listing's status (e.g. mark as sold) |

**GET /api/listings** — query params (all optional):

| Param | Meaning |
|---|---|
| `search` | Keyword matched case-insensitively against title and description; `%` and `_` match literally |
| `category`, `condition` | Exact value from the allowed lists; other values → `400` |
| `brand` | Case-insensitive partial match |
| `minPrice`, `maxPrice` | Inclusive range; `minPrice > maxPrice` → `400` |
| `status` | `available` (default) or `sold` |
| `sort` | `newest` (default, by creation date descending), `price_asc`, `price_desc`; other values → `400`. Ties are broken by id, so pages never overlap |
| `page`, `pageSize` | See Conventions |

Response `200`:
```json
{
  "data": [
    {
      "id": "...",
      "title": "...",
      "price": 100.0,
      "category": "Guitars",
      "condition": "Good",
      "brand": "Fender",
      "status": "available",
      "createdAt": "2026-11-11T10:15:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 143,
  "totalPages": 8
}
```

**GET /api/listings/{id}** — response `200`:
```json
{
  "id": "...",
  "title": "...",
  "description": "...",
  "price": 100.0,
  "category": "Guitars",
  "condition": "Good",
  "brand": "Fender",
  "status": "available",
  "createdAt": "2026-11-11T10:15:00Z",
  "updatedAt": "2026-11-11T10:15:00Z",
  "images": [ { "id": "...", "url": "https://...", "displayOrder": 0 } ],
  "seller": { "id": "...", "displayName": "...", "email": "seller@example.com" }
}
```
`seller.email` is present **only when the request is authenticated**; 
for anonymous requests it is omitted from the response (FR-15). 
Sold listings are returned too, with `status: "sold"` (FR-10). 
Errors: `404` if the listing doesn't exist.

**POST /api/listings** — body:
```json
{
  "title": "...",
  "description": "...",
  "price": 100.0,
  "category": "Guitars",
  "condition": "Good",
  "brand": "Fender"
}
```
The owner is the authenticated user (from the token, never from the 
body); status is always `available`. Response `201` with a `Location` 
header and the created listing (same shape as `GET /{id}`). 
Errors: `400`, `401`.

**PUT /api/listings/{id}** — same body as POST. It replaces the editable 
content only; it does **not** accept `status`, owner or timestamps. 
Response `200` with the updated listing. Errors: `400`, `401`, `403`, 
`404`.

**PATCH /api/listings/{id}/status** — body:
```json
{ "status": "sold" }
```
Status changes have their own endpoint (decided 2026-11-11) because 
they are a domain action, not a field edit: in v2 a completed simulated 
payment will also change the status automatically, and an intermediate 
state (e.g. `reserved`) is likely. Allowed values and transitions are 
validated in the service. Response `204`. Errors: `400`, `401`, `403`, 
`404`.

Transitions (decided 2026-11-13): `available` → `sold` and `sold` → 
`available` are both allowed (a seller can relist, e.g. when a buyer 
backs out). Setting the current status again is accepted as a no-op 
(`204`, `updatedAt` refreshed). Any other value → `400`. To revisit 
when v2 adds `reserved` and automatic sales.

## Images

| Method | Endpoint                            | Auth  | Success | Description |
|--------|-------------------------------------|-------|---------|-------------|
| POST   | /api/listings/{id}/images           | Owner | 201     | Upload an image (max 5 per listing) |
| DELETE | /api/listings/{id}/images/{imageId} | Owner | 204     | Delete a specific image |

Upload flow (separate endpoint after creation vs. combined multipart 
request), accepted formats and maximum file size are decided when image 
upload is implemented. Errors: `400` (6th image, invalid file), `401`, 
`403`, `404`.

## Health

| Method | Endpoint  | Auth | Success | Description |
|--------|-----------|------|---------|-------------|
| GET    | /health   | No   | 200     | API and database reachable (`503` if the database is not) — planned, NFR-15 |

Outside `/api` because it is for infrastructure (deployment platform, 
monitoring), not for the frontend.

## Authorization summary

- **Public** (no auth): browsing/searching listings, viewing a single 
  listing (without the seller's email), health
- **Logged-in user**: create listings, view/edit own profile, see the 
  seller's email on listings
- **Owner**: edit/delete own listings, change their status, manage their 
  images
- **Admin**: can edit/delete any listing and change its status (see 
  03-UserStories.md → US-18)

Ownership is checked in the **service layer** by comparing the listing's 
`UserId` with the authenticated user's id (read from the JWT), so that 
guessing a listing ID is not enough (NFR-2). A non-owner gets `403`, a 
missing listing gets `404`, both as `ProblemDetails` produced by the 
global exception handler (see Error responses).
