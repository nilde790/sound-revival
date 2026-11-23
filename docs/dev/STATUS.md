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
| `POST /api/auth/register`, `POST /api/auth/login` | ✅ implemented, validated, unit tested, checked manually (2026-11-16) |
| `GET /api/listings`, `GET /api/listings/{id}` | ✅ implemented, checked manually, service unit tested |
| `DELETE /api/listings/{id}`, `PATCH /api/listings/{id}/status` | ✅ implemented, checked manually, service unit tested |
| `PUT /api/listings/{id}` | ✅ implemented, checked manually, service unit tested |
| `POST /api/listings` | ✅ implemented, checked manually, service unit tested |
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
- ✅ **Step 4 — `ListingsController`** (`ccc76d3`): 
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
- ✅ **Steps 5–7 — constants and auth hardening**:
  - `Roles` / `ListingValues` constants replace literals (`1501028`).
  - Auth migrated to ADR-009 exceptions + global handler (`6df5833`).
  - Email normalized (`Trim().ToLowerInvariant()`), unique violation 
    `23505` → `BusinessRuleException` (`1d7149d`).
  - `JwtSettings` via Options with `ValidateDataAnnotations` + 
    `ValidateOnStart`; `Key` at least 32 chars; `JwtBearer` reads the 
    same validated options (`65aed97`, `2d92b4d`). Checked: an invalid 
    setting stops startup.
  - Auth DTOs annotated per 07-API-Design (login: presence and upper 
    bounds only), `DisplayName` trimmed (`2d92b4d`).
  - Search matched literally: `column.ToLower().Contains(term)`, EF 
    escapes the term (`7647363`; replaces the hand-written escape of 
    `80a1876`). SQL checked in the EF command log.
  - `ITokenService` / `JwtTokenService` (`4091ebb`): `AuthService` no 
    longer reads `JwtSettings`.
  - 14 unit tests (10 `AuthService`, 4 `JwtTokenService`). Auth flow and 
    search checked end to end with curl on 2026-11-16.
- ✅ **Step 8 — unit tests** (191 tests, all green, build 
  warning-free). Committed: overflow fix `1bdf78d`, `ListingService` 
  tests `20dcc6a`; **validation tests uncommitted**:
  - ✅ **`ListingService` complete** 
    (`SoundRevival.Tests/Services/ListingServiceTests.cs`): helpers 
    `CreateListing(ownerId)`, `CreateUpdateRequest()`, 
    `CreateCreateRequest()` (valid defaults, each test changes one 
    field); `FakeTimeProvider` passed to the constructor;
  - `DeleteAsync` complete: stranger 403, owner, admin, 404, save fails 
    (`Verify(..., Times.Never/Once)` on `DeleteListing` and 
    `SaveChangesAsync`);
  - `ChangeStateAsync` complete: 403, 404, save fails, invalid status 
    (`[Theory]`: unknown word, wrong case, empty, spaces only), valid 
    status sets `Status` and exact `UpdatedAt`;
  - `UpdateAsync`: 403, 404, save fails, invalid category / condition 
    (`[Theory]`), happy path (trim/price/`UpdatedAt` asserted on the 
    tracked entity, DTO asserted on a *distinct* reloaded listing with a 
    marker title);
  - `CreateAsync`: invalid category / condition, save fails (no reload), 
    reload returns null, happy path (entity captured with Moq 
    `Callback`; `UserId` from the parameter; reload verified with the 
    captured id);
  - `GetByIdAsync`: 404, email hidden for anonymous / shown for any 
    authenticated viewer, images ordered by `DisplayOrder` (input added 
    out of order, `Select` + `Assert.Equal` on `short[]`);
  - `GetListingsAsync`: mapping to summary, empty page, paging fields 
    (`TotalCount` 23 ≠ 3 items, so `TotalItems` can't come from 
    `Items.Count`).
  - ✅ **Validation tests** (`SoundRevival.Tests/Validation/`):
    - `MaxDecimalPlacesAttributeTests` (valid / invalid 
      `TheoryData<decimal>` incl. `decimal.MinValue`/`MaxValue`, `null`). 
      `MaxValue` exposed a real bug: `number * 10^n` overflowed → 
      `OverflowException` → 500 on user input. Fixed with 
      `Math.Round(number, MaxDecimalPlaces) == number`; new rule 
      **VAL-9** (custom attributes never throw).
    - `ValidationHelper.Validate(object)` → `Validator.TryValidateObject` 
      with `validateAllProperties: true`, returns the result list.
    - One class per request DTO (create, update, status, filter, 
      pagination, register, login). Pattern: valid helper, change one 
      field, `Assert.Single` + `MemberNames` contains `nameof(field)`; 
      each invalid value breaks exactly one rule. Covered: lengths at 
      the boundaries, `Required` with `null`/`""`/`"   "`, ranges, 
      decimals, `MinPrice > MaxPrice` (error on both fields) and the 
      fact that `IValidatableObject.Validate` is skipped when an 
      attribute fails. Tests also pin what the DTO does *not* check 
      (unknown status → service; short password / odd email at login).
    - Decided: `[StringLength]` on `Category`/`Condition` stays even 
      though the service checks the exact value (defense in depth, 
      mirrors the column limit; `[AllowedValues]` would contradict 
      ADR-010).
  - `ListingService` now takes `TimeProvider` (registered as 
    `TimeProvider.System` singleton) for `UpdatedAt`; STYLE-1 and TEST-5 
    updated.
  - Test files moved from `Mock/` to `Services/` (T3 closed).
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
| C11 🟠 | Default order is now newest first with id tiebreaker; the `sort` parameter (`price_asc`, `price_desc`, other values → 400) is still missing from the filter and the query | `ListingFilterRequestDto`, `ListingRepository` | DATA-2 |
| C14 🟠 | List filters `category`, `condition`, `status` are not checked against the allowed values: 07-API-Design says other values → `400`, the code returns `200` with an empty list (`status` can even be `null`). Filter DTO also allows 150 chars for category/condition vs 50 on create | `ListingFilterRequestDto`, `ListingService.GetListingsAsync` | VAL / code ≠ docs |
| C12 🟡 | `Create` and `Update` listing DTOs duplicate the same rules (chosen for clarity): any rule change, e.g. adding `Brand`, must be made in both | `CreateListingRequestDto`, `UpdateListingRequestDto` | DRY |
| S1 🟠 | Schema changes pending: column limits, normalized email, `Brand` column (05-Database.md) | entities, migrations | DATA-5 |
| C13 🟡 | The JWT payload carries the role under the long `ClaimTypes.Role` URI, not `role`: ASP.NET reads it, but the React frontend would have to decode that key (fix before phase 4, e.g. `JsonWebTokenHandler` + `SecurityTokenDescriptor`, and check `RoleClaimType`) | `JwtTokenService` | — |
| C8 🟡 | Unknown-email login skips `BCrypt.Verify`: timing reveals registered emails | `AuthService.LoginAsync` | SEC-6 |
| C9 🟡 | No CORS policy | `Program.cs` | SEC-8 |
| C10 🟡 | `UseHttpsRedirection` with an HTTP-only launch profile | `Program.cs` | CFG-2 |

**Architecture**

| ID | Issue | Where | Rule |
|---|---|---|---|
| A1 🟠 | `SoundRevival.Repository` also holds entities, services, DbContext: misleading name (rename/split needs an ADR) | solution | ADR-004 |
| A2 🟠 | Repository interface takes HTTP DTOs | `IListingRepository` | ARCH-6 |
| A4 🟡 | Per-repository `SaveChangesAsync` returns an unchecked `bool` and hides the shared Unit of Work | repositories, `AuthService` | ARCH-4, ERR-5 |
| A5 🟡 | `Program.cs` references Npgsql directly (registration extension method would be cleaner) | `Program.cs` | CONSIDER |
| A7 🟡 | `ListingService` sets `UpdatedAt` from the injected `TimeProvider`, but `CreatedAt` still comes from entity initializers (`DateTime.UtcNow` in `Listing`, `User`), `AuthService` sets `CreatedAt` and `JwtTokenService` the expiry with `DateTime.UtcNow`: not one place, not testable | entities, `AuthService`, `JwtTokenService` | DATA-4, TEST-5 |
| A9 🟡 | `AppException` responses (404/403/400) lack the `type` and `traceId` fields the framework adds to the 500: two slightly different `ProblemDetails` shapes. Option: write them with `IProblemDetailsService` | `GlobalExceptionHandler` | ERR-4 |
| A8 🟡 | No `CancellationToken` propagation | repositories, services, controllers | ASYNC-2 |

**Tests**

| ID | Issue | Where | Rule |
|---|---|---|---|
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
5. ~~`Roles` / `ListingValues` constants (B5)~~ ✅ done
6. ~~Migrate auth to ADR-009 (E1)~~ ✅ done
7. ~~Fix touched debt: C2, C3, C4, C6, C7, A3~~ ✅ done (details in §3)
8. ~~Unit tests~~ ✅ done (§3). **Commit the validation tests** 
   (suggested: `test: add request DTO validation tests`). Optional 
   tidy-ups in the developer's files: unused `using`s, `errors` → 
   `error` in the first filter test, typo "minimun" and the redundant 
   `is not null` checks in `ListingFilterRequestDto.Validate` (lifted 
   comparison already returns `false` when either side is `null`).
9. Then close phase 1: filter value validation (C14) and `sort` 
   parameter (C11) — same DTO and query, do them together; merge 
   `feature/CRUDlistings` into `main` via PR.

Working split agreed: Claude writes repetitive code/tests and proposes 
that the developer write each piece that introduces something new.

Working mode: `/learn-code` — the developer writes the code, Claude 
explains, gives hints on other examples and reviews each file against the 
standards.

Then phase 2 (CI, integration tests, Docker, backend deploy) — see the 
roadmap.

---
*Last updated: 2026-11-23 — step 8 complete: request DTO validation 
tests (191 tests, all green, build warning-free), uncommitted; new debt 
C14 (filter values not validated, code ≠ 07-API-Design). Next: commit, 
then C14 + C11, then PR to `main`.*
