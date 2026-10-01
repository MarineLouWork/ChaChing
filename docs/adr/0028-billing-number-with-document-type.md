# 0028 — One `BillingNumber` for quotes and invoices, carrying its document type

**Status:** Accepted — 01/10/2026. Supersedes the first point of [ADR 0013](0013-gapless-numbering-on-send.md) (`QuoteNumber`); the rest of 0013 stands.

## Context

ADR 0013 created `QuoteNumber` (`DEV-2026-0042`) and announced a separate series for invoices (`FAC-…`). The invoice needs the same value object: a year, a sequence, the same validation. Sharing the type as it was would have printed `DEV-` on invoices.

## Decision

- **`QuoteNumber` becomes `BillingNumber(TypeDocument, Year, Sequence)`**, in `Billing/`, used by `Quote` and `Invoice`.
- **`TypeDocument`** is an enum (`DEV`, `FAC`), in `Billing/`. Its name is the prefix of the displayed reference. It starts at 1: a forgotten value is never a valid one.
- The type is validated at construction (`Guard.ValidateEnum`, ADR 0027) and **takes part in equality**: `DEV-2026-0042` and `FAC-2026-0042` are two different numbers.
- One sequence per seller, per type, per year: unchanged (ADR 0013).

## Consequences

- No duplicated validation; the credit note will only add an enum value.
- **Price paid:**
  - every creation of a number takes one more argument;
  - the displayed prefix is the enum member's name: renaming `DEV` or `FAC` would change the references, and the two names are French abbreviations in an English code base;
  - the compiler does not stop an invoice number from being given to a quote. **`Quote.MarkAsSent` does not check the type yet**: to add with the invoice lifecycle, on both aggregates.
- Alternatives set aside:
  - **two types, `QuoteNumber` and `InvoiceNumber`**: the compiler forbids mixing them, but year and sequence validation is written twice;
  - **a number without prefix**, added at display time: simplest, but two numbers of different series would be equal.
