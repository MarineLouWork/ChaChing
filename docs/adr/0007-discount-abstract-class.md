# 0007 — `Discount` is an abstract class, and its contract announces the currency exception

**Status:** Accepted — 25/09/2026 (proposed on 22/09 with an `IDiscount` interface; rewritten before acceptance)

## Context

Several kinds of discounts coexist (percentage, fixed amount) and more will come. A method full of `if`s on a `string` would force us to modify existing code for every new kind.

At first no logic was shared, and an interface `IDiscount` looked enough. Then every result had to be **floored at zero**, which introduced shared code, and an abstract class came in. The interface stayed as a leftover: it had no caller besides the abstract class itself.

A second question showed up with polymorphism. A caller holds a `Discount` and does not know which kind it is. But `FixedAmountDiscount` holds its own `Money`: applied to a total in another currency, it throws. `PercentageDiscount` never does. The caller could not know that some discounts may throw.

## Decision

- **An abstract class `Discount`, no interface.** `Apply` orchestrates (template method): `Calculate`, `protected abstract`, is filled in by each kind, then `FloorAtZero`, `private`, is always applied. `protected` is what prevents a subclass from bypassing the floor.
- **The currency exception is part of `Discount`'s contract**, documented on `Apply` (`<exception>`): the total must be in the discount's currency. No conversion, no mixing of currencies. That is a business rule, which `Quote` will enforce upstream. The exception is the safety net.

## Consequences

- Adding a discount = adding a class, without touching the calling code: that is the **OCP**, the O in SOLID.
- **Liskov is respected** because the parent announces the requirement. `FixedAmountDiscount` throws as announced; `PercentageDiscount` never throws, which is allowed: **a subclass may be less demanding than its parent, never more.** Without the documentation, the same code would break Liskov: the exception would be a surprise.
- Polymorphism is tested: an array of `Discount` applied in a loop, with no `if` and no `is` on the concrete type.
- Single inheritance is used up by `Discount`: a kind of discount can no longer inherit from anything else.
- **Price paid:** the contract lives in a comment, which the compiler does not check. A caller can still ignore it, so it has to be tested (`FixedDiscount_DifferentCurrency_Throws`).
- Alternatives set aside:
  - **`IDiscount` + abstract class**: an interface with no caller, removed for YAGNI;
  - **a `FlooredDiscount` decorator** wrapping an `IDiscount` (composition rather than inheritance): more flexible, but more pieces for a single shared rule. To reconsider if discounts ever need to be combined in other ways;
  - **checking the currency when the discount is created**: impossible, because a discount only learns the total's currency when `Apply` is called.
