# SoundRevival — Project Status

> Snapshot of **where the project is now**: what's built, what's in 
> progress, known debt, next steps. Read at the start of every session; 
> updated at the end. Everything else is linked, not repeated: plan → 
> [`../09-Roadmap.md`](../09-Roadmap.md), rules → 
> [ENGINEERING-STANDARDS.md](ENGINEERING-STANDARDS.md), stack and layers 
> → [`../06-Architecture.md`](../06-Architecture.md), domain → 
> [`../04-DomainModel.md`](../04-DomainModel.md), decisions → 
> [`../adr/`](../adr/).

Solo portfolio and learning project; the developer writes all the code, 
with AI guidance and review. Repo: `github.com/nilde790/sound-revival`.

## 1. Current phase and branch

**Phase 1 — Backend foundations** · branch `feature/CRUDlistings`

## 2. Endpoints

| Endpoint | State |
|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | ✅ implemented, unit tested (old error pattern, debt E1) |
| `GET /api/listings`, `GET /api/listings/{id}` | 🚧 repository done |
| `POST`, `PUT`, `DELETE /api/listings/{id}`, `PATCH /api/listings/{id}/status` | 🚧 repository done |
| `GET/PUT /api/users/me` | ⬜ phase 3 |
| `POST/DELETE /api/listings/{id}/images…` | ⬜ phase 3 |
| `GET /health` | ⬜ phase 2 |

## 3. What's built

**On `main`**: entities `User`/`Listing`/`Image`, `AppDbContext`, 
`InitialCreate` migration; JWT register/login with BCrypt; 
`UserRepository`; 5 unit tests on `AuthService`. Build warning-free 
thanks to two documented pins (`Microsoft.OpenApi` 2.7.5 for NU1903, 
`Microsoft.EntityFrameworkCore.Relational` 10.0.9 for MSB3277).

**On `feature/CRUDlistings`**:
- ✅ Committed: `IListingRepository` / `ListingRepository` (get by id 
  tracked, filtered + paginated list, add, remove, save); 
  `PagedResult<T>`, `PaginationRequestDto`, `ListingFilterRequestDto`; 
  documentation review (standards, ADR-004..010, roadmap, learning path).
- 🚧 Uncommitted drafts: `CreateListingRequestDto`, `UpdateListingRequestDto`, 
  `UpdateListingStatusRequestDto`, `ListingResponseDto`. Still missing per 
  the contract in 07-API-Design.md: `brand`, `images`, `seller`, 
  DataAnnotations.

## 4. Technical debt

Each item names the rule it breaks in 
[ENGINEERING-STANDARDS.md](ENGINEERING-STANDARDS.md) (the *why* is 
there). The developer fixes them; Claude Code flags open items in code 
being touched. Remove a row when fixed. 
🔴 bug/real risk · 🟠 will grow · 🟡 minor

**Correctness & security**

| ID | Issue | Where | Rule |
|---|---|---|---|
| C1 🔴 | `pageSize` unbounded; `page ≤ 0` makes `Skip` negative → 500 | `PaginationRequestDto`, `ListingRepository` | VAL-2 |
| C2 🔴 | Email compared case-sensitively: duplicate accounts possible, login fails on case mismatch | `UserRepository.FindUserByEmail` | VAL-3 |
| C3 🔴 | Check-then-insert registration: concurrent requests → unhandled unique violation (500) | `AuthService.RegisterAsync` | CONC-1, ERR-3 |
| C4 🔴 | No input validation on request DTOs (DataAnnotations, ADR-010) | all request DTOs | VAL-1 |
| E1 🟠 | Auth uses built-in exceptions + controller `try/catch` + `{ "error" }`; migrate to ADR-009 | `AuthService`, `AuthController` | ERR-2, ERR-6 |
| C5 🟠 | No rate limiting on login/register | `AuthController` | SEC-5 |
| C6 🟠 | Search term not escaped in `ILIKE` (`%`, `_` act as wildcards) | `ListingRepository` | VAL-6 |
| C7 🟠 | JWT settings read with `config["…"]!` / `int.Parse`: late, unclear crash if missing | `Program.cs`, `AuthService` | CFG-1 |
| C11 🟠 | Default order is by title; contract says newest first, `sort` options, id tiebreaker | `ListingRepository` | DATA-2 |
| S1 🟠 | Schema changes pending: column limits, normalized email, `Brand` column (05-Database.md) | entities, migrations | DATA-5 |
| C8 🟡 | Unknown-email login skips `BCrypt.Verify`: timing reveals registered emails | `AuthService.LoginAsync` | SEC-6 |
| C9 🟡 | No CORS policy | `Program.cs` | SEC-8 |
| C10 🟡 | `UseHttpsRedirection` with an HTTP-only launch profile | `Program.cs` | CFG-2 |

**Architecture**

| ID | Issue | Where | Rule |
|---|---|---|---|
| A1 🟠 | `SoundRevival.Repository` also holds entities, services, DbContext: misleading name (rename/split needs an ADR) | solution | ADR-004 |
| A2 🟠 | Repository interface takes HTTP DTOs | `IListingRepository` | ARCH-6 |
| A3 🟠 | JWT generation inside `AuthService` | `AuthService` | ARCH-5 |
| A4 🟡 | Per-repository `SaveChangesAsync` returns an unchecked `bool` and hides the shared Unit of Work | repositories, `AuthService` | ARCH-4, ERR-5 |
| A5 🟡 | `Program.cs` references Npgsql directly (registration extension method would be cleaner) | `Program.cs` | CONSIDER |
| A7 🟡 | `UpdatedAt` not maintained; timestamp defaults duplicated | entities, services | DATA-4 |
| A8 🟡 | No `CancellationToken` propagation | repositories, services, controllers | ASYNC-2 |

**Tests**

| ID | Issue | Where | Rule |
|---|---|---|---|
| T1 🔴 | Registration test only asserts `NotNull`: role `user` and password hashing unverified | `AuthServiceTests` | TEST-1 |
| T2 🟠 | `IConfiguration` mock setup copy-pasted in 5 tests | `AuthServiceTests` | TEST-4 |
| T3 🟡 | Names don't state behaviour (`ReturnException`); folder `Mock/` | `SoundRevival.Tests` | TEST-3, TEST-7 |
| T4 🟡 | No integration tests yet (phase 2) | `SoundRevival.Tests` | TEST-6 |

**Build & tooling** (phase 2)

| ID | Issue | Rule |
|---|---|---|
| B1 🟡 | Warning-free not enforced (`TreatWarningsAsErrors`) | STYLE-2 |
| B2 🟡 | No Central Package Management; versions drift (`JwtBearer` 10.0.10 vs 10.0.9) | STYLE-2 |
| B3 🟡 | No `.editorconfig` / `dotnet format`; unused `using`s | STYLE-3 |
| B4 🟡 | No OpenAPI UI (Scalar) nor Bearer scheme | — |
| B5 🟡 | Repeated magic strings (`"available"`, `"user"`) | STYLE-4 |

## 5. Next steps (phase 1, in order)

Study first: [LEARNING-PATH.md](LEARNING-PATH.md) → Phase 1 (1.2, 1.3 
before the error handling step).

1. Finish listing DTOs: contract fields + DataAnnotations (C4)
2. Error handling infrastructure (ADR-009), then migrate auth (E1)
3. `ListingService`: ownership/admin, allowed values, status rules
4. `ListingsController` (CRUD + `PATCH /status`)
5. Fix touched debt: C1, C2, C3, C6, C7, C11, A3
6. Unit tests for `ListingService`; fix `AuthServiceTests` (T1–T3)

Then phase 2 (CI, integration tests, Docker, backend deploy) — see the 
roadmap.

---
*Last updated: 2026-11-11 — documentation review complete; docs slimmed 
(single source per topic).*
