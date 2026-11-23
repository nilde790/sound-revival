
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
- **Debt ids** (C1, A3, T1…) refer to [dev/STATUS.md](dev/STATUS.md) §4.
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

**Status**: ✅ done (2026-11-23). Merged into `main` 
([PR #3](https://github.com/nilde790/sound-revival/pull/3)).

**Goal**: complete listings CRUD with the quality patterns every later 
feature will reuse: validation, error handling, authorization, tests.

**Learn first**: [Learning path → Phase 1](dev/LEARNING-PATH.md#phase-1--backend-foundations) 
· **Write at phase start**: — (standards and ADR-009/010 already exist)

| Status | Item | Refs |
|---|---|---|
| ✅ | Listing repository with filtering + pagination | — |
| ✅ | Listing DTOs with DataAnnotations (create, update, status, response, paged response) | ADR-010, C4 |
| ✅ | Error handling infrastructure: custom exceptions + global `IExceptionHandler` + `ProblemDetails` | ADR-009 |
| ✅ | Migrate auth to the new error pattern; validate auth DTOs | E1, C4 |
| ✅ | `ListingService`: ownership/admin checks, allowed values, status rules | FR-6..10, FR-18 |
| ✅ | `ListingsController` (CRUD + `PATCH /status`) | 07-API-Design |
| ✅ | Fix correctness debt in touched code: bounded pagination, email normalization, duplicate-email race, LIKE escaping, sort (`newest` default, `price_asc`, `price_desc`), filter values validated | C1, C2, C3, C6, C11, C14 |
| ✅ | Options pattern for JWT settings; token generation behind its own interface | C7, A3 |
| ✅ | Unit tests for `ListingService`, `AuthService`, `JwtTokenService` and every request DTO (220) | T1, T2, T3 |

**Done when**: every listings endpoint in 07-API-Design.md works as 
documented (status codes and `ProblemDetails` bodies included); the 
acceptance criteria of US-5, US-7..US-13, US-16, US-18 pass; the 
Definition of Done holds; debts C1–C4, C6, C11, E1 and T1 are closed.

**Skills**: REST API design 🟢 · ASP.NET Core middleware & error 
handling 🟢 · EF Core 🟢 · authorization 🟢 · unit testing with mocks 🟢

### Phase 2 — Delivery pipeline (walking skeleton)

**Status**: 🚧 current phase. Plan reviewed on 2026-11-23 against the 
whole project and current practice; decisions taken before starting: 
[ADR-011](adr/ADR-011-Core-Infrastructure-Split.md) (project 
structure) and [ADR-012](adr/ADR-012-Browser-Token-Storage.md) (token 
storage, which constrains the deployment).

**Goal**: every push is built, checked and tested automatically, and the 
backend runs online with its database, logs and interactive API docs.

**Learn first**: [Learning path → Phase 2](dev/LEARNING-PATH.md#phase-2--delivery-pipeline) 
· **Write before the deploy item** (the earlier items don't depend on 
the platform): `14-DevOps.md`; ADR on the deployment platform (PaaS vs. 
Azure/AWS: cost, free-tier limits, professional value). The ADR must 
cover: frontend and API on the same site (ADR-012); managed PostgreSQL 
with `pgvector` available (phase 6), on one major version used 
everywhere (compose, Testcontainers, cloud); the infrastructure-as-code 
tool (Bicep or Terraform); the image registry; pipeline login to the 
cloud through OIDC (no stored passwords).

Items in branch order:

| Status | Item | Refs |
|---|---|---|
| ⬜ | Project structure: split `SoundRevival.Repository` into `Core` + `Infrastructure`; `SoundRevival.Tests` → `SoundRevival.UnitTests` | ADR-011, A1 |
| ⬜ | Build quality and reproducibility: `global.json` (SDK pin), `Directory.Build.props` (`TreatWarningsAsErrors`, analyzers, `EnforceCodeStyleInBuild`), Central Package Management with transitive pinning, lock files, `.editorconfig` + one formatting-only commit (listed in `.git-blame-ignore-revs`), latest patch versions | B1, B2, B3, B6 |
| ⬜ | Test platform: xUnit v3 + Microsoft.Testing.Platform, coverage | T5, 10-Testing |
| ⬜ | GitHub Actions CI: locked restore, build, format check, unit tests, coverage summary on PRs; least-privilege permissions, actions pinned by commit SHA; Dependabot (NuGet, Actions, Docker), CodeQL, secret scanning; a ruleset on `main` that requires the checks | NFR-9, NFR-10 |
| ⬜ | Integration tests: `SoundRevival.IntegrationTests` with `WebApplicationFactory` + Testcontainers, data reset between tests, own CI job | T4, 10-Testing |
| ⬜ | Containers and configuration: multi-stage Dockerfile (non-root user, minimal base image), `.dockerignore`; `docker compose up` runs API + DB (API starts when the DB is healthy, values from `.env`); `dotnet user-secrets` locally, environment variables when deployed; forwarded headers behind the platform's proxy; structured JSON console logs | NFR-8, NFR-15, SEC-4, CFG-2, C10 |
| ⬜ | Operational endpoints: `/health/live` and `/health/ready` (database); public OpenAPI document + Scalar with Bearer auth; CORS policy for the frontend origin | NFR-15, B4, NFR-13, C9 |
| ⬜ | Deploy: `14-DevOps.md` + deployment ADR; infrastructure as code; CD workflow (image pushed to a registry, OIDC login, EF Core migration bundle applied by the pipeline, smoke test on `/health/ready`); README shows the live API docs link | ADR (deploy) |

**Done when**: a PR to `main` can't merge with a failing build, format, 
test or security check; `docker compose up` starts the whole backend 
locally; the deployed API answers `/health/ready` and its docs are 
public; the cloud environment can be recreated from code; schema 
changes reach the deployed database only through the pipeline.

**Skills**: CI/CD 🟢 · Docker 🟢 · integration testing 🟢 · cloud 
deployment fundamentals 🟢 · infrastructure as code 🔵 · software 
supply-chain security 🔵

### Phase 3 — API completion

**Goal**: the remaining v1 backend features, plus abuse protection and 
realistic data.

**Learn first**: learning path extended at phase start 
· **Write at phase start**: `12-Security.md` (includes the refresh token 
details of ADR-012); ADR on image storage (provider, e.g. Cloudinary or 
the deployment cloud's blob storage; upload flow through the backend or 
directly to the storage with a short-lived signed URL)

Sort options (FR-21) and the seller's email only for authenticated 
visitors (FR-15, US-16, US-17) were completed in phase 1.

| Status | Item | Refs |
|---|---|---|
| ⬜ | Housekeeping before new endpoints, so they don't copy known flaws: repository query type instead of HTTP DTOs, `CancellationToken` propagation, timestamps from `TimeProvider`, one `ProblemDetails` shape, one normalization rule for enum-like input (body and query alike); decide UUIDv7 ids (`Guid.CreateVersion7()`) | A2, A7, A8, A9, C15 |
| ⬜ | Schema migration: column limits, normalized email (with a data update), `Brand` column | S1, 05-Database |
| ⬜ | Brand: field on create/update/responses, filter on `GET /api/listings` | FR-6, FR-20, US-14 |
| ⬜ | `GET/PUT /api/users/me` | FR-4, US-4 |
| ⬜ | Refresh tokens: `RefreshTokens` table, `POST /api/auth/refresh` and `/logout`, rotation with reuse detection, `HttpOnly` cookie; short-lived access token | ADR-012, FR-3 |
| ⬜ | JWT role claim under a key the frontend can read (`role`) | C13 |
| ⬜ | Image upload/delete (max 5, owner only); stored files deleted with their listing | FR-19, US-6, US-8 |
| ⬜ | Rate limiting on login/register per client IP (read through the forwarded headers set up in phase 2); constant-time login failure | NFR-11, C5, C8 |
| ⬜ | Seed data: 1,000 realistic listings, loaded only in local/dev environments | NFR-4 |

**Done when**: all v1 endpoints in 07-API-Design.md are implemented, 
tested and deployed; the seed dataset is ready for the NFR-4 
measurement (phase 5).

**Skills**: security hardening (OWASP API Top 10) 🟢 · token lifecycle 
(refresh, rotation, revocation) 🟢 · third-party API integration 🟢 · 
database migrations 🟢

### Phase 4 — Frontend

**Goal**: a usable React application on top of the deployed API, 
shipped continuously.

**Learn first**: learning path extended at phase start 
· **Write at phase start**: review and update `08-Frontend-Design.md` 
(styling approach, forms and client-side validation library, API type 
generation)

| Status | Item | Refs |
|---|---|---|
| ⬜ | Project setup (Vite + React + TypeScript), ESLint, type check and tests in CI; API types generated from the backend's OpenAPI document | NFR-9, 07-API-Design |
| ⬜ | Routing + auth flow (register, login, logout, protected routes); access token in memory, refresh on reload and on `401` | US-1..US-3, ADR-012 |
| ⬜ | Homepage: listings grid, filters, sort, pagination | US-10..US-15 |
| ⬜ | Listing detail (seller contact for logged-in users) | US-16, US-17 |
| ⬜ | Create/edit listing form with image upload | US-5..US-7 |
| ⬜ | Profile page | US-4 |
| ⬜ | Responsive layout and accessibility basics | NFR-5, NFR-14 |
| ⬜ | Component tests (Vitest + React Testing Library) | 10-Testing |
| ⬜ | Frontend deployed on the same site as the API, with a Content Security Policy; README gets screenshots and the live link | ADR-012 |

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
| ⬜ | Structured logging everywhere, each log line carrying the request's trace id (W3C trace context, no hand-made correlation id) | NFR-15, LOG-1..4 |
| ⬜ | OpenTelemetry traces and metrics, exported to a free backend | — |
| ⬜ | Load test against NFR-4 on the seed data (e.g. k6); indexes or caching only if measurements require them | NFR-4, DATA-6 |
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
- Natural-language search assistant: an LLM turns free text ("Les Paul 
  around €700, not damaged") into the existing structured filters 
  (brand, model, price, condition); voice input later via 
  speech-to-text. See [Product ideas](#product-ideas--differentiators-exploratory-not-committed)
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

Token refresh and server-side revocation moved to v1, phase 3 
([ADR-012](adr/ADR-012-Browser-Token-Storage.md)).

## Stretch (optional, after v2 or when a phase needs it)

Infrastructure as code moved into phase 2 (deploy item).

- Container image signing and an SBOM (software bill of materials) in 
  the CD workflow 🔵
- Background jobs (`BackgroundService` + Channels, or Hangfire) — e.g. 
  image processing, embeddings for semantic search 🟢
- Caching with `HybridCache`, only if measurements justify it 🟢
- .NET Aspire for local orchestration and the telemetry dashboard 🟣

## Product ideas — differentiators (exploratory, not committed)

Ideas that could make SoundRevival stand out from existing music-gear 
marketplaces, collected on 2026-11-17. They are **not committed scope**: 
none has requirements or an ADR yet. If one is adopted, it follows the 
v2 rule (requirements in 02-Requirements.md first, ADR where there are 
trade-offs).

Competitor check (web search, November 2026: Reverb, eBay, Mercatino 
Musicale, Subito, Resyn, generic barter apps). A search can show that a 
feature exists, not that it is missing: re-check before committing.

| # | Idea | Competitors (Nov 2026) | Notes |
|---|---|---|---|
| 1 | **Natural-language search assistant** (text, later voice) | eBay is rolling out conversational search (small share of US users); Reverb has no native one; none found on Subito / Mercatino Musicale | Advantage in the Italian music niche, easy to copy. Fits Phase 6 (LLM → filter DTO → existing search) |
| 2 | **Multi-party trade matching** (A wants B's item, B wants C's, C wants A's) | Not found in music marketplaces; Resyn supports trade offers between two users only; 2–4-party cycles exist in generic barter apps (xwap, SwapIt) | Most original idea. Algorithm = cycle detection in a directed graph (like kidney exchange). Hard parts: execution (a party drops out, value balancing with cash, simultaneous shipping, escrow) and cold start (few users → few cycles). Depends on 3 and 6 |
| 3 | **Listing open to sale and/or trade**, with the gear categories accepted in exchange | Mercatino Musicale: trade availability appears only as free text in the description (no structured field found) | Cheap, useful on its own, and the data foundation for 2. Best first candidate |
| 4 | **Gear value** (estimated market value) | Reverb Price Guide (from completed sales); Sweetwater Price Advisor | Not a differentiator; needs completed-sale data a new marketplace lacks (asking prices are biased). Useful as a support tool to balance trades |
| 5 | **Audio demos** on listings, with a mandatory list of the recording chain (transparency) | Reverb: only embedded YouTube videos; no native audio or mandatory chain disclosure found | Distinctive and cheap (upload + recording-chain table). Disclosed chain is not verifiable; needs storage and moderation |
| 6 | **Smart wishlists and predictive notifications** | Reverb: saved searches, watch list, price-drop alerts | Plain alerts are standard. Distinctive when the wishlist feeds trade matching (the "wants" edges of 2). Prediction needs historical data |

The strongest combination is **3 + 6 + 2**: the listing says what a user 
offers, the wishlist says what they want, the engine finds the chain. 
5 is a low-cost distinctive touch; 1 and 4 are expected rather than 
differentiating.

## Explicitly out of scope (no plans to implement)

- Real payment processing
- Real shipping/logistics integration
- Multi-language support
- Microservices, Kubernetes: no requirement of this project needs them, 
  and a monolith with clear layers is the right architecture at this 
  scale
