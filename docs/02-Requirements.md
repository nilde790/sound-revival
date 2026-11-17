# Requirements

Requirement ids are stable: they are referenced by user stories 
([03-UserStories.md](03-UserStories.md)), API contracts, tests and PRs. 
A requirement that is dropped keeps its id and is marked *Removed*. 
New requirements get the next free id.

## Functional Requirements

### Authentication & User Management
- **FR-1**: A user must be able to register with email and password
- **FR-2**: A user must be able to log in with email and password
- **FR-3**: A user must be able to log out. With stateless JWT 
  authentication (ADR-003) logout happens **on the client**, which 
  discards the token. The server does not revoke tokens in v1: a token 
  stays valid until it expires (`Jwt:ExpiryMinutes`). Server-side 
  revocation comes with token refresh in v2
- **FR-4**: A user must be able to view and edit their own profile 
  (display name). The email is both the login identity and the contact 
  address (see 04-DomainModel.md); whether it can be changed is decided 
  when the profile endpoints are designed
- **FR-5**: Passwords must be stored securely (hashed, never in plain text)

### Listings
- **FR-6**: A logged-in user must be able to create a new listing with: 
  title, description, category, condition, price and an optional brand
- **FR-7**: A user must be able to edit or delete their own listings
- **FR-8**: A listing must have a status: `available` or `sold`
- **FR-9**: A user must be able to mark their own listing as `sold` 
  (dedicated endpoint `PATCH /api/listings/{id}/status`, see 
  07-API-Design.md; in v2 a completed simulated payment will also set it)
- **FR-10**: Sold listings are excluded from default search results 
  (`GET /api/listings` defaults to `status=available`, see 
  07-API-Design.md), but remain reachable by direct link and are 
  clearly marked as sold
- **FR-19**: The owner of a listing must be able to attach up to 5 
  images to it and delete them. The upload flow (separate endpoint after 
  creation vs. a combined multipart request) is decided when image 
  upload is implemented (see 07-API-Design.md)

### Search & Browsing
- **FR-11**: Any visitor (logged in or not) must be able to view the 
  homepage with a list of available listings
- **FR-12**: Any visitor must be able to filter listings by category and 
  by condition
- **FR-13**: Any visitor must be able to search listings by keyword 
  (matching title and/or description)
- **FR-14**: Any visitor must be able to filter listings by price range
- **FR-15**: Any visitor must be able to view a single listing's full 
  details, including the seller's display name. The seller's contact 
  email is shown **only to authenticated visitors**, to protect sellers 
  from scraping and spam
- **FR-20**: Any visitor must be able to filter listings by brand 
  (case-insensitive, partial match)
- **FR-21**: Listing results must be sortable by newest first (default), 
  price ascending and price descending
- **FR-22**: Listing results must be paginated (`page`, `pageSize`; 
  defaults 1 and 20, `page` 1–10,000 to avoid integer overflow in the 
  offset, maximum page size 100). Out-of-range values are 
  rejected with `400`, not silently corrected

### Categories
- **FR-16**: The system must support a predefined set of instrument 
  categories (e.g. Guitars, Keyboards, Drums, Wind Instruments, Strings, 
  Other)

### Roles
- **FR-17**: Every new user is registered with role `user`; the `admin` 
  role is assigned manually and cannot be chosen at registration
- **FR-18**: An admin must be able to edit or delete any listing, 
  regardless of owner

## Non-Functional Requirements

Each NFR is written so that it can be verified (by a test, a pipeline 
check, a measurement or a review).

### Security
- **NFR-1**: All authenticated endpoints must reject requests without a 
  valid token (`401`)
- **NFR-2**: A user must not be able to edit or delete another user's 
  listing, even by guessing the listing ID (`403`)
- **NFR-3**: User passwords and password hashes must never appear in 
  logs or API responses
- **NFR-11**: Login and registration must be rate limited per client, so 
  that brute-force attempts are slowed down (`429` when exceeded)
- **NFR-12**: All client input (body, query, route) must be validated and 
  bounded before reaching business logic; invalid input returns `400`
- **NFR-13**: Cross-origin requests must be allowed only from the 
  configured frontend origin(s) (CORS)

### Performance
- **NFR-4**: With **1,000 seeded listings**, `GET /api/listings` (default 
  filters, first page) must respond in **under 500 ms at the 95th 
  percentile**, measured locally against the Docker database. This 
  requires a seed dataset (see 09-Roadmap.md)

### Usability & accessibility
- **NFR-5**: The application must be usable on both desktop and mobile 
  browser widths (responsive design, from 360 px wide)
- **NFR-14**: Main flows (browse, view listing, login, create listing) 
  must be usable with the keyboard only, every form field must have a 
  label, and text must meet WCAG 2.1 AA color contrast

### Maintainability
- **NFR-6**: The codebase must separate concerns clearly between backend, 
  frontend, and database layers, following the structure defined in 
  06-Architecture.md and the rules in 
  [dev/ENGINEERING-STANDARDS.md](dev/ENGINEERING-STANDARDS.md)
- **NFR-7**: Every architectural decision with meaningful trade-offs must 
  be documented as an ADR

### Operability
- **NFR-15**: The backend must write structured logs (no secrets or 
  personal data, see NFR-3) and expose a health endpoint that reports 
  whether the API and its database are reachable

### Deployment
- **NFR-8**: The application must be deployable via Docker 
  (docker-compose.yml at the root) to ensure reproducibility

### CI/CD
- **NFR-9**: Every push/pull request must trigger an automated pipeline 
  that must pass before merging to `main`. It runs: backend build with 
  **zero warnings**, `dotnet format --verify-no-changes`, backend tests; 
  and, once the frontend exists, ESLint, the TypeScript type check 
  (`tsc --noEmit`) and frontend tests
- **NFR-10**: The pipeline must be configured via GitHub Actions, stored 
  in `.github/workflows/`, consistent with the repository structure

## Scope

The requirements above are v1. v2 (in-app messaging, simulated payments, 
admin moderation dashboard, dynamic categories, token refresh) is 
committed scope after v1; its requirements will be added here when each 
feature is started. Real payments, real shipping and multi-language 
remain out of scope. See `01-Vision.md` and `09-Roadmap.md`.

Exploratory product ideas (trade listings, multi-party trade matching, 
audio demos, smart wishlists, natural-language search, gear value) are 
collected in [09-Roadmap.md → Product ideas](09-Roadmap.md#product-ideas--differentiators-exploratory-not-committed). 
They get requirement ids only if and when they are adopted.
