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

**Phase 2 — Delivery pipeline** (🚧 item 4 of 8 done, PR pending) · 
branch `feature/ci`, draft [PR #9](https://github.com/nilde790/sound-revival/pull/9)

### Resume here — merge the CI PR

Item 4 (GitHub Actions CI) is **complete on the branch** (2026-11-26): 
`build-and-test` green on Ubuntu in about a minute, every guard seen 
failing, Dependabot/CodeQL/secret scanning on, `main` protected by a 
ruleset. Item 3 (test platform) was merged via 
[PR #8](https://github.com/nilde790/sound-revival/pull/8) (`f01d8ba`).

**Next action**: mark PR #9 ready and merge it (merge commit; from now 
on the ruleset enforces it). After the merge: delete the branch, move 
the table below to §3, check Dependabot's first PRs (no major bump of 
`Microsoft.*`/`Npgsql` expected) and CodeQL's first results 
(Security → Code scanning). Then item 5: integration tests (T4); study 
first LEARNING-PATH 2.5.

| Step | State |
|---|---|
| 1 · branch, study questions 2.3/2.4 | ✅ `4900bf5` |
| 2 · `ci.yml` written by the developer: `pull_request` + `push` to `main`, `contents: read`, `ubuntu-latest`, locked restore → Release build → format check → test; .NET 10 runtime installed next to the SDK; first run green (54 s) | ✅ `5cd4820` |
| 3 · actions pinned by SHA (checkout v7.0.1, setup-dotnet v6.1.0), `concurrency` (cancel only PR runs), 15-min timeout; NuGet cache tried and dropped (restore from cache 15 s vs 6–7 s without) | ✅ `5f515db`, `394a645` |
| 4 · coverage in CI + ReportGenerator 5.6.1 (local tool) → job summary; `coverage-report/` gitignored | ✅ `7e802ff` |
| 5 · negative tests via throwaway draft PRs #10–#12: failing test → Test red (exit 2); bad indentation → Check formatting red (WHITESPACE); Moq version changed without lock file → Restore red (NU1004); PRs closed, branches deleted | ✅ |
| 6 · `dependabot.yml` (NuGet in `/` and `/backend`, GitHub Actions, .NET SDK; weekly, minor/patch grouped, majors tied to .NET ignored); dependency graph, Dependabot alerts and security updates, CodeQL default setup enabled; secret scanning + push protection already on | ✅ `2886bb0` |
| 7 · ruleset `Protect main` (PR required, 0 approvals, merge commit only, `build-and-test` required, no force push/deletion, no bypass); docs: 06-Architecture (CI/CD), 10-Testing, ENGINEERING-STANDARDS (new SEC-9, DoD), 09-Roadmap, README (badge), CLAUDE.md, STATUS | ✅ |

Notes from this item:
- YAML structure is indentation: a key 2 spaces off is still valid YAML 
  but means something else (`steps` became a second job; `cache` landed 
  on the step instead of `with:`). Typos in keys (`permission`, 
  `pull request`) are valid YAML too: only GitHub's schema rejects them.
- The runner image already ships SDK 11 and the .NET 10 runtime; the 
  workflow still declares both, so it does not depend on the image.
- Visual Studio regenerates `packages.lock.json` on its own after a 
  `Directory.Packages.props` change: the real mistake is leaving the 
  lock file out of the commit, which the locked restore catches.
- Not adopted on purpose: CI that auto-formats and commits (needs 
  `contents: write`, commits the developer didn't write); PR comment for 
  coverage (needs `pull-requests: write` and a third-party action); 
  CodeQL advanced setup; "require branches to be up to date".
- Dependabot reads `dependabot.yml` only from `main`: its first PRs come 
  after the merge. Unverified: whether its NuGet PRs update the lock 
  files (if not, CI fails them with NU1004).

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

Everything below is on `main`. 223 unit tests, all green; build 
warning-free (enforced from phase 2 item 2, §1).

**Phase 0** (PR #1): entities `User`/`Listing`/`Image`, `AppDbContext`, 
`InitialCreate` migration; JWT register/login with BCrypt; 
`UserRepository`. Build warning-free thanks to two documented pins 
(`Microsoft.OpenApi` 2.7.5 for NU1903, 
`Microsoft.EntityFrameworkCore.Relational` 10.0.9 for MSB3277; both 
removed in phase 2 item 2, no longer needed after the patch updates).

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
  (now `SoundRevival.Core/Exceptions`); `GlobalExceptionHandler` 
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
    (`SoundRevival.UnitTests/Services/ListingServiceTests.cs`): helpers 
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
  - ✅ **Validation tests** (`SoundRevival.UnitTests/Validation/`):
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

**Phase 2, item 1 — Core + Infrastructure split** (ADR-011, 
[PR #6](https://github.com/nilde790/sound-revival/pull/6), merged 
2026-11-24, merge commit `729ad48`): A1, A5, A10 closed.

| Step | State |
|---|---|
| 0 · branch + baseline (220 tests) | ✅ |
| 1 · `SoundRevival.Repository` → `SoundRevival.Infrastructure` | ✅ `972633c` |
| 2 · `SoundRevival.Core` created; `Entities/`, `Exceptions/`, `Interfaces/` moved | ✅ `6e0cb9f` |
| 3 · `23505` translated in `UserRepository` → `UniqueConstraintViolationException` (Core, not an `AppException`); `AuthService` catches it and throws `BusinessRuleException` | ✅ `54548c6` |
| 3e · A10: `IPasswordHasher` (Core) + `BCryptPasswordHasher` (Infrastructure, `sealed`, singleton); `AuthServiceTests` use a fake hasher (`FakeHash`), new `BCryptPasswordHasherTests` (223 tests) | ✅ `a41da96` |
| 4 · `AuthService`, `ListingService` moved to `Core/Services`; Core references only Dto | ✅ `60f241d` |
| 5 · `SoundRevival.Tests` → `SoundRevival.UnitTests` (folder, `.csproj`, `.slnx`, namespaces) | ✅ `fecf933` |
| 6 · `AddInfrastructure(services, configuration)` (DbContext + fail-fast connection string, repositories, token service, hasher, `JwtSettings`); `Program.cs` grouped; Npgsql removed from WebApi (A5 closed), `EntityFrameworkCore.Design` stays for `dotnet ef` | ✅ `474c4eb` |
| 7 · full check (2026-11-24): build 0 warnings, 223 tests; `has-pending-model-changes` → no changes; `migrations list` → `InitialCreate` applied; app starts on https :7196; curl: register 200, duplicate 400, login 200, wrong password 401, create listing 201 + `Location` | ✅ `69ce4fc` |
| 8 · docs: 06-Architecture (backend projects, dependencies, lifetimes), CLAUDE.md and README (EF commands, layers), 05-Database, 10-Testing, 09-Roadmap (item ✅), ADR-011 (implementation notes), ENGINEERING-STANDARDS (ARCH-7 lifetimes, SEC-3 hasher + 72 bytes), STATUS (A1 closed, C16 added) | ✅ `d1a706f` |

Notes from the refactor (still valid):
- Migration files keep the old CLR names as strings 
  (`"SoundRevival.Repository.Entities.User"`) on purpose; harmless, 
  checked with `has-pending-model-changes`.
- The `23505` filter lives in `UserRepository`: covered only by the 
  phase 2 integration tests (T4), not by unit tests.
- Test data left in the local DB by step 7 (user `step7-…@example.test`, 
  listing "Step 7 test guitar"): delete when convenient.
- Use `dotnet test` without `--no-build` after a change: with a failed 
  build, `--no-build` runs the old test assembly. Save all in Visual 
  Studio before running commands from the terminal.
- Docker Desktop must be running before `docker compose up -d`.

**Phase 2, item 2 — Build quality and reproducibility** 
([PR #7](https://github.com/nilde790/sound-revival/pull/7), merged 
2026-11-26, merge commit `e2af4cd`): B1, B2, B3, B6 closed. Final check: 
build 0 warnings with warnings as errors and analyzers, 223/223 tests, 
format clean, locked restore OK, no vulnerable packages, API checked 
against PostgreSQL on https :7196.

| Step | State |
|---|---|
| 1 · branch + baseline: SDK 11.0.100, 0 warnings, 223 tests, 66 format violations, no vulnerable packages | ✅ `703aa92` |
| 2 · `global.json` (11.0.100, `rollForward: latestPatch`); negative test with 11.0.200 fails as expected; file name lowercase (Linux is case-sensitive) | ✅ `5985dbb` |
| 3 · Central Package Management + transitive pinning; direct pins (`Relational`, `Microsoft.OpenApi`) become transitive pins; resolved versions compared before/after: identical | ✅ `792b91d` |
| 4 · patch updates (Microsoft 10.0.15, Npgsql 10.0.4, test packages); both pins removed after a remove-and-observe check (`Microsoft.OpenApi` now resolves to 2.13.0, the 2.7.6 pin would have downgraded it); `Solution Items` folder; `dotnet-ef` 10.0.15 as a local tool (`dotnet-tools.json`), README prerequisites | ✅ `a0ccb60`, `a31e24b`, `ccdde73` |
| 5 · `Directory.Build.props` + `packages.lock.json` per project; negative test: a stale lock file fails locked restore (NU1004) | ✅ `7649510` |
| 6 · `.editorconfig` (file-scoped namespaces, usings, naming, migrations as generated code) + `.gitattributes`; formatting-only commit (59 files, no logic lines changed); `.git-blame-ignore-revs` | ✅ `f9a84f0`, `7f1a6c9`, `b9329ef` |
| 7 · `TreatWarningsAsErrors`, `AnalysisMode` `Recommended`, `EnforceCodeStyleInBuild`; common properties moved from the csproj files; CA1707 off in tests, CA1304/CA1311/CA1862 suppressed on the EF search query, CA2201 fixed; negative tests: unused `using` (IDE0005) and `new Exception()` (CA2201) fail the build | ✅ `6119625` |
| 8 · docs: ENGINEERING-STANDARDS (STYLE-1 file-scoped, STYLE-2/3 enforced, new STYLE-5 suppressions, STYLE-6 reproducible dependencies, DoD), 06-Architecture (build configuration), 09-Roadmap (item ✅), README, CLAUDE.md (commands), STATUS | ✅ `b5867d1`, `e3cd30d` |

Also in this PR: MIT license removed, all rights reserved 
(`f5af89e`).

Notes from this item:
- Adding or updating a package changes `packages.lock.json`: commit it 
  together with `Directory.Packages.props` (CI will fail on NU1004 
  otherwise).
- Every guard was checked with a test that must fail (wrong SDK, stale 
  lock file, unused `using`, analyzer rule): a guard never seen failing 
  may not be active.
- XML comments cannot contain `--`: a malformed props file made restore 
  fail while filtered output hid the error. Check exit codes, not only 
  the output text.
- Not adopted on purpose: `AnalysisMode` `All` (211 warnings, mostly 
  rules that don't fit ASP.NET Core, e.g. CA2007 `ConfigureAwait`); 
  minor/major package upgrades (e.g. `System.IdentityModel.Tokens.Jwt` 
  8.23, .NET 11 packages).

**Phase 2, item 3 — Test platform** 
([PR #8](https://github.com/nilde790/sound-revival/pull/8), merged 
2026-11-26, merge commit `f01d8ba`): T5 closed. Final check: build 0 
warnings, 223/223 tests on xUnit v3 + Microsoft.Testing.Platform (also 
in Visual Studio's Test Explorer), format clean, locked restore OK, 
coverage report generated with migrations excluded.

| Step | State |
|---|---|
| 1 · branch + baseline: 223 tests, coverlet line coverage 43.75% (Core 99.25%, Infrastructure 6% with migrations) | ✅ `f207a5a` |
| 2 · `global.json` opts `dotnet test` into MTP; negative test: the VSTest project is refused (exit 1) | ✅ `6c6ae54` |
| 3–4 · `xunit.v3` 4.0.3 (MTP v2 by default), `OutputType` `Exe`; `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector` removed; CS8625 in 5 `TheoryData` collection expressions fixed with `(string?)null`; Test Explorer checked in VS | ✅ `6c6ae54` |
| 5 · `Microsoft.Testing.Extensions.CodeCoverage` 18.12.0; `coverage.config` excludes migrations (by namespace, so it works on Windows and Linux); `TestResults/` gitignored; line coverage 74% (Core 100%, Infrastructure 18%) | ✅ `7cd6669` |
| 6 · negative tests: a failing test exits 2, zero tests run exits 8, 0 migration classes in the report | ✅ |
| 7 · docs: 10-Testing (platform, exit codes, coverage), ENGINEERING-STANDARDS (new TEST-8, DoD), 06-Architecture, 09-Roadmap (item ✅), LEARNING-PATH 2.2, README, CLAUDE.md (commands), STATUS | ✅ `f2c5543` |

Notes from this item:
- In xUnit v3 `TheoryData<T>` holds rows (`TheoryDataRow<T>`): in a 
  collection expression a bare `null` becomes a null *row*, not a row 
  with a null value. Warnings as errors turned it into a build error 
  (CS8625); write `(string?)null`.
- XML comments cannot contain `--` (second time, after item 2): an 
  invalid `coverage.config` was reported as "does not exist or is not 
  valid" (exit 5).
- Coverage numbers depend on the tool and the exclusions: compare only 
  runs made the same way.
- Not adopted on purpose: keeping the VSTest packages "for 
  compatibility" (Visual Studio runs MTP natively); a coverage threshold 
  (CI will show a summary, not gate on a percentage).

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
| C16 🟡 | BCrypt hashes only the first 72 bytes, but `RegisterRequestDto` allows 128 characters (more bytes in UTF-8): two long passwords sharing the first 72 bytes are equivalent, silently. Options: lower `[StringLength]` to 72 *and* reject passwords over 72 UTF-8 bytes (a custom attribute, VAL-9), or check the byte length in `BCryptPasswordHasher`; mirror the limit in 07-API-Design | `RegisterRequestDto`, `BCryptPasswordHasher` | SEC-3 |
| C8 🟡 | Unknown-email login skips the password verification (`IPasswordHasher.VerifyPassword`): timing reveals registered emails | `AuthService.LoginAsync` | SEC-6 |
| C9 🟡 | No CORS policy | `Program.cs` | SEC-8 |
| C10 🟡 | `UseHttpsRedirection` with an HTTP-only launch profile | `Program.cs` | CFG-2 |

**Architecture**

| ID | Issue | Where | Rule |
|---|---|---|---|
| A2 🟠 | Repository interface takes HTTP DTOs; since step 9 the repository also switches on API strings (`"price_asc"`). Fix: a repository query type (e.g. `ListingQuery` + `ListingSort` enum) mapped by the service (phase 3 housekeeping; the query type belongs in Core, ADR-011) | `IListingRepository`, `ListingRepository` | ARCH-6 |
| A4 🟡 | Per-repository `SaveChangesAsync` returns an unchecked `bool` and hides the shared Unit of Work | repositories, `AuthService` | ARCH-4, ERR-5 |
| A7 🟡 | `ListingService` sets `UpdatedAt` from the injected `TimeProvider`, but `CreatedAt` still comes from entity initializers (`DateTime.UtcNow` in `Listing`, `User`), `AuthService` sets `CreatedAt` and `JwtTokenService` the expiry with `DateTime.UtcNow`: not one place, not testable (phase 3 housekeeping) | entities, `AuthService`, `JwtTokenService` | DATA-4, TEST-5 |
| A9 🟡 | `AppException` responses (404/403/400) lack the `type` and `traceId` fields the framework adds to the 500: two slightly different `ProblemDetails` shapes. Option: write them with `IProblemDetailsService` (phase 3 housekeeping) | `GlobalExceptionHandler` | ERR-4 |
| A8 🟡 | No `CancellationToken` propagation (phase 3 housekeeping) | repositories, services, controllers | ASYNC-2 |

**Tests**

| ID | Issue | Where | Rule |
|---|---|---|---|
| T4 🟡 | No integration tests yet (phase 2). The repository query (filters, `sort`, paging) and the `23505` translation in `UserRepository` are only checked manually: a regression in the `ORDER BY` passed all unit tests | `SoundRevival.Infrastructure` (future `SoundRevival.IntegrationTests`) | TEST-6 |

**Build & tooling** (phase 2)

| ID | Issue | Rule |
|---|---|---|
| B4 🟡 | No OpenAPI UI (Scalar) nor Bearer scheme | — |

## 5. Next steps (phase 2)

Phase 1 is done: steps 1–10 in §3, merged via PR #3. Phase 2 is 
in progress (items in branch order in 
[09-Roadmap.md → Phase 2](../09-Roadmap.md#phase-2--delivery-pipeline-walking-skeleton)). 
The docs branch was merged ([PR #5](https://github.com/nilde790/sound-revival/pull/5)).

1. ✅ Core + Infrastructure split (ADR-011), merged via PR #6.
2. ✅ Build quality (B1–B3, B6), merged via PR #7.
3. ✅ Test platform (T5), merged via PR #8.
4. ✅ GitHub Actions CI, on `feature/ci`: draft PR #9 pending (§1).
5. **Next**: integration tests (T4), as their own CI job.
6. `14-DevOps.md` and the deployment ADR come right before the deploy 
   item, with the constraints listed in the roadmap.

Git: one short `feature/<name>` branch per roadmap item, PR into `main` 
every few days, merge commit; delete the branch (local and `origin`) 
after the merge.

Working split agreed: Claude writes repetitive code/tests and proposes 
that the developer write each piece that introduces something new. 
Mechanical and configuration work (package swaps, `global.json`, 
`.csproj`/props edits, lock files) is done by Claude once the step is 
approved, followed by a summary of what changed and why.

Working mode: `/learn-code` — the developer writes the code, Claude 
explains, gives hints on other examples and reviews each file against the 
standards.

---
*Last updated: 2026-11-26 — phase 2 item 4 (GitHub Actions CI) complete 
on `feature/ci`: workflow, SHA pinning, coverage summary, Dependabot, 
CodeQL, ruleset on `main`. Next: merge draft PR #9 (§1).*
