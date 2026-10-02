# 0030 — Rules shared by quote and invoice at issue; payment date rule

**Status:** Accepted — 02/10/2026. Completes [ADR 0028](0028-billing-number-with-document-type.md) (the number's type is now checked) and [ADR 0029](0029-invoice-statuses.md).

## Context

ADR 0028 left a hole: the compiler does not stop an invoice from receiving a quote's number. The exempt-seller rule (no VAT line from an exempt seller) existed only in `Quote.MarkAsSent`, so an invoice could leave with VAT its seller may not bill. Both rules belong to the two documents.

## Decision

- **`BillingNumber.ValidateType(TypeDocument)`** is called by `Quote.MarkAsSent` (`QUO`) and `Invoice.MarkedAsIssued` (`INV`). A wrong type throws **`InvalidOperationException`**, chosen by Marine: the document cannot take a number of another series. The check runs before any field is set, so the document stays a draft.
- **The VAT regime rule lives in `BillingLine.IsAllowedFor(VatRegime)`**: the knowledge of which rate goes with which regime sits in one place, the line that carries the rate. Each aggregate keeps its own decision and throws its own `InvalidOperationException`. A subject seller may still bill an exempt line.
- **`Invoice.MarkAsPaid(paymentDate)`** refuses a date before the issue date (`ArgumentException`, `paymentDate`): the argument is inconsistent with the document. A payment on the issue date is valid.
- The method stays named `MarkedAsIssued`, like the `MarkAs…` methods: the domain interacts with nothing, it only records a fact.

## Consequences

- One definition of the rule, two callers; a new document type (credit note) calls the same two methods.
- **Price paid:**
  - the "issue" sequence of checks is now written twice, once per aggregate, and must be kept in the same order by hand;
  - `ValidateType` throws `InvalidOperationException` although the number is an argument: a caller who passes the wrong number gets a state-type exception. Accepted for consistency with the other lifecycle errors;
  - a payment date in the future is accepted: the domain does not read the clock (ADR 0029).
- Alternatives set aside:
  - **a shared `BillingRules` class**: heavier for one rule and two callers; to reconsider with a third shared rule;
  - **`ArgumentException` for the number's type**: closer to the argument, but the number is valid in itself, it is the document that cannot take it.
