# ADR-006: Dedicated endpoint for listing status changes

- **Status**: Accepted
- **Date**: 2026-11-11

## Context

A listing's status (`available` → `sold`) must be changeable by its 
owner or an admin (FR-9). It could be one more field in the general 
update (`PUT /api/listings/{id}`) or have its own endpoint. In v2, a 
completed simulated payment will also change the status automatically, 
and an intermediate `reserved` state is likely.

## Decision drivers

- Status is a domain *action* with rules (allowed transitions), not a 
  free field
- v2 must be able to change status without going through a "full edit"
- Prevent accidental status changes when the user edits the description

## Considered options

### Option A — `status` as a field of `PUT /api/listings/{id}`
- ✅ One endpoint less
- ❌ Every edit can change the status; transition rules get mixed with 
  field validation; v2 automation would reuse a user-facing edit

### Option B — `PATCH /api/listings/{id}/status` with `{ "status": "..." }`
- ✅ One place for status rules and future transitions (state machine)
- ✅ `PUT` stays a pure content edit
- ❌ One more endpoint, DTO and service method

## Decision

**Option B.** `UpdateListingRequestDto` has no `Status`; the status DTO 
carries only `status`. Allowed values and transitions are validated in 
the service.

## Consequences

- **Positive**: v1 design doesn't block v2 payments; status logic is 
  isolated and testable.
- **Negative**: slightly more code for v1.
- **Follow-ups**: with v2 payments, concurrent status changes must be 
  detected (optimistic concurrency, rule CONC-2).

## Links

- FR-9, FR-10 in [`../02-Requirements.md`](../02-Requirements.md); US-9
- [`../07-API-Design.md`](../07-API-Design.md) → PATCH /status
