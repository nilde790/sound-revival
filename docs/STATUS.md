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
                           Interfaces/ (IUserRepository, IAuthService),
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
  condition (enum), status (`available`|`sold`), createdAt, updatedAt
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

**Current branch: `feature/CRUDlistings`** — Listings CRUD, nothing
implemented yet. Housekeeping done on this branch so far: docs aligned
with the code, `CLAUDE.md` added, `.http` file rewritten for the auth
endpoints, stray files/imports removed.

## 7. Known open items / technical debt

- **No input validation** on request DTOs (empty email/password,
  negative price… are accepted). Decide the approach (DataAnnotations
  vs. FluentValidation) before/while writing Listings DTOs.
- **Schema hardening**: columns are unbounded `text` / `numeric`; the
  intended `varchar(n)` / `numeric(10,2)` limits are listed in
  `docs/05-Database.md` and need a dedicated migration.
- `AuthService.RegisterAsync` ignores the `bool` returned by
  `SaveChangesAsync()`.
- `AuthServiceTests` repeats the `IConfiguration` mock setup in every
  test — candidate for a shared helper/constructor.
- Integration test infrastructure (Testcontainers + real Postgres) not
  set up yet — deliberately deferred to Listings.

## 8. Next steps (in order)

1. **Listings CRUD** on `feature/CRUDlistings`:
   - `IListingRepository` / `ListingRepository` (mirror `IUserRepository`)
   - `IListingService` / `ListingService` (ownership checks, status rules)
   - DTOs under `SoundRevival.Dto/Listings/` (create/update request,
     response, paged response)
   - `ListingsController` with pagination + filters on `GET /api/listings`
   - Owner/admin authorization on PUT/DELETE (see §3 for claim reading)
   - Unit tests for `ListingService`; decide on integration test infra
2. Image upload via Cloudinary
3. `GET/PUT /api/users/me`
4. React frontend (Vite + TS)
5. GitHub Actions CI
6. Deployment (backend + DB, frontend) — platform TBD

v2 (explicitly deferred): in-app messaging, simulated payments, admin
moderation dashboard, dynamic categories, token refresh. Out of scope:
real payments, real shipping, multi-language.

---
*Last updated: 2026-10-06 — docs/code alignment pass on
`feature/CRUDlistings`, Listings CRUD not started yet.*
