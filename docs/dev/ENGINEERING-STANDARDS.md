# Engineering Standards

> The quality bar for every change: concrete rules, each with its reason 
> and, where useful, an example from this project.
> - **Before coding**: read the rules of the layer you touch. The 
>   [Definition of Done](#definition-of-done) says when work is finished.
> - **Review**: Claude Code applies the 
>   [checklist](#code-review-checklist) proactively (Review duty in 
>   `/CLAUDE.md`).
> - **A mistake slipped through?** Fix the code *and* add or sharpen a 
>   rule here.
>
> Known violations are tracked as debt in [STATUS.md](STATUS.md) §4.

| Level | Meaning |
|---|---|
| **MUST** | Breaking it is a bug, a security risk or a broken contract: the change is not done. |
| **SHOULD** | The default; deviate only with a stated reason. |
| **CONSIDER** | Context-dependent; the reviewer mentions it, the developer decides. |

---

## 1. Architecture and layers

See [`../06-Architecture.md`](../06-Architecture.md) and ADR-004.

- **ARCH-1 (MUST) Thin controllers**: read the request and the user id 
  from claims, call one service method, return the result. No business 
  rules, no `AppDbContext`, no entities — DTOs only. *Why*: rules in 
  controllers can't be unit tested and get duplicated.
- **ARCH-2 (MUST) Business rules in services** (ownership, allowed 
  values, transitions, max 5 images). Services depend on repository 
  interfaces, never on `AppDbContext`.
- **ARCH-3 (MUST) Repositories do data access only**: no `if` encoding a 
  business rule.
- **ARCH-4 (MUST) One `SaveChangesAsync` per use case, called by the 
  service at the end.** All repositories share one scoped `AppDbContext` 
  (EF Core's Unit of Work): saving through *any* repository commits *all* 
  pending changes of the request.
- **ARCH-5 (SHOULD) One responsibility per class**: token generation, 
  clock, external APIs go behind their own interface (debt A3).
- **ARCH-6 (SHOULD) Don't leak contracts across layers**: repositories 
  take simple parameters or an inner-layer query object, not HTTP DTOs 
  (existing deviation: A2 — don't copy it).
- **ARCH-7 (MUST)** Every new service/repository has an interface and an 
  `AddScoped` registration.
- **ARCH-8 (CONSIDER) Keep entities simple** until real state transitions 
  (v2 `reserved`) justify domain methods like `listing.MarkAsSold()`.

## 2. Input validation

Mechanism: DataAnnotations on request DTOs, `IValidatableObject` for 
cross-field rules; data-dependent rules in the service 
([ADR-010](../adr/ADR-010-Input-Validation.md)). Limits: 07-API-Design.md 
→ Field constraints.

- **VAL-1 (MUST)** Every request DTO field has explicit constraints 
  (required, length, range, format).
- **VAL-2 (MUST) Bound everything a client can make large**: `page ≥ 1`, 
  `1 ≤ pageSize ≤ 100`, max collection sizes, max text lengths. *Why*: 
  unbounded input is the cheapest denial of service.
- **VAL-3 (MUST) Normalize before comparing or storing**: trim strings; 
  emails compared case-insensitively (debt C2).
- **VAL-4 (MUST)** Enum-like values checked against the allow-lists in 
  04-DomainModel.md — query filters included.
- **VAL-5 (MUST) Money is `decimal`**, positive, ≤ 2 decimals, bounded. 
  Never `double`.
- **VAL-6 (MUST) Escape `%`, `_`, `\` in user text used in `LIKE`/`ILIKE`.** 
  Not injection (EF parameterizes), but `%` would match everything. 
  Prefer `column.ToLower().Contains(term)` (term lowered with 
  `ToLowerInvariant()`): EF escapes the term itself. A hand-built pattern 
  in `EF.Functions.ILike` must be escaped explicitly.
- **VAL-7 (MUST) No security-relevant fields in request bodies**: owner 
  id from the token; no `role`, `userId`, `id`, `createdAt`, nor `status` 
  outside its endpoint. *Why*: mass assignment (OWASP API Top 10).
- **VAL-8 (MUST) Parse and format numbers and dates in code with the 
  invariant culture**, attributes included: 
  `[Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true)]`. 
  The current culture is only for showing values to users. *Why*: on an 
  Italian machine `"0.01"` fails to parse (comma separator) and every 
  request on that DTO returns 500, while CI in English passes.

## 3. Error handling

Mechanism: custom exceptions → one global `IExceptionHandler` → RFC 9457 
`ProblemDetails` ([ADR-009](../adr/ADR-009-Error-Handling.md)).

- **ERR-1 (MUST)** Expected failures throw a specific exception; never 
  return `null`/`false` for "failed", never swallow (`catch {}`).
- **ERR-2 (MUST)** Every exception thrown on purpose is mapped in the 
  global handler and documented in 07-API-Design.md. Never use built-in 
  exceptions (`InvalidOperationException`, `UnauthorizedAccessException`, 
  `KeyNotFoundException`) for expected failures: the framework throws 
  them too, so they stay unmapped (`500`).
- **ERR-3 (MUST) Translate expected database errors**: a unique 
  violation is `DbUpdateException` with an inner `PostgresException` 
  whose `SqlState` is `23505` (`PostgresErrorCodes.UniqueViolation`).
- **ERR-4 (MUST)** Error bodies never contain stack traces, SQL or 
  internals; auth failures share one message.
- **ERR-5 (SHOULD)** A returned success `bool` must be checked; otherwise 
  return `Task` and throw.
- **ERR-6 (MUST)** No `try/catch` in controllers to build error 
  responses: mapping lives only in the global handler.

## 4. Concurrency

- **CONC-1 (MUST) Check-then-act is not atomic**: a DB unique constraint 
  is the real guard, and its violation is handled (ERR-3). The check in 
  code only gives a nicer message for the common case (debt C3).
- **CONC-2 (SHOULD → MUST with v2 payments)** Detect concurrent updates 
  to important state with optimistic concurrency (PostgreSQL `xmin` as 
  concurrency token → `DbUpdateConcurrencyException`).
- **CONC-3 (MUST)** Never run parallel operations on one `DbContext` (not 
  thread-safe).

## 5. Async and cancellation

- **ASYNC-1 (MUST)** No `.Result`, `.Wait()`, `GetAwaiter().GetResult()`.
- **ASYNC-2 (SHOULD)** Public async methods take a `CancellationToken` 
  (last, `= default`) and pass it to EF Core; controller actions receive 
  it as a parameter (bound to request abort).
- **ASYNC-3 (MUST)** Async methods end with `Async` 
  (`FindUserByEmail` is a known exception).

## 6. Data access

See [`../05-Database.md`](../05-Database.md).

- **DATA-1 (MUST)** Reads use `AsNoTracking()`; entities loaded to be 
  modified stay tracked (ADR-007).
- **DATA-2 (MUST)** Lists are paginated and deterministically ordered, 
  ending with a unique column (`ThenBy(l => l.Id)`).
- **DATA-3 (SHOULD)** Load only what you need: deliberate `Include`, 
  `Select` projections for lists, no N+1.
- **DATA-4 (MUST)** UTC timestamps set in one place; `UpdatedAt` set on 
  every modification (debt A7).
- **DATA-5 (MUST)** Schema changes via reviewed Code First migrations; 
  never edit an applied one; limits via fluent API.
- **DATA-6 (SHOULD)** Indexes follow measured query patterns, not 
  guesses.

## 7. Security

- **SEC-1 (MUST)** Every endpoint is explicitly `[Authorize]`, 
  `[Authorize(Roles = "admin")]` or `[AllowAnonymous]`, matching 07.
- **SEC-2 (MUST)** Authorization in the service, with the user id from 
  the claims (`ClaimTypes.NameIdentifier`), never from the client.
- **SEC-3 (MUST)** Passwords only hashed with BCrypt; passwords and 
  hashes never logged or returned.
- **SEC-4 (MUST)** No secrets in git: local gitignored settings or 
  `dotnet user-secrets`; environment variables when deployed.
- **SEC-5 (SHOULD)** Rate limit login/register (`AddRateLimiter`) (C5).
- **SEC-6 (SHOULD)** Login failures identical in message *and* timing 
  (C8).
- **SEC-7 (MUST)** Fix vulnerable dependencies (NU1901–NU1904); a pin 
  used as a fix carries a comment with the advisory id.
- **SEC-8 (MUST, before the frontend)** CORS allows only known origins.

## 8. Configuration

- **CFG-1 (SHOULD) Options pattern**: bind sections to classes, 
  `ValidateDataAnnotations().ValidateOnStart()`, inject `IOptions<T>`. 
  *Why*: missing settings fail at startup with a clear message; tests use 
  `Options.Create(...)` instead of mocking `IConfiguration` (C7, T2).
- **CFG-2 (SHOULD)** Environment differences live in configuration, 
  never in `IsDevelopment()` checks in business code.

## 9. Logging

- **LOG-1 (SHOULD)** `ILogger<T>` with message templates 
  (`"Listing {ListingId} created"`), not interpolation: structured logs.
- **LOG-2 (SHOULD)** Levels: Information = business events, Warning = 
  handled anomalies, Error = failed operations, Debug = dev detail.
- **LOG-3 (MUST)** Never log secrets, passwords, hashes, tokens, 
  connection strings or full bodies; log ids, not emails.
- **LOG-4 (SHOULD)** Log an unexpected exception once, at the boundary 
  (the global handler).

## 10. API design

- **API-1 (MUST)** Endpoint changes update 07-API-Design.md in the same 
  branch.
- **API-2 (SHOULD)** Status codes as in 07-API-Design.md → Status codes 
  (`201` + `Location` on create, `204` on delete).
- **API-3 (MUST)** Never return entities; responses expose only what the 
  client needs.

## 11. Testing

Strategy and tools: [`../10-Testing.md`](../10-Testing.md).

- **TEST-1 (MUST) Every business rule has a test that fails if the rule 
  is removed**: assert outcomes (`Verify`, `It.Is<…>`), not just 
  `NotNull` (debt T1).
- **TEST-2 (MUST)** Every `throw` in a service has a test.
- **TEST-3 (MUST)** One scenario per test, `Method_Scenario_ExpectedResult` 
  naming the behaviour (`ThrowsForbiddenException`), Arrange-Act-Assert.
- **TEST-4 (SHOULD)** No copy-pasted setup: constructor or builder 
  helpers (xUnit creates a new instance per test).
- **TEST-5 (MUST)** Unit tests: no DB, network, file system, real clock 
  or randomness.
- **TEST-6 (MUST) Right level**: services → unit tests with mocks; EF 
  queries and HTTP behaviour → integration tests on real PostgreSQL. 
  Don't mock `DbContext`.
- **TEST-7 (SHOULD)** Test folders mirror the code (`Services/`, 
  `Integration/`), not tools (`Mock/`).

## 12. Style and build

- **STYLE-1 (MUST)** Conventions: block-scoped namespaces matching 
  folders, `_camelCase` readonly fields, constructor injection, 
  `Guid.NewGuid()` in services, `DateTime.UtcNow`.
- **STYLE-2 (MUST)** Zero build warnings (enforcement planned: B1).
- **STYLE-3 (SHOULD)** No unused `using`s, dead or commented-out code; 
  consistent formatting (`dotnet format`, planned).
- **STYLE-4 (SHOULD)** Domain literals (`"available"`, `"admin"`) 
  centralized as constants, so typos become compile errors.

---

## Definition of Done

- [ ] Meets the related FR / US acceptance criteria (ids in the PR)
- [ ] All MUST rules hold; SHOULD deviations are explained
- [ ] Inputs validated and bounded; every expected failure mapped
- [ ] Each business rule and failure branch tested; all tests pass
- [ ] `dotnet build` with zero warnings
- [ ] Affected living docs (API, schema, architecture) updated; new 
      trade-off decisions recorded as ADRs
- [ ] Happy path and one error case tried manually (`.http` / Postman)
- [ ] `STATUS.md` updated at the end of the session

## Code Review Checklist

Full list at the end of a step; for a small change, only the touched 
layer. Report as `[RULE-ID] (level) what → why → concept`.

- **Layers**: thin controller (ARCH-1) · rules and ownership in service 
  (ARCH-2, SEC-2) · repository without business logic (ARCH-3) · one save 
  per use case (ARCH-4) · interface + `AddScoped` (ARCH-7)
- **Input**: constrained and bounded (VAL-1, VAL-2) · normalized 
  (VAL-3) · allow-lists incl. filters (VAL-4) · `decimal` money (VAL-5) · 
  escaped LIKE (VAL-6) · no owner/role/status from body (VAL-7) · 
  invariant-culture parsing (VAL-8)
- **Errors & concurrency**: specific custom exceptions, mapped, nothing 
  swallowed (ERR-1, ERR-2) · DB errors translated, DB constraint as guard 
  (ERR-3, CONC-1) · safe bodies (ERR-4) · return values checked (ERR-5) · 
  no controller `try/catch` (ERR-6) · no parallel DbContext use (CONC-3)
- **Async & data**: no blocking, `Async` suffix, token passed 
  (ASYNC-1..3) · tracking right (DATA-1) · ordered + paginated (DATA-2) · 
  no N+1 (DATA-3) · UTC + `UpdatedAt` (DATA-4) · reviewed migration 
  (DATA-5)
- **Security, config, logging**: explicit auth attribute (SEC-1) · no 
  password/hash leaks (SEC-3) · no secrets (SEC-4) · Options pattern 
  (CFG-1) · structured, safe logs (LOG-1, LOG-3)
- **API & tests**: 07 updated, right status codes, no entities 
  (API-1..3) · rules asserted, branches covered, naming, no duplicated 
  setup (TEST-1..4) · right test level (TEST-6)
- **Build**: zero warnings, no dead code, no repeated literals 
  (STYLE-2..4)
