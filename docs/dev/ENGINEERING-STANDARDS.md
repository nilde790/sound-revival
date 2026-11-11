# Engineering Standards

> **Purpose**: the quality bar every change in SoundRevival is held to.
> It turns "write good code" into concrete, checkable rules, each with
> the reason behind it and an example from this project.
>
> **How it is used**
> - **While writing code**: read the rules for the layer you are
>   touching *before* starting. The [Definition of Done](#definition-of-done)
>   says when a feature is finished.
> - **During review**: Claude Code reviews code proactively, in
>   proportion to the change: the rules of the touched layer for a small
>   edit, the full [Code Review Checklist](#code-review-checklist) at the
>   end of a step (see "Review duty" in `/CLAUDE.md`).
> - **When a mistake slips through**: the fix is not only in the code.
>   Add or sharpen the rule here, so the same mistake is caught next time.
>
> This is a *development guide* (see the documentation lifecycle in
> [`../00-README.md`](../00-README.md)): it evolves as the project grows.
> Known violations in existing code are tracked as technical debt in
> [STATUS.md](STATUS.md) §7, with the rule ID they break.

## Severity levels

Every rule has a level, used in reviews:

| Level | Meaning |
|---|---|
| **MUST** | Breaking it is a bug, a security risk, or a broken contract. A change that violates it is not done. |
| **SHOULD** | The default. Deviate only with a stated reason (in the PR or in a code comment). |
| **CONSIDER** | Good practice whose value depends on context. The reviewer mentions it, and the developer decides. |

Rules that depend on a **design decision not yet taken** say so explicitly
and point to the pending decision. They describe *what* must be true, not
*how*, until the decision is made.

---

## 1. Architecture and layers

Reference: [`../06-Architecture.md`](../06-Architecture.md).

**ARCH-1 (MUST): Controllers are thin.** A controller action reads the
request, gets the current user from the claims, calls one service method,
and maps the result or exception to an HTTP response. Controllers have no
business rules, no `AppDbContext`, and no entities: they receive and
return DTOs only.
*Why*: business rules in controllers can't be unit tested without HTTP,
and they get duplicated across endpoints.

**ARCH-2 (MUST): Business rules live in services.** This includes
ownership/admin checks, allowed values, state transitions and the
max-5-images rule. Services depend on repository **interfaces**, never on
`AppDbContext`.
*Why*: this is what makes services unit-testable with mocks (see TEST-6).

**ARCH-3 (MUST): Repositories do data access only.** They query, add and
remove, with no `if` that encodes a business rule.
*Example*: "only `available` listings by default" is a business default.
The service (or the DTO default) decides it, and the repository just
applies the filter it receives.

**ARCH-4 (MUST): One use case = one `SaveChangesAsync`, called by the
service at the end.** All repositories in a request share the **same
scoped `AppDbContext`**, so `SaveChangesAsync()` on *any* repository
commits *all* pending changes of the request (EF Core's DbContext *is* a
Unit of Work). Never call it in the middle of a use case "just to be
safe", and never assume it only saves "its own" entity.

**ARCH-5 (SHOULD): One responsibility per class.** Infrastructure
concerns go behind their own interface instead of inside a business
service: token generation, current time, external APIs (Cloudinary,
LLMs), e-mail.
*Example*: JWT generation currently lives inside `AuthService` (debt
A3). An `ITokenService` would make `AuthService` testable without mocking
`IConfiguration`.

**ARCH-6 (SHOULD): Don't let a layer's contract leak into another.**
New repository methods should take simple parameters or a query object
owned by the inner layer, not HTTP request DTOs.
*Why*: if the API contract changes (a renamed query parameter), the data
layer should not have to change.
*Known deviation*: `IListingRepository.GetListingsAsync` takes
`ListingFilterRequestDto` (debt A2). This is accepted at the current
scale, so don't copy the pattern.

**ARCH-7 (MUST): Every new service or repository has an interface and an
`AddScoped` registration** in `Program.cs` (or in a registration
extension method, if one is introduced).

**ARCH-8 (CONSIDER): Keep entities simple until behaviour justifies
otherwise.** Public setters and rules in services ("anemic model") are
fine for CRUD. When an entity gets real state transitions (v2: `available
→ reserved → sold`), a domain method such as `listing.MarkAsSold()` that
protects its own invariants becomes worth it.

---

## 2. Input validation

Reference: [`../07-API-Design.md`](../07-API-Design.md),
[`../05-Database.md`](../05-Database.md) (column limits).

> **Mechanism** ([ADR-010](../adr/ADR-010-Input-Validation.md), Accepted):
> shape/format rules are DataAnnotations on the request DTO
> (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`), checked
> automatically by `[ApiController]` before the action runs (`400` +
> `ValidationProblemDetails`). Cross-field rules use
> `IValidatableObject`. Rules that need domain knowledge or data
> (allowed lists, ownership, transitions, duplicates) are checked in the
> service.

**VAL-1 (MUST): Every field of every request DTO has explicit
constraints.** That means required/optional, min/max length, numeric
range and format (e-mail). The limits match the column targets in
`05-Database.md`.
*Example*: `RegisterRequestDto` accepts an empty e-mail, a 1-character
password and an empty display name today (debt C4).

**VAL-2 (MUST): Everything a client can make large is bounded.**
- Pagination: `page ≥ 1`, `1 ≤ pageSize ≤ max`. The max is documented in
  `07-API-Design.md`, typically 100.
- Collections in a request: a max count.
- Free text: a max length.

*Why*: `?pageSize=10000000` makes the server load the whole table, and
`page=0` makes `Skip()` negative, which is a 500 (debt C1). Unbounded
input is the cheapest denial-of-service there is.

**VAL-3 (MUST): Normalize before you compare or store.** Trim strings.
**E-mails are compared case-insensitively**: normalize them (for example
to lowercase) when you store *and* when you look them up, or use a
case-insensitive column.
*Example*: `Mario@x.it` and `mario@x.it` can register as two accounts
today, and login fails if the case doesn't match (debt C2).

**VAL-4 (MUST): Enum-like strings are checked against an allow-list.**
`Category`, `Condition`, `Status` and `Role` are validated in the service
against the lists in [`../04-DomainModel.md`](../04-DomainModel.md). This
also applies to **query filters** (`?status=`, `?category=`), not only
to request bodies.
*Why*: an unchecked value either silently returns nothing (confusing
API) or stores garbage that breaks filters later.

**VAL-5 (MUST): Money is `decimal`, positive, with at most 2 decimals and
an upper bound.** Never `double` or `float`, because binary floating
point can't represent 0.10 exactly.

**VAL-6 (MUST): User text used inside a `LIKE`/`ILIKE` pattern is
escaped.** `%` and `_` are wildcards and `\` is PostgreSQL's default
escape character, so escape all three before wrapping the term in `%…%`.
*Why*: this is not SQL injection (EF Core parameterizes), but searching
for `%` returns everything (debt C6).

**VAL-7 (MUST): Never bind security-relevant fields from the request
body.** The owner id comes **from the JWT claims**, never from the body.
`role`, `status` (outside its dedicated endpoint), `id`, `createdAt` and
`userId` are not properties of request DTOs.
*Why*: this is "mass assignment" / "over-posting", one of the OWASP API
Top 10 risks.

---

## 3. Error handling

Reference: the error mapping in `/CLAUDE.md` and
[`../07-API-Design.md`](../07-API-Design.md).

> **Mechanism** ([ADR-009](../adr/ADR-009-Error-Handling.md), Accepted):
> services throw custom exceptions (not found → 404, forbidden → 403,
> business rule → 400, invalid credentials → 401); one global
> `IExceptionHandler` maps them to RFC 9457 `ProblemDetails`.
> Controllers contain no `try/catch`.

**ERR-1 (MUST): Services signal expected failures with exceptions of a
specific type, and never swallow errors.** Never return `null` or `false`
to mean "it failed" when the caller can't tell why. Never write
`catch (Exception) { }`.

**ERR-2 (MUST): Every exception type a service can throw on purpose has
a mapping in the global exception handler and in `07-API-Design.md`.**
An expected failure that ends up as a `500` is a bug. Services never
throw built-in exceptions (`InvalidOperationException`,
`UnauthorizedAccessException`, `KeyNotFoundException`) to signal an
expected failure: the framework throws those too, so they are left
unmapped and surface as `500`.

**ERR-6 (MUST): No `try/catch` in controllers to produce error
responses.** Mapping happens only in the global handler. A controller
catching exceptions duplicates the handler and can drift from it.

**ERR-3 (MUST): Expected infrastructure exceptions are translated.** The
main case is a unique-constraint violation. EF Core throws
`DbUpdateException` whose inner exception is a `PostgresException` with
`SqlState == PostgresErrorCodes.UniqueViolation` (`"23505"`). Where a
duplicate is a normal situation (e.g. registering an e-mail that already
exists), catch that specific case and turn it into the domain error.
*See also*: CONC-1.

**ERR-4 (MUST): Error responses are safe.** No stack traces, SQL,
connection strings or internal type names in the body. Authentication
failures use the same message whatever the cause.

**ERR-5 (SHOULD): Don't return values nobody checks.** If a method
returns `bool` for success, the caller must check it. Otherwise the
method should return `Task` and throw on failure.
*Example*: `RegisterAsync` ignores the `bool` from `SaveChangesAsync()`.

---

## 4. Concurrency

**CONC-1 (MUST): Check-then-act is not atomic, so the database is the
real guard.** When code checks "does it exist?" and then inserts, two
parallel requests can both pass the check. A **unique constraint** must
exist in the DB, and its violation must be handled (ERR-3). The check in
code is only a nicer error message for the common case.
*Example*: `RegisterAsync` checks the e-mail and then inserts. Two
simultaneous registrations hit the unique index, and the second one
becomes a 500 (debt C3).

**CONC-2 (SHOULD): Concurrent updates to the same record are detected,
not silently overwritten.** For state changes that matter (a listing
going to `sold`, and in v2 a purchase), use **optimistic concurrency**: a
concurrency token, in PostgreSQL typically the `xmin` system column. A
conflicting update then throws `DbUpdateConcurrencyException` instead of
"last write wins". Not required before the status endpoint exists. It
becomes MUST with v2 payments.

**CONC-3 (MUST): Never run parallel operations on the same
`DbContext`.** No `Task.WhenAll` over queries from the same context,
because `DbContext` is not thread-safe.

---

## 5. Async and cancellation

**ASYNC-1 (MUST): Async all the way.** Never use `.Result`, `.Wait()` or
`GetAwaiter().GetResult()` on a task. They block a thread and can
deadlock.

**ASYNC-2 (SHOULD): Public async methods accept a `CancellationToken`**
as the last parameter (`CancellationToken cancellationToken = default`)
and pass it down to EF Core (`ToListAsync(cancellationToken)`,
`SaveChangesAsync(cancellationToken)`). In a controller, add a
`CancellationToken` parameter to the action: ASP.NET Core binds it to the
request being aborted.
*Why*: if the client disconnects, the database query stops instead of
running for nobody (debt A8).

**ASYNC-3 (MUST): Async method names end with `Async`.**
`IUserRepository.FindUserByEmail` is a known exception, so don't copy it.

---

## 6. Data access (EF Core + PostgreSQL)

Reference: [`../05-Database.md`](../05-Database.md).

**DATA-1 (MUST): Read-only queries use `AsNoTracking()`, and entities
loaded for an update stay tracked.** This is the decided pattern: there
is no generic `Update` method, because EF change tracking detects the
modified properties of a tracked entity.

**DATA-2 (MUST): Every list endpoint is paginated and ordered
deterministically.** The ordering ends with a unique column (`ThenBy(l =>
l.Id)`), so pages never overlap or skip rows. The sort order is part of
the API contract and is documented in `07-API-Design.md`.

**DATA-3 (SHOULD): Load only what you need.** Use `Include` on purpose,
never "just in case". For lists, consider projecting to the response
shape with `Select`. Watch for **N+1** queries: one query for the list,
then one per item for a navigation property.

**DATA-4 (MUST): Timestamps are UTC and set in one place.** `CreatedAt`
is set on creation. **`UpdatedAt` is set on every modification.** A
single place for this (the service, or a `SaveChanges` override /
interceptor) avoids forgetting it (debt A7).

**DATA-5 (MUST): Schema changes are Code First migrations.** Change
entities or `OnModelCreating`, add a migration, and review the generated
migration before applying it. Never edit a migration that has already
been applied. Column limits (`HasMaxLength`, `HasPrecision`) live in the
fluent configuration.

**DATA-6 (SHOULD): Indexes follow real query patterns.** Add one when a
query filters or sorts on a column, not speculatively. At the current
data volume (a few hundred rows), `ILIKE '%term%'` needs no optimization.
Measure before optimizing.

---

## 7. Security

Reference: [`../06-Architecture.md`](../06-Architecture.md) (auth),
[ADR-003](../adr/ADR-003-Authentication.md).

**SEC-1 (MUST): Every endpoint states its access level explicitly.**
Each one is `[Authorize]`, `[Authorize(Roles = "admin")]` or
`[AllowAnonymous]`, matching the auth column in `07-API-Design.md`.

**SEC-2 (MUST): Authorization is checked in the service, with the user id
taken from the claims**: `User.FindFirstValue(ClaimTypes.NameIdentifier)`
in the controller, passed to the service. Owner-or-admin checks never
trust ids sent by the client (NFR-2).

**SEC-3 (MUST): Passwords are only ever hashed with BCrypt.** They are
never logged, returned or stored in plain text, and neither are their
hashes. Response DTOs never contain `PasswordHash`.

**SEC-4 (MUST): Secrets never enter the repository.** Connection strings,
JWT keys and API keys stay out of git. Locally they live in the
gitignored `appsettings.Development.json` (current approach) or in
`dotnet user-secrets`. In deployment they come from environment variables
or the platform's secret store.

**SEC-5 (SHOULD): Abuse-prone endpoints are rate limited.** This applies
first of all to login and registration (brute force). ASP.NET Core has a
built-in rate limiter (`AddRateLimiter`). Bounded inputs (VAL-2) are the
other half of abuse protection (debt C5).

**SEC-6 (SHOULD): Don't leak which accounts exist.** Login failures for
"unknown e-mail" and "wrong password" have the same message *and*
similar timing. Today the unknown-e-mail branch skips `BCrypt.Verify`,
so it answers measurably faster (debt C8).

**SEC-7 (MUST): No known-vulnerable dependencies.** NuGet vulnerability
warnings (NU1901–NU1904) are fixed, not suppressed. A direct version pin
used as a fix carries a comment with the advisory id (as done for
`Microsoft.OpenApi`).

**SEC-8 (MUST, before the frontend): CORS allows only known origins.**
Never `AllowAnyOrigin` together with credentials.

---

## 8. Configuration

**CFG-1 (SHOULD): Use strongly-typed settings with the Options pattern.**
Bind configuration sections to classes (`JwtSettings`), validate them at
startup (`ValidateDataAnnotations().ValidateOnStart()`) and inject
`IOptions<T>`. Avoid `configuration["Jwt:Key"]!` and `int.Parse(...)`
scattered through the code.
*Why*: a missing setting stops the app **at startup with a clear
message**, instead of crashing on the first login. Tests become trivial
too: `Options.Create(new JwtSettings { ... })` replaces mocking
`IConfiguration` (debts C7, T2).

**CFG-2 (SHOULD): Environment differences live in configuration, not in
code.** Use `appsettings.{Environment}.json` and environment variables.
Business code never checks `IsDevelopment()`.

---

## 9. Logging

Reference: future `13-Observability.md`.

**LOG-1 (SHOULD): Use `ILogger<T>` with message templates, not string
interpolation.**
`_logger.LogInformation("Listing {ListingId} created by {UserId}", listing.Id, userId);`
*Why*: templates produce **structured** logs (searchable fields), and
interpolation turns them into plain text.

**LOG-2 (SHOULD): Use levels consistently.**
- `Information`: business events (listing created, user registered).
- `Warning`: something unexpected but handled (rejected login, concurrency conflict).
- `Error`: a failed operation that needs attention.
- `Debug`: development detail only.

**LOG-3 (MUST): Never log secrets or personal data in clear.** No
passwords, hashes, tokens, connection strings or full request bodies.
Log ids, not e-mails.

**LOG-4 (SHOULD): Log an unexpected exception once, at the boundary.**
Don't catch-log-rethrow at every layer, which produces the same stack
trace three times.

---

## 10. API design

Reference: [`../07-API-Design.md`](../07-API-Design.md).

**API-1 (MUST): The contract is documented before or together with the
code.** A new or changed endpoint (route, body, query parameters, status
codes) updates `07-API-Design.md` in the same branch.

**API-2 (SHOULD): Use standard status codes.**
- `POST` create: `201 Created`, with a `Location` header (`CreatedAtAction`) and the created resource.
- `GET`/`PUT`: `200`.
- `DELETE` and `PATCH` without a body: `204 No Content`.
- `400` for validation errors, `401` when not authenticated, `403` when not allowed, `404` when the resource is missing.

**API-3 (MUST): Never return entities.** Response DTOs expose only what
the client needs, with no sensitive fields and no navigation graphs.

---

## 11. Testing

Reference: [`../10-Testing.md`](../10-Testing.md).

**TEST-1 (MUST): Every business rule has a test that would fail if the
rule were removed.** Assert the *outcome* (returned values, state passed
to the repository via `Verify(...)` / `It.Is<...>`), not just `NotNull`.
*Example*: `RegisterAsync_UserCreated_Returnresponse` only checks
`NotNull`. It would still pass if the role were `admin` or if the
password were stored in plain text (debt T1).

**TEST-2 (MUST): Every failure branch has a test.** Each `throw` in a
service method has at least one test that triggers it and asserts the
exception type.

**TEST-3 (MUST): One scenario per test, named
`Method_Scenario_ExpectedResult`, structured as Arrange-Act-Assert.** The
expected result names the behaviour: `ThrowsUnauthorizedAccessException`,
`ReturnsTokenForValidCredentials`, not `ReturnException` (debt T3).

**TEST-4 (SHOULD): No copy-pasted setup.** Shared setup goes in the test
class constructor (xUnit creates a new instance per test, so tests stay
isolated) or in small builder/helper methods (debt T2).

**TEST-5 (MUST): Unit tests are fast and deterministic.** No database,
network, file system, real clock or random values that affect
assertions.

**TEST-6 (MUST): Test at the right level.** Services: unit tests with
mocked repository interfaces. EF Core queries and HTTP behaviour
(routing, status codes, auth): **integration tests** against a real
PostgreSQL (planned: `WebApplicationFactory` + Testcontainers). Don't
mock `DbContext` to "test" a LINQ query.

**TEST-7 (SHOULD): Test folders mirror what they test**: `Services/`,
`Integration/`, not by tool (`Mock/`).

---

## 12. Code style and build

**STYLE-1 (MUST): Follow the conventions in `/CLAUDE.md`.** That means
block-scoped namespaces matching folders, `_camelCase` private readonly
fields, constructor injection, `Guid.NewGuid()` in the service and
`DateTime.UtcNow`.

**STYLE-2 (MUST): The build is warning-free.** A new warning is fixed in
the same change. Planned: enforce it with `TreatWarningsAsErrors` in a
shared `Directory.Build.props` (debt B1).

**STYLE-3 (SHOULD): No unused `using`s, no dead code, no commented-out
code**, and consistent formatting (planned: `dotnet format` with an
`.editorconfig`, checked in CI).

**STYLE-4 (SHOULD): No repeated magic strings for domain values.** Values
such as `"available"`, `"sold"`, `"user"` and `"admin"` appear in several
places. Centralize them in one place (e.g. a static class of constants)
so a typo becomes a compile error instead of a silent bug.

---

## Definition of Done

A feature (or a step of one) is **done** when all of these are true:

- [ ] It does what the related requirement / user story says (FR / US ids cited in the PR).
- [ ] All **MUST** rules in this document hold for the new or changed code. Any **SHOULD** deviation is stated with a reason.
- [ ] Request DTOs are validated and bounded (VAL-1, VAL-2).
- [ ] Every expected failure maps to a documented status code (ERR-2).
- [ ] Unit tests cover each business rule and each failure branch (TEST-1, TEST-2), and all tests pass.
- [ ] `dotnet build` produces **zero warnings**.
- [ ] Living docs touched by the change are updated in the same branch: API contract, schema, architecture. A new decision with trade-offs gets an ADR.
- [ ] The endpoint was exercised manually (`.http` file / Postman) for the happy path and at least one error case.
- [ ] `docs/dev/STATUS.md` reflects the new state (at the end of the session).

---

## Code Review Checklist

Used in full at the end of a step and by the developer before opening a
PR. For a small change, only the section of the touched layer applies.
Report findings as
`[RULE-ID] (level) what is wrong → why it matters → which concept to look up`,
without rewriting the code unless asked.

**Layers**
- [ ] Controller: no business logic, no DbContext, DTOs only (ARCH-1)
- [ ] Rules and ownership checks in the service (ARCH-2, SEC-2)
- [ ] Repository free of business decisions (ARCH-3)
- [ ] One `SaveChangesAsync` per use case, called by the service (ARCH-4)
- [ ] New interface + `AddScoped` registration (ARCH-7)

**Input**
- [ ] Every request field constrained; pagination and collections bounded (VAL-1, VAL-2)
- [ ] Strings trimmed, e-mails case-insensitive (VAL-3)
- [ ] Enum-like values checked against the allow-list, including query filters (VAL-4)
- [ ] Money is `decimal`, positive, bounded (VAL-5)
- [ ] LIKE patterns escaped (VAL-6)
- [ ] No owner id / role / status bound from the body (VAL-7)

**Errors and concurrency**
- [ ] Specific exception types; nothing swallowed (ERR-1)
- [ ] Only custom exception types for expected failures, each mapped in the global handler (ERR-2)
- [ ] No `try/catch` in controllers (ERR-6)
- [ ] Unique violations and other expected DB errors translated (ERR-3, CONC-1)
- [ ] No internal details in error bodies (ERR-4)
- [ ] Return values checked or removed (ERR-5)
- [ ] No parallel use of one DbContext (CONC-3)

**Async and data**
- [ ] No `.Result` / `.Wait()`; `Async` suffix; `CancellationToken` passed through (ASYNC-1..3)
- [ ] `AsNoTracking` for reads, tracked entity for updates (DATA-1)
- [ ] Deterministic ordering + pagination on lists (DATA-2)
- [ ] No unnecessary `Include`, no N+1 (DATA-3)
- [ ] UTC timestamps; `UpdatedAt` maintained (DATA-4)
- [ ] Schema change → new migration, reviewed (DATA-5)

**Security, config, logging**
- [ ] Explicit `[Authorize]` / `[AllowAnonymous]` (SEC-1)
- [ ] No password or hash anywhere outside the hashing call (SEC-3)
- [ ] No secrets in code or committed config (SEC-4)
- [ ] Settings via Options pattern, no `config["x"]!` (CFG-1)
- [ ] Structured log templates; nothing sensitive logged (LOG-1, LOG-3)

**API and tests**
- [ ] `07-API-Design.md` updated; correct status codes; no entities returned (API-1..3)
- [ ] Business rules asserted, failure branches covered, names follow the convention, no duplicated setup (TEST-1..4)
- [ ] Right test level: logic → unit, queries/HTTP → integration (TEST-6)

**Build and style**
- [ ] Zero warnings; no unused usings or dead code; no repeated magic strings (STYLE-2..4)
