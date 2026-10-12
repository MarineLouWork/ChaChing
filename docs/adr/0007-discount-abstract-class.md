# 0007 — `Discount` is an abstract class (template method); its contract announces the currency exception

**Status:** Accepted

## Context

Several kinds of discounts coexist (percentage, fixed amount) and more will come. A method full of `if`s on a `string` would force us to edit existing code for every new kind.

Every result has to be **floored at zero**, which is shared code, so an interface alone is not enough. And polymorphism raises a question: a caller holds a `Discount` and does not know which kind. `FixedAmountDiscount` holds its own `Money` and throws when applied to a total in another currency; `PercentageDiscount` never does.

## Decision

- **An abstract class `Discount`, no interface.** `Apply` is the template method: it calls `Calculate` (`protected abstract`, filled in by each kind), then always applies `FloorAtZero` (`private`). `protected` prevents a subclass from bypassing the floor.
- **The currency exception is part of the contract**, documented on `Apply`: the total must be in the discount's currency. No conversion, no mixing. The business rule is enforced upstream by the document; the exception is the safety net.
- **Business rules, identical for both kinds:** a zero discount is accepted (it does nothing, and the two kinds must not contradict each other); a rate goes from 0 % to 100 %, both included (an item can be offered); a fixed amount is not negative.
- **Discounts exist at two levels**, on a line and on the document total. Line discounts apply first, then the total's.

## Consequences

- Adding a discount is adding a class, without touching the calling code (open/closed).
- Liskov holds because the parent announces the requirement: a subclass may be less demanding than its parent, never more. Polymorphism is tested with an array of `Discount` and no `is`.
- The bounds are locked by tests on each side of each boundary (`-0.01` rejected / `0` accepted, `100` accepted / `100.01` rejected).
- **Price paid:**
  - the contract lives in a comment the compiler does not check, so it has to be tested (`FixedDiscount_DifferentCurrency_Throws`);
  - single inheritance is used up by `Discount`;
  - a zero discount is often a typing mistake and now passes silently: if that matters, the check belongs to the user interface;
  - the **order** of discounts changes the result (10 % then €3 on €10 gives €6, €3 then 10 % gives €6.30); the rule is fixed above and tested.
- Alternatives set aside:
  - **`IDiscount` + abstract class**: an interface with no caller, removed (YAGNI);
  - **a `FlooredDiscount` decorator**: more flexible, more pieces for a single shared rule; to reconsider if discounts must be combined in other ways;
  - **checking the currency at creation**: impossible, the discount only learns the total's currency when applied.
