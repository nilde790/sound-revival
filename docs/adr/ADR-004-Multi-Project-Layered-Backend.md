# ADR-004: Multi-project layered backend solution

- **Status**: Accepted
- **Date**: 2026-07-12 (recorded retroactively on 2026-11-11)

## Context

The backend needs a structure that separates HTTP handling, business 
rules and data access, so that business rules can be unit tested and the 
layers can't depend on each other in the wrong direction. The project is 
also a learning exercise in professional .NET structure.

## Decision drivers

- Enforce separation of concerns with the compiler, not only by 
  convention
- Testability of business logic with mocked data access
- Familiarity: a structure commonly found in enterprise .NET code
- Simplicity proportionate to a small CRUD domain

## Considered options

### Option A — Single project, folders per layer
- ✅ Simplest setup, fastest to navigate
- ❌ Nothing stops a controller from using `AppDbContext` directly: 
  boundaries rely on discipline only

### Option B — Multi-project solution: WebApi / Repository / Dto / Tests
- ✅ Project references enforce the dependency direction 
  (WebApi → Repository + Dto; Repository → Dto; Dto → nothing)
- ✅ Controllers can't see entities' persistence details beyond what the 
  Repository project exposes
- ❌ More projects to configure; some ceremony for a small domain

### Option C — Full Clean Architecture (Domain / Application / Infrastructure / WebApi)
- ✅ Domain independent of EF Core; very common in job descriptions
- ❌ Four or more projects and extra mapping for a CRUD domain: risk of 
  ceremony without benefit at this scale

## Decision

**Option B.** It gives compiler-enforced boundaries and testable 
services with moderate complexity. Inside `SoundRevival.Repository`, 
the folders `Entities/`, `Interfaces/`, `Repository/` and `Services/` 
separate data access from business rules; services depend on 
repository interfaces, never on `AppDbContext`.

## Consequences

- **Positive**: services are unit tested with Moq; controllers stay thin; 
  the structure is easy to explain.
- **Negative**: the project name `SoundRevival.Repository` describes only 
  part of its content (entities, services, interfaces, DbContext and 
  migrations live there too), which can confuse readers. Business rules 
  and EF Core entities share one project, so the domain is not 
  persistence-independent.
- **Follow-ups**: renaming or splitting the project (e.g. towards 
  Option C) is tracked as technical debt A1 in 
  [`../dev/STATUS.md`](../dev/STATUS.md). If done, it gets a new ADR 
  that supersedes this one.

## Links

- [`../06-Architecture.md`](../06-Architecture.md) → Backend
- [ADR-001](ADR-001-Architecture.md) — separate backend and frontend
- Rules ARCH-1..ARCH-7 in [`../dev/ENGINEERING-STANDARDS.md`](../dev/ENGINEERING-STANDARDS.md)
