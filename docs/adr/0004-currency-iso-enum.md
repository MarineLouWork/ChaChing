# 0004 — `Currency` is an `enum` of ISO 4217 numeric codes

**Status:** Accepted

## Context

A currency stored as a `string` needs case and typo validation: `"eur"` and `"EUR"` would make two identical amounts different under the value equality of `Money`. .NET exposes no ISO 4217 enum; `RegionInfo.ISOCurrencySymbol` could build a list, at the cost of depending on the system's culture data.

## Decision

`enum Currency { EUR = 978, USD = 840, GBP = 826 }`: the ISO 4217 **numeric** codes, none of which is 0.

## Consequences

- The case problem disappears at compile time: **a well-chosen type removes validation instead of requiring it.**
- No currency equals 0, so the default value of the enum is detectable as invalid, for free.
- **Price paid:**
  - an enum is still an `int` at runtime: `(Currency)888` compiles, so `ArgumentValidator.Enum` (ADR 0027) is still required. An enum is not a closed type;
  - adding a currency means recompiling and redeploying.
- Alternative set aside: a **`string` validated against a list**, more flexible and less safe.
- The same trick (enums that start at 1) is used for `VatRegime`, `TypeDocument` and `InvoiceStatus`.
