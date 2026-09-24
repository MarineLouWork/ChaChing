# 0007 — `IDiscount` as the contract, abstract class for the shared code

**Status:** Proposed — 22/09/2026

## Context

Several kinds of discounts coexist (percentage, fixed amount) and more will come. A method full of `if`s on a `string` would force us to modify existing code for every new kind.

At first, no logic was shared: an interface was enough. The need to **floor** every result at zero introduced shared code, which reopens the question.

## Decision

`IDiscount` remains the contract seen by callers. An abstract class `Discount : IDiscount` holds the shared code: `Apply` orchestrates (template method) and `Calculate`, `protected abstract`, is filled in by each kind.

## Consequences

- Adding a discount = adding a class, without touching the calling code: that is the **OCP**, the O in SOLID.
- Single inheritance is used up by `Discount`: a kind of discount can no longer inherit from anything else.
- `PercentageDiscount` can never fail on the currency, `FixedAmountDiscount` can. Two implementations of the same contract with different requirements: **a Liskov question, to be settled in S3.**
- Alternative set aside for now: a `FlooredDiscount` decorator wrapping an `IDiscount`, i.e. composition rather than inheritance. To be compared in S3, when we study patterns.
