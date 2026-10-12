# 0027 — `ArgumentValidator` for technical preconditions, business rules stay in the aggregates

**Status:** Accepted

## Context

The same checks kept being written in every aggregate: "this `Guid` is not empty", "this enum value exists", "this text is not blank". They are true everywhere and know nothing about the business.

## Decision

- **A static class `ArgumentValidator` in `Common/`** holds the checks that are true everywhere: `Guid`, `NullableGuid`, `Enum<TEnum>`, `NonBlankText`. The class name says what it does, so the methods carry no prefix and call sites read as a sentence: `SellerId = ArgumentValidator.Guid(sellerId, nameof(sellerId));`.
- Each method **returns the validated value** (`NonBlankText` returns it trimmed), so it fits in an assignment.
- `Enum<TEnum>` is **generic** (`where TEnum : struct, Enum`): `Common/` knows none of the enums, so it depends on no other folder.
- They all throw **`ArgumentException(message, paramName)`**.
- **Business rules stay in the aggregate that owns them** (`EnsureStatus`, `EnsureSameCurrency`, the SIREN or email format). The test: does the rule need the object's state or business vocabulary? Then it is not a technical check.

## Consequences

- One place and one message for each technical check; a new aggregate gets them for free.
- **Price paid:** the class is tooling, not a domain concept; the messages are generic; an undefined enum value throws `ArgumentException`, not `ArgumentOutOfRangeException`; methods named `Guid` and `Enum` hide the `System` types inside the class, which writes `System.Guid.Empty`.
- Alternatives set aside:
  - **typed identifiers** (`SellerId`, `ClientId` as value objects that validate themselves): the purest DDD answer, since an empty identifier could no longer exist, but several new types and every signature to change;
  - **a base class** carrying the checks: it welds the aggregates together (ADR 0024);
  - **one non-generic method per enum**: four copies of the same test, and `Common/` had to import `Parties/`;
  - **the name `Guard`** (guard clauses): well known, but it hides the trimming and the returned value.
