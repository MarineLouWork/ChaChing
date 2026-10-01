# 0027 — `Guard` for technical preconditions, business rules stay in the aggregates

**Status:** Accepted — 01/10/2026

## Context

With `Invoice`, the same checks were written again: "this `Guid` is not empty" (in `Quote`, `Invoice`, `Client`) and "this enum value exists" (in `Quote`, `Invoice`, `Money`, `Seller`). Marine did not want to wait for more copies before factoring them.

## Decision

- **A static class `Guard` in `Common/`** holds the checks that are true everywhere and know nothing about the business: `ValidateGuid`, `ValidateNullableGuid`, `ValidateEmptyString`, `ValidateEnum<TEnum>`.
- Each method **returns the validated value**, so that it fits in an assignment: `SellerId = Guard.ValidateGuid(sellerId, nameof(sellerId));`.
- `ValidateEnum<TEnum>` is **generic** (`where TEnum : struct, Enum`): one method for `Currency`, `VatRegime`, `TypeDocument` and any future enum. `Guard` knows none of them, so `Common/` depends on no other folder.
- They all throw **`ArgumentException(message, paramName)`**.
- **Business rules stay in the aggregate that owns them**: `EnsureStatus`, `EnsureSameCurrency`, the SIREN or e-mail format. The test: does the rule need the object's state or business vocabulary? Then it is not a guard.

## Consequences

- One place and one message for each technical check; a new aggregate gets them for free.
- **Price paid:** `Guard` is tooling, not a domain concept; the messages are generic, built from the parameter name ("The name is required." became "The name is empty."). An undefined enum value throws `ArgumentException`, not `ArgumentOutOfRangeException`.
- Alternatives set aside:
  - **typed identifiers** (`SellerId`, `ClientId` as value objects that validate themselves): the purest DDD answer, an empty identifier could no longer exist; but several new types and every signature to change, and it would supersede ADR 0011;
  - **a base class** carrying the checks: see ADR 0026;
  - **one non-generic method per enum**: tried first; four copies of the same test, and `Common/` had to import `Parties/`.
