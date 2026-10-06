# CLAUDE.md

SoundRevival: a marketplace for used musical instruments. It is a solo 
**portfolio and learning project**. The backend is ASP.NET Core (.NET 10) 
with EF Core and PostgreSQL. The frontend (React + Vite + TS) is planned 
but not started.

**Read [docs/STATUS.md](docs/STATUS.md) first.** It describes the current 
state, the active branch, next steps, and known technical debt.

## How to work with me

- I am a junior developer and I use Claude Code **to learn**. Explain the 
  *why* behind choices, show the trade-offs between alternatives, and 
  point out patterns I can reuse.
- Leave the meaningful parts to me: business logic, design decisions, 
  and the first instance of a new pattern. Handle boilerplate and 
  repetitive work yourself (DTO shells, DI registration, mirroring an 
  existing pattern). Don't implement a whole feature in one go unless I 
  ask for it.
- Reply in **Italian**. Write code, comments, commit messages, and docs 
  in **English**.
- Don't commit, push, or open PRs unless I ask.
- When code and docs disagree, **say so** instead of silently picking 
  one. If a change alters a documented decision (API contract, schema, 
  architecture), update the relevant `docs/` file in the same change.
- At the end of a work session, when asked, update `docs/STATUS.md`. 
  Keep it factual and current.

## Environment & commands

Windows + PowerShell, Visual Studio. Run these from `backend/`:

```bash
docker compose -f ../docker-compose.yml up -d     # PostgreSQL 16 on :5432
dotnet build SoundRevival.slnx                    # must stay warning-free
dotnet test SoundRevival.slnx
dotnet run --project SoundRevival.WebApi          # http://localhost:5150
dotnet ef migrations add <Name> --project SoundRevival.Repository --startup-project SoundRevival.WebApi
dotnet ef database update      --project SoundRevival.Repository --startup-project SoundRevival.WebApi
```

`SoundRevival.WebApi/appsettings.Development.json` is gitignored and 
must exist locally. It provides `ConnectionStrings:DefaultConnection` 
and `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes`. 
Never print, commit, or copy its values. Manual API checks go through 
`SoundRevival.WebApi.http` or Postman. DB state is checked in DBeaver.

## Architecture rules

```
WebApi (Controllers) → Repository/Services → Repository/Repository → AppDbContext → PostgreSQL
                   ↘ Dto ↙
```

- **WebApi**: controllers and `Program.cs` only. No business logic, no 
  `AppDbContext`. Controllers receive and return **DTOs**, never entities.
- **Repository project**:
  - `Entities/`: EF Core entities.
  - `Interfaces/`: contracts for both repositories and services.
  - `Repository/`: data access only, no business rules.
  - `Services/`: business rules. Services depend on repository 
    *interfaces*, never on `AppDbContext`.
- **Dto**: plain request/response classes in per-feature folders 
  (`Dto/Auth/`, `Dto/Listings/`). This project depends on nothing.
- Every new service or repository gets an interface and an `AddScoped` 
  registration in `Program.cs`.
- Schema changes are made **Code First only**: change entities or 
  `AppDbContext`, then add a migration. Never edit an applied migration.

## Code conventions (follow the existing code)

- Block-scoped namespaces matching the folder; private readonly fields as 
  `_camelCase`; constructor injection.
- Primary keys are `Guid`, generated in the service (`Guid.NewGuid()`). 
  Timestamps use `DateTime.UtcNow`.
- Enum-like values (`Category`, `Condition`, `Status`, `Role`) are 
  **strings** validated in the service against the lists in 
  `docs/04-DomainModel.md`. `Status` and `Role` are lowercase 
  (`available`, `sold`, `user`, `admin`).
- New async methods use the `Async` suffix. `IUserRepository.FindUserByEmail` 
  is an existing exception.
- **Errors**: services throw exceptions. Controllers catch them and return 
  `{ "error": "..." }`. Current mapping: `InvalidOperationException` → 
  400, `UnauthorizedAccessException` → 401. The mapping for 403 
  (not owner) and 404 (not found) is **still to be decided**. Ask me 
  before introducing it.
- **Current user id in controllers**: 
  `User.FindFirstValue(ClaimTypes.NameIdentifier)`. The JWT `sub` claim 
  is remapped because `MapInboundClaims` is left at its default, so 
  `FindFirst("sub")` returns null. The role is in `ClaimTypes.Role`.
- Ownership checks (owner or admin) live in the **service**, not in the 
  controller.

## Business rules that must hold

- Listing GETs are public. POST requires authentication. PUT and DELETE 
  are allowed for the owner or an admin. Image endpoints are owner only.
- At most **5 images** per listing, enforced in the service.
- New listings get `status = available`. `GET /api/listings` returns only 
  `available` listings by default. Pagination uses `page` and 
  `pageSize`, defaulting to 1 and 20.
- Registration always assigns `role = user`. Passwords and hashes never 
  appear in responses or logs.

## Testing

- Unit tests use xUnit + Moq in `SoundRevival.Tests`. They test 
  **services**, with repository interfaces and `IConfiguration` mocked.
- Name tests `Method_Scenario_ExpectedResult`, one scenario per test, 
  following the Arrange-Act-Assert structure.
- Integration tests (Testcontainers + real Postgres) are planned but not 
  set up yet.

## Git

- One branch per feature (`feature/<name>`). Use conventional commits 
  (`feat:`, `fix:`, `docs:`, `chore:`, `test:`, `refactor:`).
- Merge into `main` through GitHub PRs, so the process is visible in the 
  portfolio.

## Docs map

| Need | File |
|---|---|
| Current state, next steps, debt | `docs/STATUS.md` |
| Requirements (FR/NFR ids) | `docs/02-Requirements.md` |
| Entities, enums, glossary | `docs/04-DomainModel.md` |
| Actual schema + planned constraints | `docs/05-Database.md` |
| Layers and auth details | `docs/06-Architecture.md` |
| Endpoint contracts, status codes | `docs/07-API-Design.md` |
| Frontend structure (when started) | `docs/08-Frontend-Design.md` |
| Test strategy | `docs/10-Testing.md` |
| Why decisions were made | `docs/adr/` |
