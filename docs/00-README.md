# Documentation Index

This folder contains the full documentation for SoundRevival, written 
before and alongside implementation. Read in this order:

1. [01-Vision.md](01-Vision.md) — the problem this project solves, for 
   whom, and why it exists
2. [02-Requirements.md](02-Requirements.md) — functional and 
   non-functional requirements
3. [03-UserStories.md](03-UserStories.md) — features described from the 
   user's perspective
4. [04-DomainModel.md](04-DomainModel.md) — core entities, relationships, 
   and domain glossary
5. [05-Database.md](05-Database.md) — database schema and design choices
6. [06-Architecture.md](06-Architecture.md) — tech stack, high-level 
   architecture, and communication flow
7. [07-API-Design.md](07-API-Design.md) — REST endpoint contracts
8. [08-Frontend-Design.md](08-Frontend-Design.md) — frontend folder 
   structure, routing, and state management
9. [09-Roadmap.md](09-Roadmap.md) — the plan: phases in order, what to 
   learn first, docs to write at each phase start
10. [10-Testing.md](10-Testing.md) — testing strategy for backend and 
    frontend

## Architecture Decision Records

The reasoning behind major technical decisions, following the template 
in [ADR-000-Template.md](adr/ADR-000-Template.md).

| ADR | Decision | Status |
|---|---|---|
| [001](adr/ADR-001-Architecture.md) | Separate C# backend and React frontend | Accepted |
| [002](adr/ADR-002-Database.md) | PostgreSQL with UUID primary keys | Accepted |
| [003](adr/ADR-003-Authentication.md) | JWT-based authentication | Accepted |
| [004](adr/ADR-004-Multi-Project-Layered-Backend.md) | Multi-project layered backend solution | Accepted |
| [005](adr/ADR-005-Enum-Values-As-Strings.md) | Enum-like values stored as validated strings | Accepted |
| [006](adr/ADR-006-Dedicated-Listing-Status-Endpoint.md) | Dedicated endpoint for listing status changes | Accepted |
| [007](adr/ADR-007-No-Generic-Update-In-Repositories.md) | No generic `Update` method in repositories | Accepted |
| [008](adr/ADR-008-V2-Committed-Scope.md) | v2 is committed scope, built after v1 | Accepted |
| [009](adr/ADR-009-Error-Handling.md) | Error handling: exception types, mapping, response format | Accepted |
| [010](adr/ADR-010-Input-Validation.md) | Input validation approach | Accepted |

ADRs for later decisions (image storage, deployment platform, 
observability stack, AI features, background jobs) are written 
just-in-time, at the start of the roadmap phase that needs them (see 
[09-Roadmap.md](09-Roadmap.md)).

## Development guides

The [`dev/`](dev/) folder holds the documents that guide day-to-day 
development (for the developer and for Claude Code). They describe how 
we work, not what the product is:

- [dev/STATUS.md](dev/STATUS.md) — compact snapshot of what is built, 
  what is in progress, and known technical debt. Updated at the end of 
  every work session.
- [dev/ENGINEERING-STANDARDS.md](dev/ENGINEERING-STANDARDS.md) — coding 
  standards per layer, Definition of Done, and the code review checklist 
  every change is checked against.
- [dev/LEARNING-PATH.md](dev/LEARNING-PATH.md) — for each roadmap phase, 
  the concepts to study before implementing it, with official resources 
  and self-check questions. Extended at the start of each phase.

Working conventions for contributors (and for Claude Code) are in 
[`/CLAUDE.md`](../CLAUDE.md).

## Documentation lifecycle

Not all documents change in the same way. Each category has its own rule:

| Category | Where | How it changes | Audience |
|---|---|---|---|
| **Decision records** (ADRs) | `docs/adr/` | **Immutable.** An `Accepted` ADR is never rewritten. If a decision changes, a new ADR supersedes it, and the old one only gets its status changed to `Superseded by ADR-0xx`. | Anyone who wants to know *why* the system is built this way |
| **Product & technical docs** (vision, requirements, domain, database, architecture, API, frontend, testing, …) | `docs/` (numbered files) | **Living.** They describe the system: the target design, with the real implementation state marked explicitly (✅ / 🚧 / ⬜). They are updated **in the same branch/PR** as the code that changes them (docs-as-code). A doc that disagrees with the code is a bug. | Recruiters, developers, future me |
| **Development guides** (status, standards, learning material) + `CLAUDE.md` + Claude Code skills | `docs/dev/`, repository root, `.claude/` | **Dynamic.** Updated often: every work session (status) or whenever a recurring mistake shows a gap (standards). | The developer and the AI assistant |

In practice: the **what** and the **how** live in the living docs, the 
**why** lives in ADRs. When a design changes, update the living doc 
*and* add an ADR explaining the change. Neither loses information.