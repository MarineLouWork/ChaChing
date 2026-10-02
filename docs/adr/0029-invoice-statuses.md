# 0029 — Four stored invoice statuses; overdue is derived

**Status:** Accepted — 02/10/2026.

## Context

`InvoiceStatus` was written with seven values up front (`Draft`, `Sent`, `Declined`, `Paid`, `Closed`, `Late`, `Conflicted`), but only `Draft` was used. Before writing the lifecycle, each value had to earn its place. Criterion: **a stored status is a state that changes what one is allowed to do; anything that can be deduced from a date or from another object is not stored.**

## Decision

- **`Draft → Issued → Paid`, plus `Cancelled`.**
- The enum **starts at 1**, like `TypeDocument` (ADR 0028): a forgotten value is never a valid status.
- **Today's date is a parameter**: `IsOverdue(DateOnly today)`. The domain does not read the clock; a test writes the date it wants. An injected `TimeProvider` is for the application layer, if the real time is ever needed. Overdue is strict: `today > dueDate`.
- **`DueDate` is stored** (set at issue; payment terms are negotiated and printed on the invoice), as is **`PaymentDate`** (`null` until paid): once `Paid`, "was it late?" can no longer be read from the status.
- **`Sent` becomes `Issued`**: an invoice is issued, and therefore untouchable, whether or not it has been sent.
- **`Late` is removed**: it is derived (`Issued`, due date before today). Stored, it would be wrong at midnight. It is a query, like `Totals`, computed on read and never stored.
- **`Declined` is removed**: a quote notion. A client disputing an invoice leads to a credit note.
- **`Conflicted` is removed**: a dispute is not an accounting state.
- **`Closed` is removed**: it duplicated `Paid`.
- **`Cancelled`** is set when a credit note cancels an **unpaid** invoice (`Issued → Cancelled` only). The credit note itself comes later.
- **A `Paid` invoice never becomes `Cancelled`** (decided 02/10/2026): `Paid` records that it was settled; a later credit note (refund) is its own document, linked to the invoice, and leaves the invoice's status alone. A cancelled invoice keeps all the mentions it was issued with, due date included.
- Once issued, an invoice is never modified: an action that is no longer allowed throws `InvalidOperationException` (the state forbids it, not the argument).

## Consequences

- Fewer states, fewer transitions to test and to keep consistent.
- **Price paid:**
  - "late" is no longer a field: every caller that needs it must supply today's date itself, so the clock is pushed to the layer above;
  - no partial payment: an invoice is `Paid` or it is not. Deliberately left out (YAGNI), to revisit if a real case appears;
  - `Cancelled` exists before anything can set it: it stays unreachable until `CreditNote`.
- Alternatives set aside:
  - **keeping the seven values**: no cost today, but each unused value is a promise nobody has tested;
  - **storing `Late`**: simple to read, but a background job would have to flip it every night, and it would be wrong in between.
