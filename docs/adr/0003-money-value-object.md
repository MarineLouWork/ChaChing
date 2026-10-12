# 0003 — `Money` is an immutable, signed value object, rounded at construction

**Status:** Accepted

## Context

Two amounts of €10 are interchangeable: `Money` has no identity. Three questions followed: may it be negative, can it be altered after construction, and where does rounding happen.

- A negative amount looks "unbusinesslike", but forbidding it makes `paid − due` fail on a perfectly normal case, an overpayment.
- A validation placed in a property initializer is **not replayed by `with`**: `money with { Currency = (Currency)888 }` produced an invalid `Money`.
- `Multiply` can produce more than two decimals (`€10 × 1.7215`). Rounding inside each operation left construction unconstrained: `new Money(10.005m, EUR)` kept three decimals.

## Decision

- An immutable `record` with value equality, **signed**: negative amounts are allowed, like an `int`. The rule "an invoice line is positive" belongs to the document, not to `Money`.
- `Amount` and `Currency` are **get-only**: `with` on them does not compile, so an invalid `Money` cannot be written. Changing a currency would be a conversion (it needs a rate), not a copy.
- **Rounding is an invariant, applied at construction** to 2 decimals, with `MidpointRounding.ToEven`.
- `Money` stays generic: no VAT, no business rule. The line *uses* `Money.Multiply`.

## Consequences

- A `Money` always has at most two decimals and a defined currency, whatever the path that built it.
- `ToEven` does not bias totals over a large volume, unlike `AwayFromZero`.
- **Price paid:**
  - rounding at every step is not neutral: `€0.333 × 3` gives €0.99, not €1.00. The usual accounting rule is a single rounding at the end. The totals mitigate it by computing VAT once per rate (ADR 0020), but the question stays open with an accountant: French practice often rounds half up;
  - two decimals are assumed for every currency (wrong for the yen, the Tunisian dinar), acceptable while `Currency` is limited to three values;
  - the `this with { … }` shortcut is lost in `Add`, `Subtract` and `Multiply`.
- Alternatives set aside:
  - **forbidding negative amounts**: breaks the overpayment case;
  - **rounding inside each operation only**: two behaviours for the same type depending on the path;
  - **a single rounding at the end of the computation**: accounting-correct, but it would leave `Money` with unbounded decimals in between.
