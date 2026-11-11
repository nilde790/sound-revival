# ADR-009: Error handling — exception types, mapping and response format

- **Status**: Accepted (resolves decision D1)
- **Date**: 2026-11-11

## Context

Services signal expected failures by throwing exceptions; controllers 
catch them and return `{ "error": "..." }`. Today two types are mapped, 
each with its own `try/catch` in `AuthController`:

| Exception | Status |
|---|---|
| `InvalidOperationException` | 400 |
| `UnauthorizedAccessException` | 401 |

The Listings feature needs two more outcomes: **403** (not owner, not 
admin) and **404** (listing not found). It will also have about six 
endpoints, each needing the same `try/catch` blocks. Meanwhile, 
`[ApiController]` already returns validation errors as RFC 9457 
`ValidationProblemDetails`, a different shape from `{ "error" }`. So 
clients would see two error formats.

There are three questions to answer:
1. **How** do services report "not found" and "forbidden"?
2. **Where** are failures turned into HTTP responses?
3. **What** does the error body look like?

## Decision drivers

- One consistent error format for the frontend
- No duplicated `try/catch` in every action (DRY)
- Expected failures must never become `500` (rule ERR-2)
- Learning value and market relevance (standard .NET practice)
- Small change to existing code (only `AuthController` is implemented)

## Considered options

### Question 1 — how services report failures

**1a. Reuse built-in .NET exceptions** (e.g. `KeyNotFoundException` → 404)
- ✅ No new classes
- ❌ No good built-in type for "forbidden" (`UnauthorizedAccessException` 
  already means 401 here); built-in exceptions can also be thrown by the 
  framework for unrelated reasons and be mapped by mistake

**1b. A small set of custom exceptions**, e.g. `NotFoundException`, 
`ForbiddenException`, `ConflictException`, `BusinessRuleException` 
(possibly with a common base class)
- ✅ Intention is explicit; mapping is unambiguous
- ❌ A few new classes (in the Repository project, where services live)

**1c. Result pattern** — services return `Result<T>` (success or a typed 
error) instead of throwing; libraries like ErrorOr or FluentResults, or 
a small hand-written type
- ✅ Failures are visible in method signatures; no exceptions for 
  control flow
- ❌ Every call site must check the result; more code; changes every 
  service already written. A valid alternative, better appreciated 
  after mastering the exception approach

### Question 2 — where mapping happens

**2a. `try/catch` in each controller action** (current pattern)
- ✅ Explicit, easy to read for one endpoint
- ❌ Repeated in every action; easy to forget a mapping → `500`

**2b. One global handler: `IExceptionHandler`** (ASP.NET Core 8+), 
registered with `AddExceptionHandler<T>()` and `UseExceptionHandler()`
- ✅ One place maps every exception type; controllers become one-liners
- ✅ Also catches truly unexpected exceptions, logs them once (rule 
  LOG-4) and returns a safe `500`
- ❌ The mapping is "invisible" from the controller; requires 
  understanding the middleware pipeline

### Question 3 — response body

**3a. Keep `{ "error": "..." }`**
- ✅ Already documented and implemented
- ❌ Not a standard; differs from the automatic validation responses

**3b. RFC 9457 `ProblemDetails`** (`type`, `title`, `status`, `detail`, 
`instance`), enabled with `AddProblemDetails()`
- ✅ An Internet standard that ASP.NET Core uses natively: validation 
  errors are already in this shape, so there is **one** format
- ✅ Frontend libraries and API tools understand it
- ❌ Changes the documented contract (cheap now: the frontend doesn't 
  exist yet, and only two endpoints are affected)

## Decision

**1b + 2b + 3b**: a few custom exception types, mapped in one 
`IExceptionHandler`, returning `ProblemDetails`.

| Outcome | Exception (one custom type each) | Status |
|---|---|---|
| Resource not found | not-found exception | 404 |
| Authenticated but not allowed (not owner/admin) | forbidden exception | 403 |
| Business rule violated (e.g. email already registered, 6th image, invalid status transition) | business-rule exception | 400 |
| Wrong credentials at login | invalid-credentials exception | 401 |
| Anything else | — | 500, generic message, logged once |

Rules that come with the decision:
- Services throw **only** these custom types for expected failures. 
  Built-in exceptions (`InvalidOperationException`, 
  `UnauthorizedAccessException`, `KeyNotFoundException`) are **not** 
  mapped, because the framework and libraries throw them too (e.g. EF 
  Core throws `InvalidOperationException`): mapping them would turn real 
  bugs into misleading `400`s.
- The exact class names (and whether they share a base class) are 
  chosen when implementing: the first instance of the pattern is written 
  by the developer.
- Validation errors keep the `ValidationProblemDetails` shape produced 
  by `[ApiController]` (see [ADR-010](ADR-010-Input-Validation.md)), so 
  every error body is a `ProblemDetails`.

Why: it removes the duplicated `try/catch` before six new endpoints copy 
it, gives the frontend a single error format that matches the validation 
errors ASP.NET Core already produces, and it is the pattern currently 
recommended in .NET documentation. The cost is low today (two actions 
in `AuthController`), and it grows with every endpoint written in the 
old style.

The Result pattern (1c) is a good topic to revisit later, not a 
prerequisite.

## Consequences

- **Positive**: controllers stay thin (ARCH-1); every expected failure 
  has exactly one mapping (ERR-2); unexpected errors are logged once and 
  never leak details (ERR-4).
- **Negative**: the documented `{ "error" }` shape is replaced (done in 
  `07-API-Design.md` and `/CLAUDE.md` on 2026-11-11). The existing auth 
  code must migrate: `AuthService` throws the new types instead of 
  `InvalidOperationException` / `UnauthorizedAccessException`, and 
  `AuthController`'s `try/catch` blocks are removed (technical debt E1).
- **Follow-ups**: integration tests assert the `ProblemDetails` body for 
  `400/401/403/404`.

## Links

- [`../07-API-Design.md`](../07-API-Design.md) → Status codes, Error responses
- Rules ERR-1..ERR-4, LOG-4 in [`../dev/ENGINEERING-STANDARDS.md`](../dev/ENGINEERING-STANDARDS.md)
- [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors)
- [RFC 9457 — Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457)
