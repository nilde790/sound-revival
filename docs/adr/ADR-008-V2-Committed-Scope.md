# ADR-008: v2 is committed scope, built after v1

- **Status**: Accepted
- **Date**: 2026-11-11

## Context

The original vision listed in-app messaging, payments and moderation as 
"possible v2" ideas. The project is long-running and its learning goals 
include real-time communication, payment flows and richer authorization, 
so the scope had to be clarified: optional wish list, or part of the 
plan?

## Decision drivers

- Learning value of the v2 topics (real-time, state machines, 
  concurrency, token refresh)
- Avoid a half-finished product: v1 must be complete and presentable 
  first
- Keep v1 design compatible with v2

## Considered options

### Option A — v2 as an optional wish list
- ✅ No pressure; v1 can be "the project"
- ❌ v1 design choices may block v2 later; topics with high learning 
  value risk never being done

### Option B — v2 committed, started only after v1 is complete, one feature per branch/PR
- ✅ v1 decisions are taken with v2 in mind (e.g. ADR-006)
- ✅ Clear order: no feature is built partially in parallel
- ❌ A larger total scope for a solo project; risk of a long timeline

## Decision

**Option B.** v2 contains: real-time in-app messaging (SignalR), 
simulated payments in test mode (a purchase sets the listing to `sold`, 
likely via a `reserved` state), an admin moderation dashboard, dynamic 
admin-managed categories, and token refresh. Real payments, real 
shipping and multi-language stay out of scope.

## Consequences

- **Positive**: v1 avoids dead ends; the roadmap has a long-term 
  direction.
- **Negative**: scope risk. Mitigation: v1 is finished and deployed 
  before any v2 work, and each v2 feature is a separate, complete PR.
- **Follow-ups**: v2 requirements are added to 02-Requirements.md when 
  each feature starts. The order of v1 phases is defined in 
  09-Roadmap.md.

## Links

- [`../01-Vision.md`](../01-Vision.md), [`../09-Roadmap.md`](../09-Roadmap.md)
- [ADR-006](ADR-006-Dedicated-Listing-Status-Endpoint.md)
