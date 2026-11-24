# ADR-011: Split the backend into Core and Infrastructure

- **Status**: Accepted
- **Date**: 2026-11-23
- **Supersedes**: [ADR-004](ADR-004-Multi-Project-Layered-Backend.md)

## Context

ADR-004 chose four projects: `WebApi`, `Repository`, `Dto`, `Tests`. 
After phase 1, `SoundRevival.Repository` holds entities, repository and 
service interfaces, services, exceptions, JWT generation, 
`AppDbContext` and migrations. Three concrete problems:

1. **The name describes only part of the content** (debt A1).
2. **Business logic depends on infrastructure, and nothing stops it.** 
   `AuthService` imports EF Core and Npgsql to recognize the PostgreSQL 
   unique violation (`23505`). Rule ARCH-2 forbids it, but services and 
   EF Core share one project, so the compiler can't enforce the rule.
3. **The inner layer depends on the HTTP contract.** The entities 
   `Listing` and `User` import `SoundRevival.Dto.Constants`, and 
   `IListingRepository` takes HTTP DTOs (debt A2).

Timing: phase 2 writes project paths into the Dockerfile, the CI 
workflow and a new integration test project. Restructuring later would 
touch all of them a second time.

## Decision drivers

- Compiler-enforced boundaries: business logic must not be able to 
  reference EF Core, Npgsql or JWT libraries
- Names that match the content
- Cost of change, which grows with every file that refers to project 
  paths (phase 2)
- Complexity proportionate to a small domain whose entities have no 
  behaviour yet
- Market relevance: the structure should be recognizable to .NET teams
- A path towards Domain/Application separation when v2 adds real domain 
  logic (the `reserved` state machine, payments)

## Considered options

### Option 1 — Keep the structure, accept A1
- ✅ No work
- ❌ All three problems remain

### Option 2 — Rename only (e.g. `SoundRevival.Core`)
- ✅ Cheap; the name becomes accurate
- ❌ Solves problem 1 only: EF Core and services still share a project

### Option 3 — Split into Core + Infrastructure
`Core` holds entities, interfaces, services and exceptions, with no 
infrastructure packages. `Infrastructure` holds EF Core, Npgsql and JWT 
code and implements Core's interfaces.
- ✅ The compiler enforces problem 2's rule; problem 1 is solved; 
  problem 3 becomes visible in the project references
- ✅ A common .NET layout ("Core / Infrastructure", the dependency 
  inversion at the heart of Clean and Onion architecture)
- ❌ One more project; the unique-violation translation must move

### Option 4 — Full Clean Architecture (Domain / Application / Infrastructure / WebApi)
- ✅ The most cited layout in .NET job descriptions
- ❌ Today `Domain` would hold three classes without behaviour: 
  structure without benefit (the reason ADR-004 rejected it still holds)

### Option 5 — Vertical slices (one folder per feature)
- ✅ Growing popularity; features change in one place
- ❌ Replaces the layered rules (ARCH-1..8) and most of the 
  documentation; less common than layered/Clean in job descriptions

## Decision

**Option 3**, plus **separate unit and integration test projects**. It 
gives the compiler-enforced boundary at moderate cost, and it can grow 
into Option 4 later without reversing any dependency.

| Project | Contents | References |
|---|---|---|
| `SoundRevival.Dto` | Request/response DTOs, validation attributes, constants (unchanged) | — |
| `SoundRevival.Core` | `Entities/`, `Interfaces/` (repositories, services, `ITokenService`), `Services/`, `Exceptions/`. No EF Core, Npgsql or JWT packages | Dto |
| `SoundRevival.Infrastructure` | `AppDbContext`, `Migrations/`, repository implementations, `JwtTokenService`, `JwtSettings` | Core |
| `SoundRevival.WebApi` | Controllers, exception handler, `Program.cs` | Core, Dto, Infrastructure (DI registration only) |
| `SoundRevival.UnitTests` | Renamed from `SoundRevival.Tests`: services with mocked interfaces, DTO validation, `JwtTokenService` | Core, Dto, Infrastructure |
| `SoundRevival.IntegrationTests` | New in phase 2: `WebApplicationFactory` + Testcontainers | WebApi |

Rules that follow:
- The unique-violation translation moves to Infrastructure: the 
  repository catches the `23505` `DbUpdateException` and throws an 
  exception defined in Core. The service no longer knows the database 
  is PostgreSQL. Which exception is decided when it is implemented.
- Constants (`ListingValues`, `Roles`) stay in `Dto` for now: Core 
  already depends on Dto because services return DTOs, so moving them 
  to Core would create a cycle. They are revisited together with A2.
- Controllers depend only on Core interfaces; WebApi references 
  Infrastructure only to register implementations in `Program.cs`.
- Unit tests run without Docker; integration tests run in their own CI 
  job.

## Consequences

- **Positive**: business logic can't reference infrastructure; names 
  match contents; unit tests stay fast and independent of Docker; 
  phase 2 files (Dockerfile, CI, test projects) are written once, on 
  the final paths.
- **Negative**: one more project; every file changes namespace; EF 
  commands change (`--project SoundRevival.Infrastructure`); existing 
  branches based on the old layout would conflict (none open today).
- **Follow-ups**:
  - Implemented as the first branch of phase 2, before build quality. 
    `git mv` keeps file history. Moving `Migrations/` does not touch the 
    database: `__EFMigrationsHistory` stores migration ids, not 
    namespaces.
  - Update in the same branch: 06-Architecture.md, CLAUDE.md (commands), 
    README (commands), ENGINEERING-STANDARDS (ARCH rules mention 
    project names), STATUS (A1 closed).
  - Open point, decided during the refactor: `AuthService` calls the 
    BCrypt library directly. Options: leave it in Core, or put it behind 
    an `IPasswordHasher` interface (Core) implemented in Infrastructure, 
    like `ITokenService`.
  - A registration extension method in Infrastructure (e.g. 
    `AddInfrastructure`) would also close A5. A2 (repository query 
    type) becomes easier: the query object belongs in Core.
  - When v2 brings domain behaviour (`listing.MarkAsSold()`, the 
    `reserved` state, rule ARCH-8), split Core into Domain + Application 
    with a new ADR superseding this one.

## Implementation notes (2026-11-24)

Appended after implementation; the decision above is unchanged. They 
record how the open points were settled and where the code differs 
from the table.

- **Unique violation**: the repository throws 
  `UniqueConstraintViolationException` (Core); `AuthService` turns it 
  into `BusinessRuleException("Email already registered")`.
- **BCrypt open point**: decided for `IPasswordHasher` (Core), 
  implemented by `BCryptPasswordHasher` (Infrastructure, singleton). 
  Core has no BCrypt package, and `AuthService` tests use a fake hasher.
- **Registration**: `AddInfrastructure(configuration)` in 
  Infrastructure registers DbContext, repositories, token service, 
  hasher and validated `JwtSettings` (A5 closed). Core services stay 
  registered in `Program.cs`.
- **Deviation from the table**: Infrastructure references **Core and 
  Dto**, because `IListingRepository` takes filter DTOs. It disappears 
  with A2 (query object in Core).
- `Microsoft.EntityFrameworkCore.Design` stays in WebApi: `dotnet ef` 
  needs it in the startup project.

## Links

- [ADR-004](ADR-004-Multi-Project-Layered-Backend.md) (superseded)
- [`../06-Architecture.md`](../06-Architecture.md) → Backend
- Rules ARCH-1..8 in [`../dev/ENGINEERING-STANDARDS.md`](../dev/ENGINEERING-STANDARDS.md)
- Debts A1, A2, A5 in [`../dev/STATUS.md`](../dev/STATUS.md)
- [Common web application architectures (Microsoft)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
