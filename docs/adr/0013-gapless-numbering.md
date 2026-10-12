# 0013 — Gapless numbering, assigned when the document is sent, one `BillingNumber` for every document

**Status:** Accepted

## Context

A document needs a readable public reference; its `Guid` is a technical key and is never shown. French law requires invoices to be numbered in a **continuous chronological sequence**, unique within a year (art. 242 nonies A, annex II of the CGI); distinct series are allowed if each stays continuous. Quotes have no such obligation, but the product gives them the same guarantee.

A gap appears whenever a number is taken and then lost: a deleted draft, a failed transaction.

## Decision

- **`BillingNumber(TypeDocument, Year, Sequence)`** is a value object shared by quotes and invoices, displayed `QUO-2026-0042` or `INV-2026-0042`. `TypeDocument` (`QUO`, `INV`, `CRE` for the future credit note) starts at 1 and takes part in equality: two numbers of different series are different numbers.
- **The number is assigned when the document is sent or issued**, never at creation, so drafts, which can be deleted, never consume a number.
- **The domain does not generate it:** `MarkAsSent(number, …)` receives it, and the use case gets it through a port (`IBillingNumberGenerator`).
- **The counter is a table** (seller, document type, year → last number), incremented **in the same transaction** as saving the document. If the transaction fails, the number is not consumed. One sequence per seller, per type, per year.
- **The document checks the type of the number it receives** (`BillingNumber.ValidateType`): a quote refuses an `INV` number, an invoice refuses a `QUO` one, with an `InvalidOperationException` and before any field is set.

## Consequences

- No gap and no duplicate per seller, even under concurrent sends (the counter row is locked for the transaction).
- A draft has no reference; the UI shows "Draft".
- **Price paid:**
  - sends by the same seller are serialised on the counter row (negligible for a freelancer), and the number cannot be known before sending;
  - the displayed prefix is the enum member's name: renaming it would change the references;
  - the wrong-series check throws a state-type exception although the number is an argument, accepted for consistency with the other lifecycle errors.
- Alternatives set aside:
  - **a SQL Server `SEQUENCE` or `IDENTITY`**: fast, but values are lost on rollback, leaving gaps;
  - **`MAX(number) + 1`**: two concurrent sends can read the same maximum;
  - **a number at creation**: every deleted draft leaves a gap;
  - **two types, `QuoteNumber` and `InvoiceNumber`**: the compiler forbids mixing them, but year and sequence validation is written twice;
  - **a number without prefix**, added at display time: two numbers of different series would be equal.
