# 0003 — `Money` is a signed value object

**Status:** Accepted — 22/09/2026

## Context

Two amounts of €10 are interchangeable: `Money` has no identity of its own.

The sign remained to be decided. Forbidding negative amounts looked "more business-like" (the meaning of a credit note belongs to the invoice), but it makes `paid − due` fail on a perfectly normal case: an overpayment.

## Decision

An immutable `record` with value equality. **Negative amounts are allowed**: `Money` is a signed quantity, like an `int`.

## Consequences

- `Subtract` never throws because of the sign, and a negative result is useful information.
- The rule "an invoice line is positive" belongs to `Invoice`, not to `Money`.
- Immutability: every operation returns a new object. The `+`, `-` and `*` operators bring back the writing comfort.
- `Money` stays **generic**: no rule from any specific business. VAT belongs to the invoice line, which *uses* `Money.Multiply`.
