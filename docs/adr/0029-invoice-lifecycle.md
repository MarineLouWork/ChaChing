# 0029 — Invoice lifecycle: four stored statuses, overdue is derived

**Status:** Accepted

## Context

`InvoiceStatus` first had seven values written up front (`Draft`, `Sent`, `Declined`, `Paid`, `Closed`, `Late`, `Conflicted`), of which only `Draft` was used. Each value had to earn its place. Criterion: **a stored status is a state that changes what one is allowed to do; anything that can be deduced from a date or from another object is not stored.**

## Decision

- **`Draft → Issued → Paid`, plus `Cancelled`.** The enum starts at 1 (ADR 0004). `Issued`, not `Sent`: an invoice is issued, and therefore untouchable, whether or not it has been sent.
- **Overdue is derived**, never stored: `IsOverdue(DateOnly today)` is true for an `Issued` invoice whose due date is strictly before today. Stored, it would be wrong at midnight. **The domain does not read the clock:** today's date is a parameter, a test writes the date it wants, and an injected clock belongs to the application layer.
- **`DueDate` and `PaymentDate` are stored:** the due date is set at issue (payment terms are negotiated and printed on the invoice); once `Paid`, "was it late?" can no longer be read from the status.
- **Removed:** `Declined` (a quote notion; a dispute leads to a credit note), `Conflicted` (a dispute is not an accounting state), `Closed` (duplicated `Paid`), `Late` (derived).
- **`Cancelled`** is reached from `Issued` only (a credit note cancels an unpaid invoice). A `Paid` invoice never becomes `Cancelled`: `Paid` records that it was settled, and a later refund is its own document, linked to the invoice. A draft cannot be cancelled either.
- **An issued invoice is never modified.** An action that is not allowed throws `InvalidOperationException` (the state forbids it, not the argument).
- **Rules at issue:** the number must be of type `INV` (ADR 0013); `dueDate` not before `issueDate`; at least one line; snapshots matching the invoice's seller and client (ADR 0019); no VAT line from an exempt seller (ADR 0014). All checks run before any field is set, so a refused invoice stays a draft.
- **Payment:** `MarkAsPaid(paymentDate)` refuses a date before the issue date (`ArgumentException`: the argument is inconsistent with the document). A payment on the issue date is valid.

## Consequences

- Fewer states, fewer transitions to test and keep consistent.
- **Price paid:**
  - "late" is no longer a field: every caller needing it supplies today's date, so the clock is pushed to the layer above;
  - a payment date in the future is accepted, since the domain does not read the clock: the use case controls it;
  - no partial payment: an invoice is `Paid` or it is not. Deliberately left out (YAGNI), to revisit if a real case appears;
  - `Cancelled` exists before anything can set it; it stays unreachable until the credit note;
  - the issue checks are written once per aggregate (quote and invoice) and kept in the same order by hand.
- Alternatives set aside:
  - **the seven values**: each unused value is a promise nobody has tested;
  - **storing `Late`**: a nightly job would have to flip it, and it would be wrong in between;
  - **a shared `BillingRules` class** for the issue checks: heavier for two callers; to reconsider with a third shared rule.
