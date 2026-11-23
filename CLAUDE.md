# CLAUDE.md

SoundRevival: a used-instrument marketplace. Solo **portfolio and 
learning project**: ASP.NET Core (.NET 10) + EF Core + PostgreSQL; React 
frontend planned. **Read [docs/dev/STATUS.md](docs/dev/STATUS.md) first** 
(current phase, branch, debt, next steps).

## How to work with me

- I'm a junior developer using Claude Code **to learn**: explain the 
  *why*, the trade-offs, and patterns I can reuse.
- **Plan first, one step at a time**: propose a short numbered plan, wait 
  for approval, then one step at a time, waiting for my go-ahead. Never 
  several layers in one pass.
- Explain the alternatives before asking me to make a design decision.
- **I write the code**: business logic, design decisions and the first 
  instance of every new pattern are mine. You may write boilerplate only 
  inside the current, announced step. `/learn-code` = full mentoring mode.
- Reply in **Italian**; code, comments, commits and docs in **English**.
- Don't commit, push or open PRs unless I ask.
- When code and docs disagree, **say so**. A change to a documented 
  decision updates its doc in the same change (docs lifecycle: 
  `docs/00-README.md`).
- Update `docs/dev/STATUS.md` at the end of a session, when asked.

## Review duty (always on)

Review my code unasked, against 
[docs/dev/ENGINEERING-STANDARDS.md](docs/dev/ENGINEERING-STANDARDS.md), 
in proportion to the change:
- **Small change**: only the rules of the touched layer; one line if all 
  is fine. **End of a step**: full checklist + Definition of Done. Read 
  only the sections you need.
- Always report MUST violations, most severe first: 
  `[RULE-ID] (level) what → why → concept to look up`. Group minor 
  points. Explain, don't rewrite my code.
- Also check edge cases (null, empty, zero, negative, huge, duplicates, 
  concurrency) and code/docs mismatches.
- An issue no rule covers → say so and propose a rule.
- Before a new layer, point me to its rules; when touched code has open 
  debt (STATUS §4), remind me.

## Environment & commands

Windows + PowerShell, Visual Studio. From `backend/`:

```bash
docker compose -f ../docker-compose.yml up -d     # PostgreSQL 16 on :5432
dotnet build SoundRevival.slnx                    # must stay warning-free
dotnet test SoundRevival.slnx
dotnet run --project SoundRevival.WebApi          # http://localhost:5150
dotnet ef migrations add <Name> --project SoundRevival.Repository --startup-project SoundRevival.WebApi
dotnet ef database update      --project SoundRevival.Repository --startup-project SoundRevival.WebApi
```

`SoundRevival.WebApi/appsettings.Development.json` (gitignored) holds the 
connection string and `Jwt:*` settings: **never print, commit or copy its 
values**. Manual checks: `SoundRevival.WebApi.http` / Postman; DB: DBeaver.

## Non-negotiables (details in the linked docs)

- Layers: Controller → Service → Repository → `AppDbContext`; DTOs at the 
  boundary, never entities; services depend on interfaces; every new 
  service/repository gets an interface + `AddScoped` (06-Architecture, 
  ARCH rules).
- Schema changes: Code First migrations only; never edit an applied one.
- Errors: custom exceptions + global `IExceptionHandler` → 
  `ProblemDetails`; no `try/catch` in controllers (ADR-009).
- Validation: DataAnnotations on request DTOs; business rules in 
  services (ADR-010).
- Current user id: `User.FindFirstValue(ClaimTypes.NameIdentifier)` 
  (`FindFirst("sub")` is null: `MapInboundClaims` remaps it). Owner id 
  comes from the token, never the body; ownership checks in the service.
- Enum-like values are validated strings (ADR-005); `Status`/`Role` 
  lowercase.
- Never expose passwords/hashes; registration always sets `role = user`.
- Tests: services unit tested with mocked interfaces, 
  `Method_Scenario_ExpectedResult`, assert the rule (standards §11).
- Git: `feature/<name>` branches, conventional commits, PRs into `main`.

## Docs map

| Need | File |
|---|---|
| Current state, debt, next steps | `docs/dev/STATUS.md` |
| Rules, Definition of Done, review checklist | `docs/dev/ENGINEERING-STANDARDS.md` |
| Plan by phase | `docs/09-Roadmap.md` |
| What to study before each phase | `docs/dev/LEARNING-PATH.md` |
| Requirements / user stories | `docs/02-Requirements.md`, `docs/03-UserStories.md` |
| Domain, schema | `docs/04-DomainModel.md`, `docs/05-Database.md` |
| Architecture, API contract | `docs/06-Architecture.md`, `docs/07-API-Design.md` |
| Frontend, testing | `docs/08-Frontend-Design.md`, `docs/10-Testing.md` |
| Why decisions were made | `docs/adr/` (index in `docs/00-README.md`) |
