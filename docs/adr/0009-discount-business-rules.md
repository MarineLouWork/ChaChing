# 0009 — Discount business rules: zero is accepted, rates between 0 and 100 %

**Status:** Accepted — 25/09/2026

## Context

The two kinds of discount contradicted each other: `FixedAmountDiscount` accepted an amount of 0, `PercentageDiscount` rejected a rate of 0. Two sibling classes, two different rules for the same situation.

The upper bound of the rate also had to be decided: is 100 % a valid discount, that is, may an item be given away?

## Decision

- **A zero discount is accepted**, for both kinds. It does nothing and is harmless.
- **A rate is valid from 0 to 100 %, both included.** 100 % is allowed: an item can be offered.
- A fixed amount must not be negative.

## Consequences

- The two kinds follow the same rule: no surprise when switching from one to the other.
- The rules are locked by **boundary tests**, one case on each side of each boundary: `-0.01` rejected / `0` accepted, `100` accepted / `100.01` rejected. A case far from the boundary (`110`) proves nothing: `> 100` and `>= 100` both reject it.
- **Price paid:** a zero discount is often a typing mistake, and it now goes through silently. If that becomes a problem, the check belongs to the user interface, not to the domain.
- **Still open:** the **order** in which discounts are applied changes the result (10 % then €3 on €10 gives €6; €3 then 10 % gives €6.30). That is a business rule for `Quote`, to be decided when it is built.
