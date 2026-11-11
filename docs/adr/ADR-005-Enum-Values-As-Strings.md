# ADR-005: Enum-like values stored as validated strings

- **Status**: Accepted
- **Date**: 2026-07-12 (recorded retroactively on 2026-11-11)

## Context

Listings have a category, a condition and a status; users have a role. 
Each is one value from a small, fixed list (see 
[`../04-DomainModel.md`](../04-DomainModel.md)). We need to decide how to 
model and store them.

## Decision drivers

- The lists are small and change rarely (v1)
- Readable data in the database and in API payloads
- Simplicity: no joins just to show a category name
- Keep the door open for admin-managed categories (v2)

## Considered options

### Option A — Lookup tables (`Categories`, `Conditions`, …) with foreign keys
- ✅ The database guarantees valid values; lists can change without 
  deploying code
- ❌ Joins and extra entities for lists that don't change in v1

### Option B — C# `enum` mapped by EF Core (as int or as string)
- ✅ Type safety in code; typos become compile errors
- ❌ Display names with spaces ("Like New", "Wind Instruments") need 
  extra mapping; stored as int, the data is unreadable in SQL

### Option C — Plain `string` columns, validated in the service against the documented lists
- ✅ Readable in the DB and in JSON; display names stored as-is
- ✅ Simplest to start with
- ❌ The database accepts any text: validity depends on the service

## Decision

**Option C.** Values are strings: `Category` and `Condition` use their 
display names (`Guitars`, `Like New`), `Status` and `Role` are lowercase 
(`available`, `sold`, `user`, `admin`). The service validates them 
against the lists in 04-DomainModel.md.

## Consequences

- **Positive**: no joins, readable data, trivial serialization.
- **Negative**: validation must not be forgotten — including on query 
  filters (rule VAL-4). The same literals repeat in several places, so a 
  typo is a silent bug unless they are centralized (rule STYLE-4, debt 
  B5).
- **Follow-ups**: dynamic, admin-managed categories (v2) will replace 
  the category list with a table; that change needs a data migration and 
  a new ADR superseding this one for `Category`. A PostgreSQL `CHECK` 
  constraint could add a database-level guard if needed.

## Links

- [`../05-Database.md`](../05-Database.md) → Design notes
- Rules VAL-4, STYLE-4 in [`../dev/ENGINEERING-STANDARDS.md`](../dev/ENGINEERING-STANDARDS.md)
