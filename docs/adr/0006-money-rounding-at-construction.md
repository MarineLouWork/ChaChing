# 0006 — Rounding to 2 decimals, at construction

**Status:** Accepted — 22/09/2026

## Context

`Multiply` can produce more than two decimals: `€10 × 1.7215`. Rounding inside each operation left construction unconstrained: `new Money(10.005m, EUR)` kept three decimals. Same type, two behaviours depending on the path taken.

## Decision

Rounding is an **invariant**: it applies at construction, in the same place as the currency validation. `MidpointRounding.ToEven` (banker's rounding).

## Consequences

- A `Money` always has at most two decimals, whatever the path.
- `ToEven` does not bias totals over a large volume, unlike `AwayFromZero`, which always rounds up at the midpoint.
- ⚠️ **Rounding at every step is not neutral.** `€0.333 × 3` gives €0.99 instead of €1.00. The usual accounting rule is a single rounding, at the end. Decision accepted here to keep the invariant simple: **to be reviewed when invoice totals come in**.
- Assumes 2 decimals for every currency. Wrong for the yen (0) and the Tunisian dinar (3). Acceptable as long as the enum is limited to EUR, USD and GBP.
