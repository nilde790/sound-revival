# Testing Strategy

Test rules (naming, what to assert, setup) are in 
[dev/ENGINEERING-STANDARDS.md](dev/ENGINEERING-STANDARDS.md) §11. This 
document describes *which kinds* of tests exist, what each one covers, 
and the tools used. The acceptance criteria in 
[03-UserStories.md](03-UserStories.md) are the starting list of test 
cases.

## Test pyramid

```
            ┌──────────────┐
            │  End-to-end  │  few, slow — Playwright (later phase)
            ├──────────────┤
            │ Integration  │  some — WebApplicationFactory + Testcontainers
            ├──────────────┤
            │     Unit     │  many, fast — xUnit + Moq (services), Vitest (frontend)
            └──────────────┘
```

Many fast unit tests for business logic, fewer integration tests for the 
parts that only make sense against real infrastructure, very few 
end-to-end tests for the critical user journeys. Each level catches 
problems the others can't.

| Level | State |
|---|---|
| Backend unit tests | ✅ in use: services (`AuthService`, `ListingService`, `JwtTokenService`) and request DTO validation |
| Backend integration tests | ⬜ phase 2 (debt T4) |
| Frontend unit/component tests | ⬜ with the frontend |
| End-to-end tests | ⬜ later phase (see 09-Roadmap.md) |
| Coverage report in CI | ⬜ with the CI pipeline |

## Backend

### Unit tests — xUnit + Moq

Test the business logic inside **services**, in isolation: repository 
interfaces are mocked with Moq, so no database is involved.

Covered:
- Every business rule, asserted on the outcome (e.g. registration stores 
  role `user` and a hashed password; a non-owner can't edit a listing; 
  a 6th image is rejected)
- Every failure branch: each `throw` in a service has a test
- Validation logic that lives in services (allowed values, state 
  transitions)
- Request DTO validation (DataAnnotations, `IValidatableObject`, custom 
  attributes), run through `Validator.TryValidateObject` without HTTP 
  (`SoundRevival.UnitTests/Validation/`)

Conventions (naming, assertions, setup, folders): rules TEST-1..7 in 
the standards.

Not covered by unit tests (see integration tests):
- Whether an EF Core LINQ query produces the right SQL and results
- Routing, model binding, authentication/authorization attributes, 
  status codes

### Integration tests — WebApplicationFactory + Testcontainers

Run the real application in memory (`WebApplicationFactory<Program>` 
from `Microsoft.AspNetCore.Mvc.Testing`) against a **real PostgreSQL** 
started in a disposable Docker container by **Testcontainers**, then 
destroyed. Never against the development database.

Why both tools: `WebApplicationFactory` exercises the full HTTP pipeline 
(routing, JSON, auth, filters) without deploying anything; Testcontainers 
gives the same database engine as production, so PostgreSQL-specific 
behaviour (`ILIKE`, constraints, `numeric` precision) is really tested. 
An in-memory database provider would hide exactly those differences.

Covered:
- Repository queries: filters, sorting, pagination, case-insensitive 
  email lookup
- End-to-end API behaviour per endpoint: status codes, response shape, 
  `401`/`403`/`404` cases (e.g. `POST /api/listings` → row created and 
  linked to the authenticated user; anonymous `GET /{id}` has no seller 
  email)
- Constraints: duplicate email registration returns `400`, not `500`

Practical notes:
- Requires Docker running locally and in CI (GitHub-hosted Ubuntu 
  runners have Docker)
- One container per test class/collection (xUnit fixtures), with data 
  reset between tests, to keep the suite fast
- Tests create their own users and get tokens through the real login 
  endpoint

## Frontend

### Unit/component tests — Vitest + React Testing Library

Test components the way a user uses them (click, type, read the rendered 
output), not their internal implementation.

Covered:
- Forms validate input before submission (e.g. `ListingForm` blocks 
  submit with an empty title)
- Conditional rendering (e.g. "Sold" badge when status is `sold`; 
  "log in to contact the seller" when the seller email is absent)
- Basic hook behaviour where useful (e.g. `useAuth` reflects login state)

API calls are mocked at the network level (e.g. Mock Service Worker) 
rather than by mocking Axios internals.

## End-to-end tests — Playwright (later phase)

A handful of browser tests against the full running stack, for the 
critical journeys: register → create listing → see it on the homepage; 
log in → mark as sold → it disappears from search. Planned after the 
frontend and deployment are in place, because they need the whole stack.

## Coverage

Coverage is collected in CI (`coverlet.collector` is already referenced 
by the test project) and reported on pull requests. It is a signal for 
untested areas, not a target: a high percentage with assertions like 
`Assert.NotNull` proves little (rule TEST-1). No minimum threshold is 
enforced for now.

## What is intentionally NOT tested

- Third-party services themselves (image storage, LLM providers): we test 
  that our code calls them correctly, behind an interface that can be 
  mocked
- Framework behaviour (e.g. that ASP.NET Core parses JSON)
- Purely visual/styling correctness (no visual regression testing)

## CI integration

Backend unit and integration tests, and frontend tests once they exist, 
run automatically via GitHub Actions on every push/pull request (see 
06-Architecture.md → CI/CD) and must pass before merging to `main` 
(NFR-9).
