# SoundRevival — Project Status

> **Purpose of this document**: a single, self-contained snapshot of the
> whole project (design decisions + current implementation state) meant
> to be pasted into a *new* Claude chat instead of attaching the full
> `docs/` folder. It should always answer "what is this project, and
> where are we right now?" Update it at the end of every work session
> (or ask Claude to do it) so it never drifts from reality.
>
> Full rationale/history lives in `docs/` (numbered docs + `adr/`) and in
> the ADRs — this file is the **compressed, current-state** view, not a
> replacement for them.

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

| Layer | Choice |
|---|---|
| Backend | C# / ASP.NET Core Web API, controller-based (net10.0) |
| ORM | EF Core, **Code First** + Migrations |
| Database | PostgreSQL 16 (Docker), UUID primary keys everywhere |
| Auth | JWT (HMAC-SHA256), BCrypt password hashing |
| Frontend | React + Vite + TypeScript (**not started yet**) |
| Frontend state | React Query (server state) + Context (auth) + `useState` (local UI) |
| Image storage | Cloudinary — only the URL is persisted in `images` (**not started**) |
| Containerization | Docker Compose (Postgres now; backend/frontend to be added) |
| Testing | xUnit + Moq (backend unit — **AuthService done**), EF Core + real Postgres container (backend integration — **not started**), Vitest + RTL (frontend — not started) |
| CI/CD | GitHub Actions, `.github/workflows/` — **folder exists, empty** |

Key ADRs (see `docs/adr/`): separate C# backend + React frontend
(ADR-001) · PostgreSQL + UUID PKs over SERIAL (ADR-002) · JWT over
server sessions (ADR-003).

## 3. Backend architecture

Multi-project solution, physically separated layers (not just folders):
SoundRevival.WebApi → Controllers, HTTP concerns, auth middleware. No business logic.
SoundRevival.Repository → Interfaces/ (contracts, e.g. IUserRepository, IAuthService)
Services/ (business logic + EF Core access)
Repository/ (EF Core implementations)
Entities/ (User, Listing, Image)
SoundRevival.Dto → request/response DTOs, decoupled from entities
SoundRevival.Tests → xUnit + Moq, targets Repository layer via interfaces


Dependency direction: `WebApi → Repository (via interfaces) + Dto` ·
`Repository → Dto`. Controllers never touch the DB directly.

**JWT middleware** (`Program.cs`): `AddAuthentication` + `AddJwtBearer`
configured and validated; `app.UseAuthentication()` before
`app.UseAuthorization()`, before `MapControllers()`. Verified end-to-end
via a temporary `[Authorize]` test endpoint (now removed): 401 without
token, 200 with valid token and correct claims read back.

## 4. Domain model (compact)

User (1) ──< Listing (many) ──< Image (many, max 5 per listing)


- **User**: id, email (unique), passwordHash, displayName, role
  (`user`|`admin`, default `user`), createdAt
- **Listing**: id, userId, title, description, price, category (enum),
  condition (enum), status (`available`|`sold`), createdAt, updatedAt
- **Image**: id, listingId, url (Cloudinary), displayOrder

Category/Condition are fixed enums validated at app level (not tables)
— category = Guitars/Keyboards/Drums/Wind/Strings/Other; condition =
New/Like New/Good/Fair/Poor. Max-5-images is a business rule enforced
in the service layer, not a DB constraint.

## 5. API surface (planned contract — see `docs/07-API-Design.md` for full detail)

| Method | Endpoint | Auth | Status |
|---|---|---|---|
| POST | `/api/auth/register` | No | ✅ implemented + unit tested |
| POST | `/api/auth/login` | No | ✅ implemented + unit tested |
| GET/PUT | `/api/users/me` | Yes | ⬜ not started |
| GET | `/api/listings` (paginated, filterable) | No | ⬜ not started |
| GET | `/api/listings/{id}` | No | ⬜ not started |
| POST | `/api/listings` | Yes | ⬜ not started |
| PUT/DELETE | `/api/listings/{id}` | Owner/admin | ⬜ not started |
| POST/DELETE | `/api/listings/{id}/images...` | Owner | ⬜ not started |

Base path `/api`, plural kebab-case resources, verbs express action,
pagination via `page`/`pageSize` (defaults 1/20), errors as
`{ "error": "..." }`.

## 6. Frontend design (planned, not yet implemented)

Feature-based folder structure (`features/auth`, `features/listings`,
`features/users`, each with `components/hooks/api/types.ts`), plus
`shared/`, `pages/`, `routes/AppRoutes.tsx`. Axios instance with a JWT
interceptor in `shared/lib/axios.ts`. Routing: `/`, `/listings/:id`,
`/listings/new` (auth), `/login`, `/register`, `/profile` (auth), via a
`ProtectedRoute` wrapper. Full detail in `docs/08-Frontend-Design.md`.

## 7. CURRENT STATE — what's actually built

**`feature/auth` branch: complete, tested, and cleaned up.**

- Entities (`User`, `Listing`, `Image`), `AppDbContext`, initial EF Core
  migration applied to Postgres
- Full JWT auth: `AuthService` / `IAuthService` / `AuthController`
  (register + login), BCrypt hashing
- Repository pattern in place: `IUserRepository` / `UserRepository`
  (Moq-testable), registered in `Program.cs` via `AddScoped`
- JWT middleware (`AddAuthentication`/`AddJwtBearer`,
  `UseAuthentication`/`UseAuthorization`) added to `Program.cs` and
  verified end-to-end
- 5 unit tests (xUnit + Moq) on `AuthService`, all passing:
  `LoginAsync` (valid credentials / user not found / wrong password),
  `RegisterAsync` (email already exists / successful registration)
- Debug-only code (`IncludeErrorDetails`, auth event logging) and the
  temporary `/api/auth/me` diagnostic endpoint have been removed
- Register + login manually re-verified via Postman; DB rows verified
  via DBeaver

**PR status**: a PR (`feature/auth → main`) was opened and the earlier
mergeability/CI block was checked and resolved — **confirm the merge
itself has actually been completed on GitHub before starting Listings**,
since the chat history doesn't show an explicit "merged" confirmation.

**Not started**: Listings CRUD, Image upload/Cloudinary, any
frontend code, CI pipeline content, deployment, backend integration
tests (EF Core + real Postgres container — infra not yet set up).

## 8. Known open items / technical debt

- Integration test infrastructure (e.g. Testcontainers + real Postgres)
  not yet set up — needed for the "integration" half of the testing
  strategy in `10-Testing.md`; can be introduced now (quick win on Auth)
  or deferred to Listings (more to actually test)
- NU1903 security warning on `Microsoft.OpenApi` v2.0.0 — deferred to a
  dedicated commit, not yet done
- Possible `dotnet-ef` global tool ↔ project EF Core version mismatch
  warnings (cosmetic so far)
- Confirm the `feature/auth → main` merge is actually completed on
  GitHub

## 9. Roadmap — next in sequence

1. Confirm `feature/auth → main` merge is complete
2. NU1903 fix (dedicated commit)
3. Decide: set up integration test infra now, or defer to Listings
4. Listings CRUD: `IListingRepository`/service, DTOs, controller
   (`POST/GET/PUT/DELETE /api/listings`), JWT owner-only authorization
5. Image upload via Cloudinary
6. React frontend (Vite + TS) — auth flow, homepage, listing detail,
   create/edit form, profile page
7. Populate GitHub Actions CI (`.github/workflows/`)
8. Deployment (backend+DB, frontend) — platform TBD

v2 (explicitly deferred): in-app messaging, simulated payments, admin
moderation dashboard, dynamic categories. Explicitly out of scope
forever: real payments, real shipping, multi-language.

## 10. Working conventions

- Git: one feature branch per feature, conventional commits (`feat:`,
  `docs:`, `chore:`), merged into `main` via GitHub PRs (not CLI) for
  portfolio-visible process
- New Claude chat per feature/topic, to control context/token usage —
  **this document is the intended per-chat context payload**
- Docs-first: all `docs/0X-*.md` + ADRs were written before
  implementation started
- Verification ritual after significant changes: Postman (API) +
  DBeaver (DB state) before moving to the next feature
- Unit tests (xUnit + Moq) written per service as logic is completed,
  following the Arrange-Act-Assert pattern, mocking repository
  interfaces rather than hitting a real DB
- Tools: Visual Studio + PowerShell, DBeaver, Postman, Docker Desktop
  (WSL2) + Compose, Git/GitHub
- `appsettings.Development.json` is gitignored — must be recreated by
  hand on every new machine (connection string + JWT key); same for
  the ASP.NET Core HTTPS dev cert (`dotnet dev-certs https --trust`)
  and the global `dotnet-ef` tool

---
*Last updated: reflects state as of 2026‑10‑04. Regenerate/update this
file whenever the project state changes meaningfully — outdated status
here is worse than no status at all.*