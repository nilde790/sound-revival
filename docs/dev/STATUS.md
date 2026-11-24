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

**Phase 2 — Delivery pipeline** (🚧 item 1 of 8 in progress) · branch 
`feature/core-infrastructure` (not pushed)

### Work in progress — Core + Infrastructure split (ADR-011)

Resume here. Steps of the refactor, each ending green (0 warnings, all 
tests passing):

| Step | State |
|---|---|
| 0 · branch + baseline (220 tests) | ✅ |
| 1 · `SoundRevival.Repository` → `SoundRevival.Infrastructure` | ✅ `972633c` |
| 2 · `SoundRevival.Core` created; `Entities/`, `Exceptions/`, `Interfaces/` moved | ✅ `6e0cb9f` |
| 3 · `23505` translated in `UserRepository` → `UniqueConstraintViolationException` (Core, not an `AppException`); `AuthService` catches it and throws `BusinessRuleException`; no EF/Npgsql left in `AuthService` or its tests | ✅ `54548c6` |
| 3e · A10: `IPasswordHasher` (Core) + `BCryptPasswordHasher` (Infrastructure, `sealed`), injected in `AuthService`, registered as **singleton** in `Program.cs`; `AuthServiceTests` use a fake hasher (`FakeHash`), new `BCryptPasswordHasherTests` (223 tests) | ✅ |
| 4 · move `AuthService`, `ListingService` to `Core/Services` (`JwtTokenService`, `JwtSettings`, `BCryptPasswordHasher` stay); Core references only Dto | ✅ |
| 5 · `SoundRevival.Tests` → `SoundRevival.UnitTests` (folder, `.csproj`, `.slnx`, namespaces) | ✅ |
| 6 · `Infrastructure/DependencyInjection.cs` → `AddInfrastructure(services, configuration)` (DbContext + fail-fast connection string, repositories, token service, hasher, `JwtSettings`); `Program.cs` grouped (infrastructure / core / web); Npgsql removed from WebApi (A5 closed), `EntityFrameworkCore.Design` stays for `dotnet ef` | ✅ |
| 7 · full check (2026-11-24): build 0 warnings, 223 tests; `has-pending-model-changes` → no changes (old CLR names in the snapshot are harmless); `migrations list` → `InitialCreate` applied; app starts on https :7196 (DI validated on build); curl: register 200, duplicate 400, login 200 (real BCrypt), wrong password 401, create listing 201 + `Location`; no errors in the log | ✅ |
| **8 · docs: 06-Architecture (backend), CLAUDE.md (commands, layers), README, 10-Testing, ENGINEERING-STANDARDS (ARCH-7 → "interface + registration with the right lifetime", not always `AddScoped`), STATUS (A1, A10, A5)** | ⬜ **next** |

**Next action: step 8** (docs), then PR into `main`. Baseline: 0 
warnings, 223 tests green. Docs still naming `SoundRevival.Tests` 
(06-Architecture, 09-Roadmap, 10-Testing, ADR-011, sections of this 
file describing past work) are updated in step 8.

Notes collected during the refactor:
- Migration files keep the old CLR names as strings 
  (`"SoundRevival.Repository.Entities.User"`) on purpose; checked in 
  step 7 with `has-pending-model-changes`.
- The `23505` filter now lives in `UserRepository`: covered only by the 
  phase 2 integration tests (T4), not by unit tests.
- Optional test hardening: `RegisterAsync_OtherSaveError_IsNotTranslated` 
  uses `InvalidOperationException`, which `AuthService` also throws 
  itself; assert the message or use a type the service never throws.
- For step 8 (ADR-011 vs code): the ADR says Infrastructure depends 
  on Core only, but its `.csproj` also references Dto because 
  `ListingRepository` takes `ListingFilterRequestDto` (A2); the ADR's 
  BCrypt open point is now decided (`IPasswordHasher`).
- Found in 3e review, to decide (proposed debt row, SEC): BCrypt uses 
  only the first 72 bytes of a password, while `RegisterRequestDto` 
  allows 128 characters (up to more bytes in UTF-8): longer passwords 
  are silently truncated. Options: lower the DTO limit, or a byte check 
  in the hasher.
- Trailing whitespace and unused `using`s left in touched files: 
  `dotnet format` handles them in the build-quality item (B3).
- Use `dotnet test` without `--no-build` after a change: with a failed 
  build, `--no-build` runs the old test assembly. Save all in Visual 
  Studio before running commands from the terminal.

### Coherence check (2026-11-23)

On 2026-11-23 the whole project was checked for coherence (docs vs. 
code, phase vs. phase, current practice). Outcome: docs fixed, phase 2 
plan revised, two decisions taken before starting: 
[ADR-011](../adr/ADR-011-Core-Infrastructure-Split.md) (Core + 
Infrastructure split, separate integration test project) and 
[ADR-012](../adr/ADR-012-Browser-Token-Storage.md) (access token in 
memory, refresh token in an `HttpOnly` cookie; token refresh moves to 
phase 3).

Phase 1 — Backend foundations ✅ closed on 2026-11-23: 
[PR #3](https://github.com/nilde790/sound-revival/pull/3) merged into 
`main` (merge commit `6cf99d7`).

## 2. Endpoints

| Endpoint | State |
|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | ✅ implemented, validated, unit tested, checked manually (2026-11-16) |
| `GET /api/listings`, `GET /api/listings/{id}` | ✅ implemented, checked manually, service unit tested; filter values and `sort` validated (2026-11-23) |
| `DELETE /api/listings/{id}`, `PATCH /api/listings/{id}/status` | ✅ implemented, checked manually, service unit tested |
| `PUT /api/listings/{id}` | ✅ implemented, checked manually, service unit tested |
| `POST /api/listings` | ✅ implemented, checked manually, service unit tested |
| `GET/PUT /api/users/me` | ⬜ phase 3 |
| `POST/DELETE /api/listings/{id}/images…` | ⬜ phase 3 |
| `GET /health/live`, `GET /health/ready` | ⬜ phase 2 |
| `POST /api/auth/refresh`, `POST /api/auth/logout` | ⬜ phase 3 (ADR-012) |

## 3. What's built

Everything below is on `main`. 220 unit tests, all green; build 
warning-free.

**Phase 0** (PR #1): entities `User`/`Listing`/`Image`, `AppDbContext`, 
`InitialCreate` migration; JWT register/login with BCrypt; 
`UserRepository`. Build warning-free thanks to two documented pins 
(`Microsoft.OpenApi` 2.7.5 for NU1903, 
`Microsoft.EntityFrameworkCore.Relational` 10.0.9 for MSB3277).

**Phase 1** (PR #3, from `feature/CRUDlistings`), step by step:
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
- ✅ **Step 8 — unit tests** (191 tests at the end of the step). 
  Committed: overflow fix `1bdf78d`, `ListingService` tests `20dcc6a`, 
  validation tests `82b425d`:
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
- ✅ **Step 9 — filter values and `sort`** (C14 + C11 closed; 218 
  tests, all green, build warning-free):
  - `ListingFilterRequestDto` (`a65c050`): category/condition limit 
    150 → 50 (same as create); `Status` and new `Sort` are `[Required]` 
    with defaults (`available`, `newest`): an omitted parameter keeps the 
    default, an empty one binds `null` → `400`. Tidy-ups done (typo, 
    redundant `is not null`, unused `using`s).
  - `ListingValues`: `SortNewest`, `SortPriceAsc`, `SortPriceDesc` + 
    `Sorts` set (kept `const`, so they work as switch patterns).
  - `ListingService.CheckFilterValues` (`31f5eac`), first line of 
    `GetListingsAsync` (fail fast, no query on bad input): optional 
    category/condition checked only when present, status and sort 
    always; exact match, no trim → `BusinessRuleException` 400.
  - `ListingRepository`: switch expression on `Sort`, every branch with 
    `ThenBy(Id)`; unknown value → `ArgumentOutOfRangeException` (500, a 
    service bug). A regression (`newest` ascending) was caught in review, 
    not by tests: the repository is mocked (T4).
  - Tests (`e69d56a`): filter DTO boundaries moved to 50/51, `Sort` 
    length, `Required` (`null`/`""`/spaces), defaults; service: valid 
    combinations reach the repository, invalid category/condition/
    status/sort throw with `Verify(..., Times.Never)` on the repository.
  - Checked against PostgreSQL (2026-11-23): `ORDER BY` in the EF log for 
    all three sorts, prices ascending with `price_asc`, 400s without a DB 
    query.
- ✅ **Step 10 — branch review** (2026-11-23, `d01fc13`): typos fixed; 
  same-status PATCH is now a real no-op (early return after ownership 
  and status checks, nothing saved, `UpdatedAt` untouched; 2 tests). 
  Email normalization vs pre-existing mixed-case rows: local DB 
  checked, none; the S1 migration must also lowercase stored emails 
  (noted in S1). A7 and A9 left as debt.
- `Brand` is deliberately absent from the DTOs: the column arrives with 
  the phase 3 migration (S1).

**Baseline before phase 2** (measured 2026-11-23): clean build with 0 
warnings, 220/220 tests green, no vulnerable packages (transitive 
included). Not yet clean: 66 whitespace violations from 
`dotnet format --verify-no-changes` (B3), SDK not pinned and patch 
versions behind (B6).

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
| C12 🟡 | `Create` and `Update` listing DTOs duplicate the same rules (chosen for clarity): any rule change, e.g. adding `Brand`, must be made in both | `CreateListingRequestDto`, `UpdateListingRequestDto` | DRY |
| S1 🟠 | Schema changes pending: column limits, normalized email (include a data update `lower(trim(email))`, after checking for collisions, so accounts stored before normalization can still log in), `Brand` column (05-Database.md) | entities, migrations | DATA-5 |
| C13 🟡 | The JWT payload carries the role under the long `ClaimTypes.Role` URI, not `role`: ASP.NET reads it, but the React frontend would have to decode that key (phase 3, e.g. `JsonWebTokenHandler` + `SecurityTokenDescriptor`, and check `RoleClaimType`) | `JwtTokenService` | — |
| C15 🟡 | Enum-like input normalized inconsistently: `status` in the PATCH body is trimmed (`" sold "` accepted), while query filters must match exactly. One rule for body and query (phase 3 housekeeping) | `ListingService` | VAL-3 |
| C8 🟡 | Unknown-email login skips `BCrypt.Verify`: timing reveals registered emails | `AuthService.LoginAsync` | SEC-6 |
| C9 🟡 | No CORS policy | `Program.cs` | SEC-8 |
| C10 🟡 | `UseHttpsRedirection` with an HTTP-only launch profile | `Program.cs` | CFG-2 |

**Architecture**

| ID | Issue | Where | Rule |
|---|---|---|---|
| A1 🟠 | `SoundRevival.Repository` also holds entities, services, DbContext: misleading name, and services can reference EF Core/Npgsql (`AuthService` does). Decided: split into Core + Infrastructure, first item of phase 2 | solution | ADR-011 |
| A2 🟠 | Repository interface takes HTTP DTOs; since step 9 the repository also switches on API strings (`"price_asc"`). Fix: a repository query type (e.g. `ListingQuery` + `ListingSort` enum) mapped by the service (phase 3 housekeeping; the query type belongs in Core, ADR-011) | `IListingRepository`, `ListingRepository` | ARCH-6 |
| A4 🟡 | Per-repository `SaveChangesAsync` returns an unchecked `bool` and hides the shared Unit of Work | repositories, `AuthService` | ARCH-4, ERR-5 |
| A7 🟡 | `ListingService` sets `UpdatedAt` from the injected `TimeProvider`, but `CreatedAt` still comes from entity initializers (`DateTime.UtcNow` in `Listing`, `User`), `AuthService` sets `CreatedAt` and `JwtTokenService` the expiry with `DateTime.UtcNow`: not one place, not testable (phase 3 housekeeping) | entities, `AuthService`, `JwtTokenService` | DATA-4, TEST-5 |
| A9 🟡 | `AppException` responses (404/403/400) lack the `type` and `traceId` fields the framework adds to the 500: two slightly different `ProblemDetails` shapes. Option: write them with `IProblemDetailsService` (phase 3 housekeeping) | `GlobalExceptionHandler` | ERR-4 |
| A8 🟡 | No `CancellationToken` propagation (phase 3 housekeeping) | repositories, services, controllers | ASYNC-2 |

**Tests**

| ID | Issue | Where | Rule |
|---|---|---|---|
| T4 🟡 | No integration tests yet (phase 2). The repository query (filters, `sort`, paging) is only checked manually: a regression in the `ORDER BY` passed all unit tests | `SoundRevival.Tests` | TEST-6 |

**Build & tooling** (phase 2)

| ID | Issue | Rule |
|---|---|---|
| B1 🟡 | Warning-free not enforced (`TreatWarningsAsErrors`) | STYLE-2 |
| B2 🟡 | No Central Package Management; versions drift (`JwtBearer` 10.0.10 vs 10.0.9) | STYLE-2 |
| B3 🟡 | No `.editorconfig` / `dotnet format`; unused `using`s; 66 whitespace violations today | STYLE-3 |
| B4 🟡 | No OpenAPI UI (Scalar) nor Bearer scheme | — |
| B6 🟡 | No `global.json`: the SDK is not pinned (the dev machine has only SDK 11.0.100, CI would pick its own); patch versions behind (EF Core 10.0.9 → 10.0.15, JwtBearer 10.0.10 → 10.0.15, Microsoft.OpenApi 2.7.5 → 2.13.1, Npgsql 10.0.3 → 10.0.4) | STYLE-2, SEC-7 |
| T5 🟡 | Test stack on xUnit v2 + VSTest + coverlet; .NET 11 templates default to xUnit v3 + Microsoft.Testing.Platform | — |

## 5. Next steps (phase 2)

Phase 1 is done: steps 1–10 in §3, merged via PR #3. Phase 2 is 
in progress (items in branch order in 
[09-Roadmap.md → Phase 2](../09-Roadmap.md#phase-2--delivery-pipeline-walking-skeleton)). 
The docs branch was merged ([PR #5](https://github.com/nilde790/sound-revival/pull/5)).

1. **Finish the Core + Infrastructure split** (ADR-011, A1) on 
   `feature/core-infrastructure`: resume from "Work in progress" in §1 
   (step 3e tests, then steps 4–8), then PR into `main`.
2. Then build quality (B1–B3, B6) → test platform (T5) → CI → 
   integration tests (T4), so every later PR is checked automatically.
3. `14-DevOps.md` and the deployment ADR come right before the deploy 
   item, with the constraints listed in the roadmap.

Git: one short `feature/<name>` branch per roadmap item, PR into `main` 
every few days, merge commit. Delete `feature/CRUDlistings` on GitHub 
(still on `origin`) and locally once no longer needed.

Working split agreed: Claude writes repetitive code/tests and proposes 
that the developer write each piece that introduces something new.

Working mode: `/learn-code` — the developer writes the code, Claude 
explains, gives hints on other examples and reviews each file against the 
standards.

---
*Last updated: 2026-11-24 — PR #5 (coherence check, ADR-011, ADR-012) 
merged; Core + Infrastructure split steps 0–3 committed on 
`feature/core-infrastructure`, plus step 3e (password hasher, A10 
closed, 223 tests), step 4 (services in Core), step 5 
(`SoundRevival.UnitTests`); step 6 (`AddInfrastructure`, A5 closed) 
and step 7 (full check, all green). Next: step 8 (docs) in §1, then 
PR.*
