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
| `GET /api/listings`, `GET /api/listings/{id}` | ✅ implemented and checked manually (uncommitted); unit tests missing |
| `POST`, `PUT`, `DELETE /api/listings/{id}`, `PATCH /api/listings/{id}/status` | ✅ implemented and checked manually (uncommitted); unit tests missing |
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
- ✅ **Step 1 — repository** (`6a7905f`): `IListingRepository` / 
  `ListingRepository` (get by id tracked, filtered + paginated list 
  ordered newest first with id tiebreaker, add, remove, save); 
  `PagedResult<T>`.
- ✅ **Step 2 — DTOs and validation** (`8576ddd`), all in 
  `SoundRevival.Dto`, validated with DataAnnotations (ADR-010):
  - requests: `CreateListingRequestDto`, `UpdateListingRequestDto` (same 
    rules as create, duplicated on purpose), `UpdateListingStatusRequestDto`, 
    `PaginationRequestDto` (`page` 1–10,000, `pageSize` 1–100), 
    `ListingFilterRequestDto` (+ `IValidatableObject` for 
    `minPrice ≤ maxPrice`);
  - responses: `ListingSummaryResponseDto` (list item), 
    `ListingDetailResponseDto` with nested `SellerResponseDto` (email 
    omitted when null) and `ListingImageResponseDto`; 
    `PagedResponseDto<T>` (computed `TotalPages`);
  - custom attribute `Validation/MaxDecimalPlacesAttribute`.
- ✅ Documentation review (standards, ADR-004..010, roadmap, learning 
  path) committed in `b909472`.
- ✅ **Step 3a — error handling infrastructure** (ADR-009, 
  `027a09f`): abstract `AppException` (message + `StatusCode`) 
  with `NotFoundException` 404, `ForbiddenException` 403, 
  `BusinessRuleException` 400, `InvalidCredentialsException` 401 
  (`SoundRevival.Repository/Exceptions`); `GlobalExceptionHandler` 
  (`SoundRevival.WebApi/ExceptionHandling`) writes `ProblemDetails` as 
  `application/problem+json` for `AppException`, returns `false` for 
  anything else (framework → opaque `500`, logged); registered with 
  `AddExceptionHandler` + `AddProblemDetails` + `UseExceptionHandler` 
  (before authentication). Verified end to end with a temporary endpoint 
  (404, 403, opaque 500), then removed.
- ✅ **Step 3 — `ListingService`** (`027a09f`): `IListingService` with the current user passed as parameters 
  (`Guid userId`, `bool isAdmin`; `Guid? userId` for the detail, email 
  shown only when authenticated). All six methods: list (paged summary), 
  detail, create, update, delete, change status. Private helpers 
  `MapToDetails`, `CheckCategoriesAndConditions`, `CheckOwnership` 
  (owner or admin, else 403). Strings trimmed into locals, `UpdatedAt` 
  set on update/status, `SaveChangesAsync` result checked (false → 
  `InvalidOperationException` → 500), created/updated listing reloaded 
  with seller and images before mapping.
  - Repository: new `GetListingDetailByIdAsync` (`AsNoTracking` + 
    `Include` User and Images) for reads; `GetListingByIdAsync` stays 
    tracked for update/delete/status.
  - `SoundRevival.Dto/Constants/ListingValues`: allowed `Categories`, 
    `Conditions`, `Statuses` (`IReadOnlySet<string>`, exact match).
  - Status transitions decided and documented in 07-API-Design.md 
    (`sold` ↔ `available` both allowed, same status is a no-op).
- ✅ **Step 4 — `ListingsController`** (**uncommitted**): 
  `IListingService` registered with `AddScoped`; all six endpoints, thin 
  (no `try/catch`), explicit `[Authorize]`/`[AllowAnonymous]`, `201` + 
  `Location` via `CreatedAtAction`, `204` for delete/status. Current user 
  via `WebApi/Extensions/ClaimsPrincipalExtensions`: `GetUserIdOrNull()` 
  (anonymous allowed) and `GetUserId()` (`[Authorize]` only, throws 
  `InvalidOperationException` → 500 if the claim is missing); admin via 
  `User.IsInRole("admin")`. Checked manually with Postman: 200, 201, 204, 
  400, 401, 403 (other user's listing), 404.
  - Fixes found while testing: pagination parameter renamed `pagination` 
    (a parameter named `page` switched model binding to prefix mode and 
    ignored `page`/`pageSize`); `[Range(typeof(decimal), …)]` now uses 
    `ParseLimitsInInvariantCulture` (on an Italian machine `"0.01"` threw 
    → 500 on every create). New rule VAL-8.
  - 9 listing read requests added to `SoundRevival.WebApi.http`.
- `Brand` is deliberately absent from the DTOs: the column arrives with 
  the phase 3 migration (S1).

## 4. Technical debt

Each item names the rule it breaks in 
[ENGINEERING-STANDARDS.md](ENGINEERING-STANDARDS.md) (the *why* is 
there). The developer fixes them; Claude Code flags open items in code 
being touched. Remove a row when fixed. 
🔴 bug/real risk · 🟠 will grow · 🟡 minor

**Correctness & security**

| ID | Issue | Where | Rule |
|---|---|---|---|
| C5 🟠 | No rate limiting on login/register | `AuthController` | SEC-5 |
| C6 🟠 | Search term not escaped in `ILIKE` (`%`, `_` act as wildcards) | `ListingRepository` | VAL-6 |
| C11 🟠 | Default order is now newest first with id tiebreaker; the `sort` parameter (`price_asc`, `price_desc`, other values → 400) is still missing from the filter and the query | `ListingFilterRequestDto`, `ListingRepository` | DATA-2 |
| C12 🟡 | `Create` and `Update` listing DTOs duplicate the same rules (chosen for clarity): any rule change, e.g. adding `Brand`, must be made in both | `CreateListingRequestDto`, `UpdateListingRequestDto` | DRY |
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
| A7 🟡 | `UpdatedAt` now set by `ListingService` on update/status, but timestamps still come from entity defaults (`DateTime.UtcNow` initializers), not one place | entities, services | DATA-4 |
| A9 🟡 | `AppException` responses (404/403/400) lack the `type` and `traceId` fields the framework adds to the 500: two slightly different `ProblemDetails` shapes. Option: write them with `IProblemDetailsService` | `GlobalExceptionHandler` | ERR-4 |
| A8 🟡 | No `CancellationToken` propagation | repositories, services, controllers | ASYNC-2 |

**Tests**

| ID | Issue | Where | Rule |
|---|---|---|---|
| T3 🟡 | Folder `Mock/` is misleading (test names are fixed in `AuthServiceTests`) | `SoundRevival.Tests` | TEST-7 |
| T4 🟡 | No integration tests yet (phase 2) | `SoundRevival.Tests` | TEST-6 |

**Build & tooling** (phase 2)

| ID | Issue | Rule |
|---|---|---|
| B1 🟡 | Warning-free not enforced (`TreatWarningsAsErrors`) | STYLE-2 |
| B2 🟡 | No Central Package Management; versions drift (`JwtBearer` 10.0.10 vs 10.0.9) | STYLE-2 |
| B3 🟡 | No `.editorconfig` / `dotnet format`; unused `using`s | STYLE-3 |
| B4 🟡 | No OpenAPI UI (Scalar) nor Bearer scheme | — |

## 5. Next steps (phase 1, in order)

Study first: [LEARNING-PATH.md](LEARNING-PATH.md) → Phase 1 (1.2, 1.3 
before the error handling step).

1. ~~Listing DTOs + DataAnnotations~~ ✅ done
2. ~~D1 + error handling infrastructure (ADR-009)~~ ✅ done
3. ~~`ListingService`~~ ✅ done; `sort` (C11) still open
4. ~~`ListingsController`~~ ✅ done and committed, C1 closed
5. ~~`Roles` / `ListingValues` constants (B5)~~ ✅ done (uncommitted)
6. ~~Migrate auth to ADR-009 (E1)~~ ✅ done (uncommitted; not yet 
   checked manually with Postman)
7. Fix touched debt: ~~C2, C3~~ ✅ done (uncommitted; email normalized 
   with `Trim().ToLowerInvariant()`, unique violation `23505` → 
   `BusinessRuleException`, 5 new tests, T1 closed); ~~C7~~ ✅ done 
   (uncommitted; `JwtSettings` + Options with `ValidateOnStart`, 
   `AuthService` and `JwtBearer` use it; app start not yet checked 
   manually); C6, A3 open
8. Unit tests for `ListingService` (above all `CheckOwnership`: owner, 
   admin, stranger; invalid category/condition/status; 404s; save 
   called), `MaxDecimalPlacesAttribute` and the DTO annotations 
   (`Validator.TryValidateObject`); fix `AuthServiceTests` (T1–T3)

Working mode: `/learn-code` — the developer writes the code, Claude 
explains, gives hints on other examples and reviews each file against the 
standards.

Then phase 2 (CI, integration tests, Docker, backend deploy) — see the 
roadmap.

---
*Last updated: 2026-11-15 — `ListingsController` committed; B5 closed 
(`Roles.Admin`/`Roles.User`, `ListingValues.Available`/`Sold`), not 
committed yet; next: commit, auth migration (E1).*
