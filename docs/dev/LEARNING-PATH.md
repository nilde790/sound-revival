# Learning Path

> **Purpose**: for each roadmap phase, the concepts to understand 
> **before** implementing it, in a sensible order, with official 
> resources and self-check questions. It is a development guide (see the 
> documentation lifecycle in [`../00-README.md`](../00-README.md)): a new 
> phase is added at the start of that phase, so the material is current 
> when it is used.
>
> **How to use it**
> - Read the topics of the current phase in order; each one says where 
>   it shows up in SoundRevival.
> - Study the official resource until you can answer the *Check 
>   yourself* questions **in your own words**, without looking. Then 
>   implement.
> - Claude Code can quiz you on these questions (`/learn-code`).
> - Each topic's **market value**: 🟢 consolidated · 🔵 emerging · 🟣 
>   niche (see [`../09-Roadmap.md`](../09-Roadmap.md)).
> - **Memorize / understand / look up**: you need to *understand* the 
>   concepts. Exact API names can be *looked up* when you need them.

---

## Phase 1 — Backend foundations

Roadmap: [Phase 1](../09-Roadmap.md#phase-1--backend-foundations). 
Rules to keep open while coding: ARCH, VAL, ERR, CONC, DATA, TEST in 
[ENGINEERING-STANDARDS.md](ENGINEERING-STANDARDS.md).

### 1.1 HTTP semantics and REST status codes 🟢
**Where in SoundRevival**: every endpoint in `07-API-Design.md`: why 
`POST` returns `201` + `Location`, why `DELETE` returns `204`, `401` 
vs `403`, `400` vs `404`.
**Resources**: [MDN — HTTP response status codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) · 
[MDN — HTTP request methods](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods)
**Check yourself**
1. What is the difference between `401` and `403`? Give an example of 
   each from SoundRevival.
2. Why is `PUT` idempotent and `POST` not? Why does that matter for 
   retries?
3. Why does `GET /api/listings/{id}` for a missing id return `404`, but 
   a filter that matches nothing return `200` with an empty list?

### 1.2 The ASP.NET Core middleware pipeline 🟢
**Where**: `Program.cs` — the order of `UseExceptionHandler`, 
`UseAuthentication`, `UseAuthorization`, `MapControllers`; it is the 
foundation for the global exception handler (ADR-009).
**Resources**: [ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
**Check yourself**
1. What does a middleware do with the request before and after calling 
   `next`?
2. Why must the exception handler be registered early in the pipeline?
3. What would happen if `UseAuthorization` came before 
   `UseAuthentication`?

### 1.3 Error handling: custom exceptions, `IExceptionHandler`, `ProblemDetails` 🟢
**Where**: ADR-009, the first instance you will write; rules ERR-1..6.
**Resources**: [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors) · 
[RFC 9457 — Problem Details](https://www.rfc-editor.org/rfc/rfc9457)
**Check yourself**
1. Why do we map our own exception types and never 
   `InvalidOperationException`?
2. Trace a `ForbiddenException` thrown in `ListingService` all the way to 
   the JSON the client receives. Which components does it pass through?
3. What does the client see for an unexpected `NullReferenceException`, 
   and where is the real cause recorded?

### 1.4 Model validation with DataAnnotations 🟢
**Where**: listing and auth request DTOs (ADR-010, rules VAL-1..7, the 
field constraints in 07-API-Design.md).
**Resources**: [Model validation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
**Check yourself**
1. Who runs the validation, and when, given `[ApiController]`? Does your 
   action code run if the DTO is invalid?
2. Which rule belongs in a DataAnnotation, and which in the service: 
   "price > 0", "category is in the allowed list", "user owns the 
   listing", "minPrice ≤ maxPrice"? Why?
3. What is over-posting, and how do request DTOs prevent it?

### 1.5 Authentication vs. authorization; resource-based authorization 🟢
**Where**: `[Authorize]` on endpoints, the owner-or-admin check in 
`ListingService` (NFR-2, rule SEC-2), the user id read from claims.
**Resources**: [Introduction to authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction) · 
[Resource-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased)
**Check yourself**
1. Why can't `[Authorize]` alone protect `PUT /api/listings/{id}` from a 
   non-owner?
2. Why must the owner id come from the token and never from the body?
3. Why is the claim read with `ClaimTypes.NameIdentifier` and not 
   `"sub"` in this project?

### 1.6 EF Core: change tracking, queries, pagination, migrations 🟢
**Where**: `ListingRepository`, ADR-007 (no generic `Update`), rules 
DATA-1..6, debt C1, C11, S1.
**Resources**: [Change tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/) · 
[Pagination](https://learn.microsoft.com/en-us/ef/core/querying/pagination) · 
[Migrations overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
**Check yourself**
1. What does `SaveChangesAsync` actually save, and why does calling it 
   on *any* repository commit *all* pending changes of the request?
2. What breaks if `GetListingByIdAsync` uses `AsNoTracking()`?
3. Why must a paginated query have a deterministic `OrderBy` ending with 
   a unique column?
4. When does a LINQ query actually hit the database?

### 1.7 Concurrency basics: races and database constraints 🟢
**Where**: duplicate-email registration (debt C3, rules CONC-1, ERR-3); 
later the `sold` status (CONC-2).
**Resources**: [PostgreSQL — Constraints](https://www.postgresql.org/docs/current/ddl-constraints.html) · 
[EF Core — Handling concurrency conflicts](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
**Check yourself**
1. Describe step by step how two simultaneous registrations with the 
   same email can both pass the "email exists?" check.
2. Why is the unique index the real guard, and the check in code only a 
   nicer error message?
3. What is the difference between pessimistic and optimistic 
   concurrency?

### 1.8 Options pattern and configuration 🟢
**Where**: JWT settings (debt C7), and simpler `AuthService` tests (T2).
**Resources**: [Options pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
**Check yourself**
1. What does `ValidateOnStart` change compared to reading 
   `config["Jwt:Key"]!` at the first login?
2. How does `Options.Create(...)` simplify the unit tests?

### 1.9 async/await and `CancellationToken` 🟢
**Where**: every repository and service method (rules ASYNC-1..3, debt 
A8).
**Resources**: [Asynchronous programming with async and await](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
**Check yourself**
1. Why is `.Result` on a `Task` dangerous in a web app?
2. Where does the `CancellationToken` of a controller action come from, 
   and what happens to a running query when the client disconnects?

### 1.10 Unit testing with xUnit and Moq 🟢
**Where**: `ListingServiceTests`, fixing `AuthServiceTests` (debt T1–T3, 
rules TEST-1..7).
**Resources**: [Unit testing best practices for .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices) · 
[Moq quickstart](https://github.com/devlooped/moq/wiki/Quickstart)
**Check yourself**
1. Why does `Assert.NotNull(result)` not prove that registration assigns 
   role `user`? How would `Verify` with `It.Is<User>(...)` prove it?
2. What is the difference between a stub (`Setup` + `Returns`) and a 
   verified interaction (`Verify`)? When do you need each?
3. Why does xUnit create a new instance of the test class for every 
   test, and how does that make constructor setup safe?

---

## Phase 2 — Delivery pipeline

Roadmap: [Phase 2](../09-Roadmap.md#phase-2--delivery-pipeline-walking-skeleton). 
Topics follow the roadmap's branch order: study each one just before 
its item. `14-DevOps.md` and the deployment ADR are written before the 
deploy item; read them with 2.11–2.13.

### 2.0 Dependency inversion: Core and Infrastructure 🟢
**Where**: [ADR-011](../adr/ADR-011-Core-Infrastructure-Split.md), the 
first branch of the phase.
**Resources**: [Common web application architectures (Microsoft)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
**Check yourself**
1. Why does `IUserRepository` live in Core, where it is used, and not in 
   Infrastructure, where it is implemented?
2. After the split, what happens if a service adds 
   `using Npgsql;`? Why is that better than rule ARCH-2 alone?
3. Why can the `Migrations/` folder move to another project without 
   touching the database?

### 2.1 Build quality: shared build settings, central package versions, reproducible restores 🟢
**Where**: debts B1–B3, B6 (`global.json`, `Directory.Build.props`, 
`Directory.Packages.props`, `packages.lock.json`, `.editorconfig`).
**Resources**: [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) · 
[dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) · 
[global.json](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) · 
[Lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
**Check yourself**
1. What problem does Central Package Management solve? Give the example 
   already present in this repository. What does transitive pinning add?
2. Why turn warnings into errors instead of "trying to keep them at 
   zero"?
3. Without `global.json`, which SDK builds the project on your machine 
   and which one in CI?
4. What does a lock file guarantee that a version number in the project 
   file does not?

### 2.2 Test platform: xUnit v3 and Microsoft.Testing.Platform 🔵
**Where**: debt T5, `SoundRevival.UnitTests`.
**Resources**: [Microsoft.Testing.Platform overview](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro) · 
[xUnit v3: what's new](https://xunit.net/docs/getting-started/v3/whats-new)
**Check yourself**
1. With Microsoft.Testing.Platform, what is the test project when you 
   build it? How does that differ from VSTest?
2. What changes for coverage collection?

### 2.3 Continuous integration with GitHub Actions 🟢
**Where**: `.github/workflows/`, NFR-9/NFR-10, a ruleset on `main`.
**Resources**: [GitHub Actions documentation](https://docs.github.com/en/actions) · 
[Rulesets](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets)
**Check yourself**
1. What are a workflow, a job, a step and a runner?
2. What should make a PR impossible to merge, and where is that 
   configured?
3. Why run `dotnet format --verify-no-changes` in CI instead of relying 
   on everyone's editor?

### 2.4 Software supply-chain security 🔵
**Where**: the CI item (Dependabot, CodeQL, secret scanning, actions 
pinned by SHA, `permissions:`); rule SEC-7.
**Resources**: [Security hardening for GitHub Actions](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions) · 
[Dependabot](https://docs.github.com/en/code-security/dependabot)
**Check yourself**
1. Why pin a third-party action to a commit SHA instead of a tag like 
   `@v4`?
2. What can a workflow do with the default token permissions, and why 
   reduce them?
3. Dependabot opens a PR that updates a package: what has to happen 
   before you merge it?

### 2.5 Integration testing: `WebApplicationFactory` + Testcontainers 🟢
**Where**: debt T4, [`../10-Testing.md`](../10-Testing.md) → Integration 
tests.
**Resources**: [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests) · 
[Testcontainers for .NET](https://dotnet.testcontainers.org/)
**Check yourself**
1. What does `WebApplicationFactory` start, and what does it *not* need 
   (no port, no deployment)?
2. Why a real PostgreSQL in a container instead of EF Core's in-memory 
   provider? Name one bug only the real database would catch here.
3. How do tests stay independent if they share one database container?

### 2.6 Docker: images, multi-stage builds, compose 🟢
**Where**: the API Dockerfile, `docker-compose.yml` with API + DB 
(NFR-8).
**Resources**: [Containerize a .NET app](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container) · 
[Docker multi-stage builds](https://docs.docker.com/build/building/multi-stage/)
**Check yourself**
1. Image vs. container: what is the difference?
2. Why does a multi-stage build produce a smaller and safer image? Why 
   run it as a non-root user?
3. Inside compose, why does the API reach the database at `postgres:5432` 
   and not `localhost:5432`?
4. What is the difference between "the DB container started" and "the 
   DB is ready", and how does compose wait for the second?

### 2.7 Configuration, secrets, proxies and logs across environments 🟢
**Where**: rules SEC-4, CFG-2, debt C10; the deployed environment's 
variables; the platform's reverse proxy; JSON console logs (NFR-15).
**Resources**: [Safe storage of app secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) · 
[Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/) · 
[Proxies and load balancers](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer) · 
[Console log formatting](https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter)
**Check yourself**
1. In which order does ASP.NET Core read configuration sources, and why 
   do environment variables override `appsettings.json`?
2. How does `Jwt__Key` (double underscore) map to `Jwt:Key`?
3. Behind the platform's proxy, HTTPS ends before the app: what does the 
   app see as scheme and client IP without forwarded headers? Which 
   later feature breaks (phase 3 rate limiting)?
4. Why are JSON logs easier to search in the cloud than plain text?

### 2.8 Health checks: liveness and readiness 🟢
**Where**: `/health/live`, `/health/ready` (NFR-15), used by the hosting 
platform.
**Resources**: [Health checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
**Check yourself**
1. What is the difference between "the process is up" and "the app can 
   serve requests"? What does the platform do when each one fails?
2. Why must liveness not check the database?
3. Why should health endpoints not require authentication, and what 
   must they not reveal?

### 2.9 CORS 🟢
**Where**: NFR-13, rule SEC-8, debt C9, ADR-012 (credentials).
**Resources**: [Enable CORS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors) · 
[MDN — CORS](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)
**Check yourself**
1. Who enforces CORS: the browser or the server? Why does Postman ignore 
   it?
2. Why is `AllowAnyOrigin` with credentials forbidden?
3. "Same origin" and "same site" are different: give an example of two 
   URLs that are same site but not same origin.

### 2.10 OpenAPI and interactive API docs 🟢
**Where**: debt B4, the public API docs of the walking skeleton.
**Resources**: [OpenAPI support in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview) · 
[Scalar](https://github.com/scalar/scalar)
**Check yourself**
1. What is the difference between the OpenAPI *document* and the UI 
   that renders it?
2. How does a client try an authenticated endpoint from the docs UI?
3. Why is an accurate OpenAPI document also a contract for the frontend 
   (types generated from it in phase 4)?

### 2.11 Deployment fundamentals 🟢
**Where**: the deployment ADR, written before the deploy item.
**Resources**: chosen with the ADR (official docs of the selected 
platform).
**Check yourself**
1. PaaS vs. container platform vs. virtual machine: who manages what?
2. What happens to files written to the container's disk on redeploy, 
   and why does that justify external image storage?
3. Which costs and limits of the free tier could break the demo, and how 
   would you notice?

### 2.12 Infrastructure as code 🔵
**Where**: the deploy item; tool (Bicep or Terraform) chosen in the 
deployment ADR.
**Resources**: [Bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview) · 
[Terraform](https://developer.hashicorp.com/terraform/intro)
**Check yourself**
1. What does "declarative" mean here, and what happens when you apply 
   the same file twice?
2. Why is an environment described in code easier to review, repeat and 
   delete than one built by clicking in a portal?

### 2.13 Continuous delivery: OIDC login and migrations in the pipeline 🔵
**Where**: the CD workflow of the deploy item.
**Resources**: [OpenID Connect in GitHub Actions](https://docs.github.com/en/actions/security-for-github-actions/security-hardening-your-deployments/about-security-hardening-with-openid-connect) · 
[EF Core migration bundles](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying#bundles)
**Check yourself**
1. With OIDC, which secret is no longer stored in GitHub, and what 
   proves the workflow's identity instead?
2. Why apply migrations as a pipeline step instead of calling 
   `Database.Migrate()` when the app starts? Think of two replicas 
   starting together.
3. What does a smoke test after the deploy catch that CI can't?

---

*Phases 3–6 are added at the start of each phase (see 
[`../09-Roadmap.md`](../09-Roadmap.md)).*
