# API Design

## Conventions

- Base path: `/api`
- Resources are plural, lowercase, kebab-case for multi-word names
- HTTP verbs express the action (GET, POST, PUT, DELETE) — never in the URL
- Nested resources express ownership (e.g. images belong to a listing)
- Responses are JSON
- Paginated endpoints accept `page` and `pageSize` query params (defaults: 
  `page=1`, `pageSize=20`) and return metadata alongside the data

## Authentication

| Method | Endpoint             | Auth required | Description                  |
|--------|-----------------------|----------------|--------------------------------|
| POST   | /api/auth/register    | No             | Create a new user account     |
| POST   | /api/auth/login       | No             | Authenticate, returns JWT      |

**POST /api/auth/register** — body:
```json
{ "email": "user@example.com", "password": "...", "displayName": "..." }
```
Errors: `400` if the email is already registered.

**POST /api/auth/login** — body:
```json
{ "email": "user@example.com", "password": "..." }
```
Errors: `401` on unknown email or wrong password (same message for both, 
so the API doesn't reveal which emails are registered).

Both endpoints return `200` with the same body (`AuthResponseDto`), so a 
user is logged in immediately after registering:
```json
{ "token": "eyJ...", "userId": "...", "displayName": "...", "role": "user" }
```

## Users

| Method | Endpoint         | Auth required        | Description              |
|--------|-------------------|------------------------|----------------------------|
| GET    | /api/users/me     | Yes (logged-in user)  | Get current user's profile |
| PUT    | /api/users/me     | Yes (logged-in user)  | Update current user's profile |

## Listings

| Method | Endpoint              | Auth required            | Description                          |
|--------|------------------------|----------------------------|----------------------------------------|
| GET    | /api/listings           | No                         | List/search listings (paginated, filterable) |
| GET    | /api/listings/{id}      | No                         | Get a single listing's details        |
| POST   | /api/listings           | Yes (logged-in user)      | Create a new listing                  |
| PUT    | /api/listings/{id}      | Yes (owner or admin)      | Update a listing                      |
| DELETE | /api/listings/{id}      | Yes (owner or admin)      | Delete a listing                      |

**GET /api/listings** — query params:
- `category` (optional)
- `condition` (optional)
- `minPrice`, `maxPrice` (optional)
- `search` (optional, matches title/description)
- `status` (optional, default: `available` only)
- `page`, `pageSize`

Response:
```json
{
  "data": [ { "id": "...", "title": "...", "price": 100.0, "category": "Guitars", "status": "available" } ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 143,
  "totalPages": 8
}
```

**POST /api/listings** — body:
```json
{
  "title": "...",
  "description": "...",
  "price": 100.0,
  "category": "Guitars",
  "condition": "Good"
}
```
Note: images are uploaded separately (see below) after the listing is 
created, or in a combined multipart request — final decision to confirm 
during implementation.

## Images

| Method | Endpoint                          | Auth required        | Description                     |
|--------|-------------------------------------|------------------------|------------------------------------|
| POST   | /api/listings/{id}/images           | Yes (owner)            | Upload an image (max 5 per listing) |
| DELETE | /api/listings/{id}/images/{imageId} | Yes (owner)            | Delete a specific image           |

## Authorization summary

- **Public** (no auth): browsing/searching listings, viewing a single listing
- **Logged-in user**: create listings, view/edit own profile
- **Owner only**: edit/delete own listings and manage their images
- **Admin**: can edit/delete any listing (see 03-UserStories.md → Roles & moderation)

Ownership is checked in the **service layer** by comparing the listing's 
`UserId` with the authenticated user's id (read from the JWT), so that 
guessing a listing ID is not enough (NFR-2). A non-owner gets `403`; a 
missing listing gets `404`.

## Error responses (standard shape)

```json
{ "error": "message describing what went wrong" }
```

Common status codes used: `400` (validation error), `401` (not 
authenticated), `403` (not authorized for this action), `404` (resource 
not found).