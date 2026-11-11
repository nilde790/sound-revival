# Roadmap

The plan: **what** gets built, in **which order**, and **what to learn 
first**. The detailed snapshot of the current work (active branch, 
uncommitted files, open debt) lives in [dev/STATUS.md](dev/STATUS.md).

## How to read this roadmap

- **Phases are done in order**, one feature per branch/PR. A phase is 
  finished when its *Done when* criteria hold, not when its list is 
  ticked.
- **Deploy early** (decided 2026-11-11): the backend goes online at the 
  end of phase 2 as a "walking skeleton", and every later phase ships to 
  the deployed environment as soon as it is done (continuous delivery).
- **Learn first**: each phase links to the concepts to study *before* 
  implementing, in [dev/LEARNING-PATH.md](dev/LEARNING-PATH.md). The 
  learning path is extended at the start of each phase.
- **Docs just-in-time**: technical documents and ADRs listed under 
  *Write at phase start* are written when the phase begins, not before.
- **Debt ids** (C1, A3, T1…) refer to [dev/STATUS.md](dev/STATUS.md) §7.
- **Market value** of the main skills, from the November 2026 audit: 
  🟢 consolidated (asked almost everywhere) · 🔵 emerging (growing 
  demand) · 🟣 niche or context-specific.

Status legend: ✅ done · 🚧 in progress · ⬜ not started

---

## v1 — MVP

### Phase 0 — Authentication

**Status**: ✅ done. Solution setup, EF Core Code First schema, JWT register/login with 
BCrypt, first unit tests. Merged into `main` ([PR #1](https://github.com/nilde790/sound-revival/pull/1)).

### Phase 1 — Backend foundations

**Status**: 🚧 current phase.

**Goal**: complete listings CRUD with the quality patterns every later 
feature will reuse: validation, error handling, authorization, tests.

**Learn first**: [Learning path → Phase 1](dev/LEARNING-PATH.md#phase-1--backend-foundations) 
· **Write at phase start**: — (standards and ADR-009/010 already exist)

| Status | Item | Refs |
|---|---|---|
| ✅ | Listing repository with filtering + pagination | — |
| 🚧 | Listing DTOs with DataAnnotations (create, update, status, response, paged response) | ADR-010, C4 |
| ⬜ | Error handling infrastructure: custom exceptions + global `IExceptionHandler` + `ProblemDetails` | ADR-009 |
| ⬜ | Migrate auth to the new error pattern; validate auth DTOs | E1, C4 |
| ⬜ | `ListingService`: ownership/admin checks, allowed values, status rules | FR-6..10, FR-18 |
| ⬜ | `ListingsController` (CRUD + `PATCH /status`) | 07-API-Design |
| ⬜ | Fix correctness debt in touched code: bounded pagination, email normalization, duplicate-email race, LIKE escaping, default sort newest | C1, C2, C3, C6, C11 |
| ⬜ | Options pattern for JWT settings; token generation behind its own interface | C7, A3 |
| ⬜ | Unit tests for `ListingService`; fix `AuthServiceTests` | T1, T2, T3 |

**Done when**: every listings endpoint in 07-API-Design.md works as 
documented (status codes and `ProblemDetails` bodies included); the 
acceptance criteria of US-5, US-7..US-13, US-16, US-18 pass; the 
Definition of Done holds; debts C1–C4, C6, C11, E1 and T1 are closed.

**Skills**: REST API design 🟢 · ASP.NET Core middleware & error 
handling 🟢 · EF Core 🟢 · authorization 🟢 · unit testing with mocks 🟢

### Phase 2 — Delivery pipeline (walking skeleton)

**Goal**: every push is built and tested automatically, and the backend 
runs online with its database and interactive API docs.

**Learn first**: [Learning path → Phase 2](dev/LEARNING-PATH.md#phase-2--delivery-pipeline) 
· **Write at phase start**: `14-DevOps.md`; ADR on the deployment 
platform (PaaS vs. Azure/AWS: cost, free-tier limits, professional value)

| Status | Item | Refs |
|---|---|---|
| ⬜ | Build quality: `Directory.Build.props` (`TreatWarningsAsErrors`), Central Package Management, `.editorconfig` + `dotnet format` | B1, B2, B3 |
| ⬜ | GitHub Actions CI: build, format check, unit tests, coverage; branch protection on `main` | NFR-9, NFR-10 |
| ⬜ | Integration tests: `WebApplicationFactory` + Testcontainers, run in CI | T4, 10-Testing |
| ⬜ | Dockerfile (multi-stage) for the API; `docker compose up` runs API + DB | NFR-8 |
| ⬜ | Configuration per environment; secrets via environment variables | SEC-4, CFG-2, C10 |
| ⬜ | `GET /health` (API + database) | NFR-15 |
| ⬜ | OpenAPI UI (Scalar) with Bearer auth | B4 |
| ⬜ | CORS policy for the future frontend origin | NFR-13, C9 |
| ⬜ | Deploy backend + database; README shows the live API docs link | ADR (deploy) |

**Done when**: a PR to `main` can't merge with a failing build/test; 
`docker compose up` starts the whole backend locally; the deployed API 
answers `/health` and its docs are public.

**Skills**: CI/CD 🟢 · Docker 🟢 · integration testing 🟢 · cloud 
deployment fundamentals 🟢

### Phase 3 — API completion

**Goal**: the remaining v1 backend features, plus abuse protection and 
realistic data.

**Learn first**: learning path extended at phase start 
· **Write at phase start**: `12-Security.md`; ADR on image storage 
(Cloudinary vs. S3/Azure Blob)

| Status | Item | Refs |
|---|---|---|
| ⬜ | Schema migration: column limits, normalized email, `Brand` column | S1, 05-Database |
| ⬜ | Brand filter and sort options on `GET /api/listings` | FR-20, FR-21 |
| ⬜ | Seller info in listing detail (email only when authenticated) | FR-15, US-16, US-17 |
| ⬜ | `GET/PUT /api/users/me` | FR-4, US-4 |
| ⬜ | Image upload/delete (max 5, owner only) | FR-19, US-6 |
| ⬜ | Rate limiting on login/register; constant-time login failure | NFR-11, C5, C8 |
| ⬜ | Seed data: 1,000 realistic listings | NFR-4 |

**Done when**: all v1 endpoints in 07-API-Design.md are implemented, 
tested and deployed; NFR-4 is measured on seed data.

**Skills**: security hardening (OWASP API Top 10) 🟢 · third-party API 
integration 🟢 · database migrations 🟢

### Phase 4 — Frontend

**Goal**: a usable React application on top of the deployed API, 
shipped continuously.

**Learn first**: learning path extended at phase start 
· **Write at phase start**: review and update `08-Frontend-Design.md`

| Status | Item | Refs |
|---|---|---|
| ⬜ | Project setup (Vite + React + TypeScript), ESLint, type check and tests in CI | NFR-9 |
| ⬜ | Routing + auth flow (register, login, logout, protected routes) | US-1..US-3 |
| ⬜ | Homepage: listings grid, filters, sort, pagination | US-10..US-15 |
| ⬜ | Listing detail (seller contact for logged-in users) | US-16, US-17 |
| ⬜ | Create/edit listing form with image upload | US-5..US-7 |
| ⬜ | Profile page | US-4 |
| ⬜ | Responsive layout and accessibility basics | NFR-5, NFR-14 |
| ⬜ | Component tests (Vitest + React Testing Library) | 10-Testing |
| ⬜ | Frontend deployed; README gets screenshots and the live link | — |

**Done when**: all v1 user stories pass in the deployed application.

**Skills**: React + TypeScript 🟢 · server-state management (React 
Query) 🟢 · frontend testing 🟢

### Phase 5 — Observability, performance and end-to-end tests

**Goal**: know what the running system is doing, prove the critical 
journeys work, measure before optimizing.

**Write at phase start**: `13-Observability.md`, 
`16-Performance-and-Concurrency.md`; ADR on the observability stack

| Status | Item | Refs |
|---|---|---|
| ⬜ | Structured logging everywhere; correlation id per request | NFR-15, LOG-1..4 |
| ⬜ | OpenTelemetry traces and metrics, exported to a free backend | — |
| ⬜ | Performance check against NFR-4; indexes or caching only if measurements require them | NFR-4, DATA-6 |
| ⬜ | End-to-end tests (Playwright) for the critical journeys, in CI | 10-Testing |

**Skills**: observability / OpenTelemetry 🔵 · performance analysis 🟢 
· E2E testing 🟢

### Phase 6 — AI integration

**Goal**: add AI features justified by the domain, built with 
professional practices (provider abstraction, cost control, evaluation, 
safety).

**Write at phase start**: `15-AI-Integration.md`; ADR choosing the 
feature(s) and provider

Candidates, each to be evaluated in 15-AI-Integration.md (problem, 
value, skills, integration, alternatives, cost, worth the complexity?):
- Listing description assistant (from keywords or photos)
- Semantic search ("beginner guitar under €300") with `pgvector`
- Automatic moderation of listing text
- Price suggestion from similar listings
- A chatbot (expected to be the least justified for a marketplace)

**Skills**: LLM integration, RAG, evaluation 🔵

---

## v2 — Committed scope (after v1, see ADR-008)

Built on top of a finished, deployed v1, one feature per branch/PR. Each 
feature starts by adding its requirements to 02-Requirements.md and an 
ADR where there are trade-offs.

| Status | Feature | Notes |
|---|---|---|
| ⬜ | Real-time **in-app messaging** between buyers and sellers (SignalR) | 🟣 real-time is .NET-specific in this form; WebSockets concepts are general |
| ⬜ | **Simulated payment flow** (provider test mode, no real money) | A purchase sets the listing to `sold` via `reserved`: status becomes a state machine; optimistic concurrency required (CONC-2) |
| ⬜ | **Admin moderation dashboard** (flagged listings, user management) | Builds on FR-18 |
| ⬜ | **Dynamic, admin-managed categories** | Supersedes ADR-005 for categories; schema + data migration |
| ⬜ | **Token refresh** and server-side revocation | Supersedes part of ADR-003; completes FR-3 |

## Stretch (optional, after v2 or when a phase needs it)

- Infrastructure as Code (Bicep or Terraform) for the deployed 
  environment 🔵
- Background jobs (`BackgroundService` + Channels, or Hangfire) — e.g. 
  image processing, embeddings for semantic search 🟢
- Caching with `HybridCache`, only if measurements justify it 🟢
- .NET Aspire for local orchestration and the telemetry dashboard 🟣

## Explicitly out of scope (no plans to implement)

- Real payment processing
- Real shipping/logistics integration
- Multi-language support
- Microservices, Kubernetes: no requirement of this project needs them, 
  and a monolith with clear layers is the right architecture at this 
  scale
