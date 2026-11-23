# ADR-007: No generic `Update` method in repositories

- **Status**: Accepted
- **Date**: 2026-11-11

## Context

While designing `IListingRepository`, the question was whether to add an 
`UpdateListing(Listing listing)` method next to `AddListing` and 
`DeleteListing`.

## Decision drivers

- Avoid methods that do nothing useful
- Use EF Core the way it is designed to work
- Keep repositories small

## Considered options

### Option A — `UpdateListing(Listing)` calling `_context.Listings.Update(listing)`
- ✅ Symmetric with Add/Delete; familiar from tutorials
- ❌ For an entity loaded in the same request it is redundant: EF Core 
  already tracks it. `Update()` also marks **every** property as 
  modified, producing an `UPDATE` of all columns instead of only the 
  changed ones

### Option B — Rely on change tracking
The service loads the entity with `GetListingByIdAsync` (tracked), 
changes its properties, and calls `SaveChangesAsync`. EF Core compares 
the entity with its snapshot and updates only the changed columns.
- ✅ No extra repository method; minimal `UPDATE` statements
- ❌ Requires understanding change tracking; `GetListingByIdAsync` must 
  stay tracked (no `AsNoTracking`)

## Decision

**Option B.** Read-only queries use `AsNoTracking()`; queries that load 
an entity to modify it stay tracked (rule DATA-1).

## Consequences

- **Positive**: smaller repository, efficient updates, a concrete lesson 
  on how EF Core's Unit of Work works.
- **Negative**: the "tracked vs no-tracking" difference must be kept in 
  mind; making `GetListingByIdAsync` no-tracking would silently break 
  updates (nothing would be saved).
- **Follow-ups**: for bulk changes without loading entities, EF Core's 
  `ExecuteUpdateAsync` is the tool, if ever needed.

## Links

- Rules DATA-1, ARCH-4 in [`../dev/ENGINEERING-STANDARDS.md`](../dev/ENGINEERING-STANDARDS.md)
- [Change tracking in EF Core](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
