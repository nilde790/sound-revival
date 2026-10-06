# Requirements

## Functional Requirements

### Authentication & User Management
- **FR-1**: A user must be able to register with email and password
- **FR-2**: A user must be able to log in with email and password
- **FR-3**: A user must be able to log out
- **FR-4**: A user must be able to view and edit their own profile 
  (e.g. display name, contact email)
- **FR-5**: Passwords must be stored securely (hashed, never in plain text)

### Listings
- **FR-6**: A logged-in user must be able to create a new listing with: 
  title, description, category, price, condition, and up to 5 images
- **FR-7**: A user must be able to edit or delete their own listings
- **FR-8**: A listing must have a status: `available` or `sold`
- **FR-9**: A user must be able to mark their own listing as `sold`
- **FR-10**: Sold listings are excluded from default search results 
  (`GET /api/listings` defaults to `status=available`, see 
  07-API-Design.md), but remain reachable by direct link and are 
  clearly marked as sold

### Search & Browsing
- **FR-11**: Any visitor (logged in or not) must be able to view the 
  homepage with a list of available listings
- **FR-12**: Any visitor must be able to filter listings by category
- **FR-13**: Any visitor must be able to search listings by keyword 
  (matching title and/or description)
- **FR-14**: Any visitor must be able to filter listings by price range
- **FR-15**: Any visitor must be able to view a single listing's full 
  details, including seller's contact information

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

### Security
- **NFR-1**: All authenticated endpoints must reject requests without a 
  valid session/token
- **NFR-2**: A user must not be able to edit or delete another user's 
  listing, even by guessing the listing ID
- **NFR-3**: User passwords must never appear in logs or API responses

### Performance
- **NFR-4**: The homepage listing query must remain responsive with at 
  least a few hundred sample listings (realistic seed data volume for a 
  portfolio demo, not production scale)

### Usability
- **NFR-5**: The application must be usable on both desktop and mobile 
  browser widths (responsive design)

### Maintainability
- **NFR-6**: The codebase must separate concerns clearly between backend, 
  frontend, and database layers, following the structure defined in 
  06-Architecture.md
- **NFR-7**: Every architectural decision with meaningful trade-offs must 
  be documented as an ADR

### Deployment
- **NFR-8**: The application must be deployable via Docker 
  (docker-compose.yml at the root) to ensure reproducibility

### CI/CD
- **NFR-9**: Every push/pull request must trigger an automated pipeline 
  that runs linting and tests before allowing merge to `main`
- **NFR-10**: The pipeline must be configured via GitHub Actions, stored 
  in `.github/workflows/`, consistent with the repository structure

## Out of scope (v1)

See `01-Vision.md` → *"What this project is NOT"* and `09-Roadmap.md` for 
features deferred to future iterations (real payments, shipping, 
real-time messaging).