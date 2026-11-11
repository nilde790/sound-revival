# SoundRevival — Project Status

> **Purpose of this document**: a single, self-contained snapshot of
> **where the project is right now** (what's built, what's next, known
> debt). It is referenced from `CLAUDE.md`, so Claude Code reads it at
> the start of a session; it can also be pasted into a plain Claude chat.
> Update it at the end of every work session so it never drifts from
> reality — outdated status here is worse than no status at all.
>
> Stable rules and conventions live in `CLAUDE.md`; full design rationale
> lives in the numbered `docs/` files and in `docs/adr/`.

---

## 1. What this is

**SoundRevival** — a vertical marketplace for buying/selling *used
musical instruments* between private users and small shops (vs. generic
marketplaces where instrument listings drown among unrelated
categories).

- **Framing**: technical portfolio case study, not a commercial product.
  Goal = demonstrate full-stack competence (auth, relational data,
  REST API, frontend, testing, deployment) with professional practices
  (ADRs, docs-first, CI/CD, PRs).
- **Solo project.** Developer: Alessandro (Italian, learns by writing
  the code himself with guidance — wants reasoning, not finished code).
- Repo: `github.com/nilde790/sound-revival`

## 2. Tech stack

| Layer | Choice | State |
|---|---|---|
| Backend | C# / ASP.NET Core Web API, controller-based (net10.0) | ✅ in use |
| ORM | EF Core 10, **Code First** + Migrations, Npgsql | ✅ in use |
| Database | PostgreSQL 16 (Docker), UUID primary keys everywhere | ✅ in use |
| Auth | JWT (HMAC-SHA256), BCrypt password hashing | ✅ in use |
| Backend tests | xUnit + Moq (unit) · Testcontainers + Postgres (integration) | unit ✅ · integration ⬜ |
| Frontend | React + Vite + TypeScript, React Query, Axios, React Router | ⬜ not started |
| Image storage | Cloudinary — only the URL is persisted in `Images` | ⬜ not started |
| Containerization | Docker Compose | Postgres only |
| CI/CD | GitHub Actions in `.github/workflows/` | ⬜ not created yet |

Key ADRs (see `docs/adr/`): separate C# backend + React frontend
(ADR-001) · PostgreSQL + UUID PKs over SERIAL (ADR-002) · JWT over
server sessions (ADR-003).

## 3. Backend architecture (compact)

```
SoundRevival.WebApi      → Controllers, Program.cs (DI, JWT middleware). No business logic.
SoundRevival.Repository  → Entities/, AppDbContext, Migrations/,
                           Interfaces/ (IUserRepository, IListingRepository, IAuthService),
                           Repository/ (EF Core data access),
                           Services/ (business logic, depends on repo interfaces)
SoundRevival.Dto         → request/response DTOs, decoupled from entities
SoundRevival.Tests       → xUnit + Moq, services tested with mocked repositories
```

Dependency direction: `WebApi → Repository (via interfaces) + Dto` ·
`Repository → Dto`. Controllers never touch the DB directly.

**Reading the authenticated user's id in a controller** (decided):
`AuthService` writes the id in the `sub` claim; `MapInboundClaims` is
left at its default (`true`), so ASP.NET renames it to
`ClaimTypes.NameIdentifier`. Use
`User.FindFirstValue(ClaimTypes.NameIdentifier)` — `User.FindFirst("sub")`
returns `null`. Role is in `ClaimTypes.Role`, so `[Authorize(Roles = "admin")]`
works out of the box.

## 4. Domain model (compact)

```
User (1) ──< Listing (many) ──< Image (many, max 5 per listing)
```

- **User**: id, email (unique), passwordHash, displayName, role
  (`user`|`admin`, default `user`), createdAt
- **Listing**: id, userId, title, description, price, category (enum),
  condition (enum), brand (optional free text — decided 2026-11-11, not
  migrated yet, see S1), status (`available`|`sold`), createdAt, updatedAt
- **Image**: id, listingId, url (Cloudinary), displayOrder

Category/Condition are fixed enums stored as text and validated at app
level (not tables) — category = Guitars / Keyboards / Drums / Wind
Instruments / Strings / Other; condition = New / Like New / Good / Fair /
Poor. Max-5-images is a business rule enforced in the service layer.

## 5. API surface (see `docs/07-API-Design.md` for full contracts)

| Method | Endpoint | Auth | Status |
|---|---|---|---|
| POST | `/api/auth/register` | No | ✅ implemented + unit tested |
| POST | `/api/auth/login` | No | ✅ implemented + unit tested |
| GET/PUT | `/api/users/me` | Yes | ⬜ not started |
| GET | `/api/listings` (paginated, filterable) | No | 🚧 **current** |
| GET | `/api/listings/{id}` | No | 🚧 **current** |
| POST | `/api/listings` | Yes | 🚧 **current** |
| PUT/DELETE | `/api/listings/{id}` | Owner/admin | 🚧 **current** |
| PATCH | `/api/listings/{id}/status` | Owner/admin | 🚧 **current** (mark as sold, FR-9) |
| POST/DELETE | `/api/listings/{id}/images...` | Owner | ⬜ after core CRUD |

## 6. CURRENT STATE — what's actually built

**`main`**: `feature/auth` complete, tested and merged.

- Entities (`User`, `Listing`, `Image`), `AppDbContext`, `InitialCreate`
  migration applied to Postgres
- JWT auth: `AuthService` / `IAuthService` / `AuthController`
  (register + login), BCrypt hashing
- Repository pattern: `IUserRepository` / `UserRepository`, registered
  in `Program.cs` via `AddScoped`
- 5 unit tests on `AuthService` (xUnit + Moq), all passing
- Build is warning-free: direct pins on `Microsoft.OpenApi` 2.7.5
  (NU1903, GHSA-v5pm-xwqc-g5wc) and `Microsoft.EntityFrameworkCore.Relational`
  10.0.9 (MSB3277 version conflict pulled in by Npgsql)

**Current branch: `feature/CRUDlistings`** — Listings CRUD in progress.

- ✅ Committed (`feat: add listing repository with filtering and
  pagination`): `IListingRepository` / `ListingRepository` (get by id,
  filtered + paginated list with `AsNoTracking`, add, remove, save),
  registered with `AddScoped`; `PagedResult<T>` (items + total count,
  repository → service), `PaginationRequestDto`,
  `ListingFilterRequestDto`
- 🚧 Uncommitted: `CreateListingRequestDto`, `ListingResponseDto`,
  `UpdateListingRequestDto` (empty shell). The contract in
  `docs/07-API-Design.md` was extended on 2026-11-11: `brand` in
  create/update/response, and `images` + `seller` (email only for
  authenticated requests) in the detail response — the DTOs don't have
  these yet
- Decided: no `UpdateListing` repository method (EF change tracking on
  the entity loaded by `GetListingByIdAsync`, which therefore must stay
  tracked); listing status changes via a dedicated
  `PATCH /api/listings/{id}/status`, so `UpdateListingRequestDto` has no
  `Status`
- Earlier housekeeping on this branch: docs aligned with the code,
  `CLAUDE.md` added, `.http` file rewritten for the auth endpoints

## 7. Known open items / technical debt

Found in the technical audit of 2026-11-11 plus earlier notes. Each item
names the rule it breaks in [ENGINEERING-STANDARDS.md](ENGINEERING-STANDARDS.md),
where the *why* is explained. The developer fixes these; when code being
touched has an open item, Claude Code points it out (Review duty in
`/CLAUDE.md`). Remove a row when it is fixed.

**Correctness & security** (🔴 bug/real risk · 🟠 will grow · 🟡 minor)

| ID | Issue | Where | Rule |
|---|---|---|---|
| C1 🔴 | `pageSize` unbounded (`?pageSize=10000000` loads the table); `page ≤ 0` makes `Skip` negative → 500 | `PaginationRequestDto`, `ListingRepository.GetListingsAsync` | VAL-2 |
| C2 🔴 | E-mail compared case-sensitively: `Mario@x.it` and `mario@x.it` can be two accounts; login fails on case mismatch | `UserRepository.FindUserByEmail`, unique index on `Users.Email` | VAL-3 |
| C3 🔴 | Registration is check-then-insert: two concurrent requests → unique-index violation → unhandled `DbUpdateException` (500) | `AuthService.RegisterAsync` | CONC-1, ERR-3 |
| C4 🔴 | No input validation on request DTOs (empty e-mail/password/display name, negative price accepted). Mechanism decided: DataAnnotations (ADR-010) | all request DTOs | VAL-1 |
| E1 🟠 | Auth code uses the old error pattern: `AuthService` throws built-in exceptions (`InvalidOperationException`, `UnauthorizedAccessException`) and `AuthController` maps them with `try/catch` to `{ "error" }`. Migrate to custom exceptions + global handler + `ProblemDetails` (ADR-009) | `AuthService`, `AuthController` | ERR-2, ERR-6 |
| C5 🟠 | No rate limiting on login/register (brute force) | `AuthController` | SEC-5 |
| C6 🟠 | Search term not escaped in `ILIKE` (`%`, `_` act as wildcards) | `ListingRepository.GetListingsAsync` | VAL-6 |
| C7 🟠 | JWT settings read with `config["…"]!` and `int.Parse`: missing config crashes late with an unclear error | `Program.cs`, `AuthService.GenerateAuthResponse` | CFG-1 |
| C8 🟡 | Unknown-e-mail login skips `BCrypt.Verify` → faster response reveals which e-mails exist | `AuthService.LoginAsync` | SEC-6 |
| C9 🟡 | No CORS policy (blocks the React frontend on another origin) | `Program.cs` | SEC-8 |
| C10 🟡 | `UseHttpsRedirection` enabled while the default launch profile is HTTP-only | `Program.cs`, `launchSettings.json` | CFG-2 |
| S1 🟠 | Schema changes pending: columns are unbounded `text` / `numeric`, email not normalized, new `Brand` column (decided 2026-11-11); targets in `docs/05-Database.md` need a migration | entities, `AppDbContext`, migrations | DATA-5 |
| C11 🟠 | Default listing order is by title; the contract says newest first, with `sort` options and an id tiebreaker | `ListingRepository.GetListingsAsync` | DATA-2 |

**Architecture**

| ID | Issue | Where | Rule |
|---|---|---|---|
| A1 🟠 | Project `SoundRevival.Repository` contains entities, services, interfaces, DbContext and migrations: the name describes only data access. Rename/split is a design decision to discuss (ADR) | solution structure | — (to be decided) |
| A2 🟠 | Repository interface takes HTTP DTOs (`ListingFilterRequestDto`, `PaginationRequestDto`); `PagedResult` lives in `Dto` | `IListingRepository` | ARCH-6 |
| A3 🟠 | JWT generation inside `AuthService` (two responsibilities; forces `IConfiguration` mocks in tests) | `AuthService` | ARCH-5 |
| A4 🟡 | `SaveChangesAsync` on each repository returns a `bool` nobody checks, and hides that it commits the whole shared DbContext | `UserRepository`, `ListingRepository`, `AuthService.RegisterAsync` | ARCH-4, ERR-5 |
| A5 🟡 | `Program.cs` references Npgsql directly to register the DbContext (a registration extension method in the Repository project would keep the composition root clean) | `Program.cs` | CONSIDER |
| A7 🟡 | `UpdatedAt` is never maintained automatically; timestamp defaults duplicated in entity initializers and service | entities, services | DATA-4 |
| A8 🟡 | No `CancellationToken` propagation in async methods | repositories, services, controllers | ASYNC-2 |

**Tests**

| ID | Issue | Where | Rule |
|---|---|---|---|
| T1 🔴 | `RegisterAsync_UserCreated_Returnresponse` only asserts `NotNull`: it doesn't verify `Role == "user"` (FR-17) nor that the password is hashed (FR-5) | `AuthServiceTests` | TEST-1 |
| T2 🟠 | `IConfiguration` mock setup copy-pasted in all 5 tests | `AuthServiceTests` | TEST-4, CFG-1 |
| T3 🟡 | Test names don't state the behaviour (`ReturnException`, typo `Returnresponse`); folder named `Mock/` | `SoundRevival.Tests/Mock/` | TEST-3, TEST-7 |
| T4 🟡 | No integration tests (EF queries, HTTP pipeline) — planned: `WebApplicationFactory` + Testcontainers | `SoundRevival.Tests` | TEST-6 |

**Build & tooling**

| ID | Issue | Rule |
|---|---|---|
| B1 🟡 | "Warning-free" is not enforced: no `Directory.Build.props` with `TreatWarningsAsErrors` | STYLE-2 |
| B2 🟡 | No Central Package Management; versions already drift (`JwtBearer` 10.0.10 vs EF/OpenAPI 10.0.9; Npgsql referenced in two projects) | STYLE-2 |
| B3 🟡 | No `.editorconfig` / `dotnet format`; minor formatting inconsistencies and unused `using`s | STYLE-3 |
| B4 🟡 | OpenAPI document generated but no UI (e.g. Scalar) and no Bearer security scheme | — (DX) |
| B5 🟡 | Repeated magic strings for domain values (`"available"`, `"user"`) | STYLE-4 |

## 8. Next steps (in order)

Current phase: **Phase 1 — Backend foundations** of
`docs/09-Roadmap.md` (concepts to study first: `docs/dev/LEARNING-PATH.md`
→ Phase 1). Detail of the work in progress:

1. **Listings CRUD** on `feature/CRUDlistings`:
   - ✅ `IListingRepository` / `ListingRepository`
   - 🚧 DTOs under `SoundRevival.Dto/Listings/`: create, response,
     update and status DTOs exist as drafts; paged API response ⬜. Add
     `brand` / `images` / `seller` per the updated contract, and
     DataAnnotations per the field constraints in `07-API-Design.md`
     (D2 ✅ decided: ADR-010)
   - ⬜ Error handling infrastructure (D1 ✅ decided: ADR-009): custom
     exception types + global `IExceptionHandler` + `ProblemDetails`;
     then migrate the auth code (debt E1)
   - ⬜ `IListingService` / `ListingService` (ownership checks, status
     rules) throwing the new exception types
   - ⬜ `ListingsController` with pagination + filters on `GET /api/listings`
   - ⬜ Owner/admin authorization on PUT/DELETE/PATCH status (see §3)
   - ⬜ Unit tests for `ListingService`; fix `AuthServiceTests` (T1–T3)
2. Then **Phase 2 — Delivery pipeline**: CI, integration tests, Docker,
   deploy of the backend ("walking skeleton", decided 2026-11-11).

Everything after that — phases 3–6 and v2 (ADR-008) — is planned in
`docs/09-Roadmap.md`, the single source for the plan.

---
*Last updated: 2026-11-11 — technical audit done; documentation review
plan step 1: STATUS moved to `docs/dev/`, `ENGINEERING-STANDARDS.md`
added, review duty in `CLAUDE.md`, audit findings recorded as debt (§7).
Step 6: README rewritten for the portfolio, MIT `LICENSE` added —
documentation review plan complete. Step 5: roadmap reorganized into phases with deploy early (walking
skeleton after phase 2); `LEARNING-PATH.md` added for phases 1–2.
D1 and D2 decided: ADR-009 (custom exceptions + global handler +
ProblemDetails) and ADR-010 (DataAnnotations) accepted; auth migration
tracked as E1. Step 3: ADR template upgraded; ADR-004..008 record past decisions;
ADR-009 (D1) and ADR-010 (D2) written as Proposed. Step 2: docs
contradictions fixed; decided brand field, seller email only
for authenticated users, default sort newest first, `pageSize` max 100
with `400` when out of range; user stories got ids and acceptance criteria.
Earlier the same day: listing repository committed, create/response DTOs
written, status endpoint decided, v2 promoted to committed scope.*
