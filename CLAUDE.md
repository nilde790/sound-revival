# CLAUDE.md

SoundRevival: a marketplace for used musical instruments. It is a solo 
**portfolio and learning project**. The backend is ASP.NET Core (.NET 10) 
with EF Core and PostgreSQL. The frontend (React + Vite + TS) is planned 
but not started.

**Read [docs/dev/STATUS.md](docs/dev/STATUS.md) first.** It describes the current 
state, the active branch, next steps, and known technical debt.

## How to work with me

- I am a junior developer and I use Claude Code **to learn**. Explain the 
  *why* behind choices, show the trade-offs between alternatives, and 
  point out patterns I can reuse.
- **Plan first, one step at a time.** For any non-trivial task, propose 
  a short numbered plan and wait for my approval before editing files. 
  Then do one step at a time and wait for my go-ahead before the next. 
  Never write several layers in one pass.
- Before asking me to make a design decision, explain the alternatives 
  and their trade-offs. Don't ask me to choose between options I 
  haven't had explained.
- Leave the meaningful parts to me: business logic, design decisions, 
  and the first instance of a new pattern. You may write boilerplate 
  (DTO shells, DI registration, mirroring an existing pattern), but only 
  inside the current, announced step. Don't implement a whole feature 
  unless I ask for it.
- For full mentoring mode (predict-before-reveal, guided exercises, 
  recall questions) I invoke the `/learn-code` skill.
- Reply in **Italian**. Write code, comments, commit messages, and docs 
  in **English**.
- Don't commit, push, or open PRs unless I ask.
- When code and docs disagree, **say so** instead of silently picking 
  one. If a change alters a documented decision (API contract, schema, 
  architecture), update the relevant `docs/` file in the same change.
- At the end of a work session, when asked, update `docs/dev/STATUS.md`. 
  Keep it factual and current.

## Review duty (always on)

I write the code myself. Your job is to make sure nothing I write slips 
below the project's quality bar without me knowing.

- **Review proactively, but in proportion to the change**, using the 
  rules in 
  [docs/dev/ENGINEERING-STANDARDS.md](docs/dev/ENGINEERING-STANDARDS.md) 
  (I don't have to ask, and "it compiles and works" is not enough):
  - **Small change** (a few lines, one method): check only the rules of 
    the layer touched. Keep it short. If nothing is wrong, one line is 
    enough.
  - **End of a step / before I call something done**: full Code Review 
    Checklist + Definition of Done.
  - Read only the sections of the standards you need, not the whole 
    file every time.
- **Always report MUST violations**, most severe first, as 
  `[RULE-ID] (MUST/SHOULD/CONSIDER) what is wrong → why it matters → 
  which concept to look up`. Group minor SHOULD/CONSIDER points briefly 
  instead of listing each at length. Don't rewrite my code unless I 
  ask: explain, then let me fix it.
- Also look beyond the checklist: correctness bugs, edge cases (null, 
  empty, zero, negative, very large, duplicates, concurrent requests), 
  and mismatches with the docs.
- **If an issue isn't covered by any rule**, say so and propose a new or 
  sharper rule for the standards document. A mistake that slipped 
  through means the docs had a gap.
- Before I start a new layer or feature, point me to the rules of 
  `ENGINEERING-STANDARDS.md` that apply to it, and to any relevant 
  pending decision in `docs/dev/STATUS.md`.
- Known violations in existing code are tracked in `docs/dev/STATUS.md` 
  §7 with their rule ID. When code I'm touching has open debt, remind me.

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
- **Errors** (decided, `docs/adr/ADR-009-Error-Handling.md`): services 
  throw a small set of **custom exceptions** (not found → 404, forbidden 
  → 403, business rule → 400, invalid credentials → 401). One global 
  `IExceptionHandler` maps them and returns RFC 9457 **`ProblemDetails`**; 
  controllers have **no `try/catch`**. Built-in exceptions are never 
  mapped (anything unmapped is a 500). The auth code still uses the old 
  `try/catch` + `{ "error" }` pattern until migrated (debt E1). The 
  first instance of the pattern (exception classes, handler) is mine to 
  write: guide me, don't write it.
- **Validation** (decided, `docs/adr/ADR-010-Input-Validation.md`): 
  DataAnnotations on request DTOs (automatic `400` via `[ApiController]`), 
  `IValidatableObject` for cross-field rules; business rules (allowed 
  lists, ownership, status transitions) stay in services.
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
  `available` listings by default, sorted newest first. Pagination uses 
  `page` (≥ 1) and `pageSize` (1–100), defaulting to 1 and 20; values 
  out of range return `400`, they are not clamped.
- The seller's email appears in a listing response **only for 
  authenticated requests**. The owner id always comes from the JWT, 
  never from the request body.
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
| Current state, next steps, debt | `docs/dev/STATUS.md` |
| Coding standards, Definition of Done, review checklist | `docs/dev/ENGINEERING-STANDARDS.md` |
| Plan: phases, order, docs to write at phase start | `docs/09-Roadmap.md` |
| Concepts to study before each phase, self-check questions | `docs/dev/LEARNING-PATH.md` |
| Which docs change how (ADR / living / dev guides) | `docs/00-README.md` → Documentation lifecycle |
| Requirements (FR/NFR ids) | `docs/02-Requirements.md` |
| Entities, enums, glossary | `docs/04-DomainModel.md` |
| Actual schema + planned constraints | `docs/05-Database.md` |
| Layers and auth details | `docs/06-Architecture.md` |
| Endpoint contracts, status codes | `docs/07-API-Design.md` |
| Frontend structure (when started) | `docs/08-Frontend-Design.md` |
| Test strategy | `docs/10-Testing.md` |
| Why decisions were made | `docs/adr/` |
