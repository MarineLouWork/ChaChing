# 0013 — Gapless numbering, assigned when sent

**Status:** Accepted — 28/09/2026

## Context

A quote needs a readable public reference; its `Guid` is a technical key and is never shown. French law requires invoices to be numbered in a **continuous chronological sequence**, unique within a year (art. 242 nonies A, annex II of the CGI). Distinct series (a prefix per series) are allowed if each one stays continuous. Quotes have no such obligation, but the product wants the same guarantee: no gap.

A gap appears whenever a number is taken and then lost: a deleted draft, a failed transaction.

## Decision

- **`QuoteNumber` is a value object** (format `DEV-2026-0042`), validated in the domain. Invoices will get their own series (`FAC-…`).
- **The number is assigned by `Send()`**, never at creation: drafts, which can be deleted, never consume a number.
- **The domain does not generate it.** `quote.Send(number, …)` receives it; the use case gets it through a port.
- **The counter is a table** (seller, document type, year → last number), incremented **in the same transaction** as saving the sent quote. If the transaction fails, the number is not consumed.
- One sequence **per seller, per type, per year**.

## Consequences

- No gap and no duplicate, per seller, even under concurrent sends (the counter row is locked for the transaction).
- A draft has no reference; the UI shows "Draft" instead.
- **Price paid:** sends by the same seller are serialized on the counter row (negligible for a freelancer), and the number cannot be known before sending.
- Alternatives set aside:
  - **a SQL Server `SEQUENCE` or `IDENTITY`**: fast, but values are lost on rollback, leaving gaps;
  - **`MAX(number) + 1`**: two concurrent sends can read the same maximum and get the same number;
  - **a number at creation**: every deleted draft would leave a gap.
