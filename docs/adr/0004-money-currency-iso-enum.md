# 0004 — `Currency` is an `enum` of ISO 4217 codes

**Status:** Accepted — 22/09/2026

## Context

A currency stored as a `string` requires validating case and typos: `"eur"` and `"EUR"` would make two identical amounts different for the record's value equality.

.NET does not expose an ISO 4217 enum. `RegionInfo.ISOCurrencySymbol` could build the list, at the cost of depending on the system's culture data.

## Decision

`enum Currency { EUR = 978, USD = 840, GBP = 826 }`: the **ISO 4217 numeric codes**, none of which is 0.

## Consequences

- The case problem disappears at compile time: **a well-chosen type removes validation instead of requiring it.**
- Since no currency equals 0, an enum's default value is detectable as invalid, for free, just through the choice of numbers.
- An `enum` is still an `int` at runtime: `(Currency)888` compiles. Validation through `Enum.IsDefined` is therefore still required. **An enum is not a closed type.**
- Adding a currency requires recompiling and redeploying. Accepted at this stage; a `string` validated against a list would be more flexible and less safe.
