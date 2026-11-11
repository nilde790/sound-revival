# ADR-010: Input validation approach

- **Status**: Accepted (resolves decision D2)
- **Date**: 2026-11-11

## Context

No request DTO is validated today (debt C4): an empty email or a 
negative price reaches the services. The field constraints are already 
defined in [`../07-API-Design.md`](../07-API-Design.md) → Field 
constraints. What remains to choose is the **mechanism**.

Two kinds of validation exist, and they belong to different places 
whatever mechanism is chosen:

| Kind | Examples | Where |
|---|---|---|
| **Shape / format** — can be checked looking only at the request | required, length, range, email format, `minPrice ≤ maxPrice`, `pageSize ≤ 100` | at the edge, before the service runs |
| **Business rules** — need domain knowledge or data | allowed category list, owner check, status transitions, max 5 images, email already registered | in the service (rule ARCH-2) |

This ADR decides the mechanism for the first kind.

## Decision drivers

- Consistency with the error format (see [ADR-009](ADR-009-Error-Handling.md))
- Little boilerplate, few dependencies
- Ability to express cross-field rules (`minPrice ≤ maxPrice`)
- Testability
- Market relevance

## Considered options

### Option A — DataAnnotations (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`)
Attributes on DTO properties. With `[ApiController]`, ASP.NET Core 
validates them **automatically** before the action runs and returns 
`400` with a `ValidationProblemDetails` body listing the errors per 
field.
- ✅ Built in, no extra package, almost zero code
- ✅ Already produces the standard error format
- ✅ The constraints are visible right on the DTO
- ❌ Cross-field rules need `IValidatableObject` or custom attributes, 
  which get clumsy as rules grow
- ❌ The `Dto` project needs `System.ComponentModel.DataAnnotations` 
  (part of .NET, so still no external dependency)

### Option B — FluentValidation
One validator class per DTO (`RuleFor(x => x.Price).GreaterThan(0)`).
- ✅ Very expressive; cross-field and conditional rules read well
- ✅ Validators are easy to unit test; widely used in .NET teams
- ❌ External dependency and one extra class per DTO
- ❌ Its automatic MVC integration is no longer recommended by its 
  maintainers: validators are invoked manually or through a filter, 
  which means extra wiring

### Option C — Manual checks in services
`if (string.IsNullOrWhiteSpace(request.Title)) throw ...`
- ✅ No framework to learn
- ❌ Verbose and repeated; one error at a time instead of all fields; 
  mixes shape checks with business rules

## Decision

**Option A (DataAnnotations)** for shape validation, plus **business 
rules in the services** as today. Use `IValidatableObject` for the few 
cross-field rules (e.g. `minPrice ≤ maxPrice` on the filter DTO).

Why: it is built in, needs almost no code, and produces the same 
`ProblemDetails` format decided in ADR-009, so clients get one 
consistent error shape. FluentValidation is the natural upgrade if 
cross-field or conditional rules multiply. That switch would be a new 
ADR, and it would not change the API contract.

## Consequences

- **Positive**: closes debt C4 quickly; constraints live next to the 
  fields they describe.
- **Negative**: business-rule errors (thrown by services) and shape 
  errors (from attributes) come from two places; ADR-009 makes both use 
  `ProblemDetails`.
- **Follow-ups**: update the field-constraints note in 07-API-Design.md 
  to "implemented"; add unit tests for `IValidatableObject` rules and 
  integration tests for the `400` bodies.

## Links

- Rules VAL-1..VAL-7 in [`../dev/ENGINEERING-STANDARDS.md`](../dev/ENGINEERING-STANDARDS.md)
- [Model validation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [FluentValidation documentation](https://docs.fluentvalidation.net/)
