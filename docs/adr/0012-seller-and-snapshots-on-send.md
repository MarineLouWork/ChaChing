# 0012 — Multi-seller, references while drafting, snapshots when sent

**Status:** Accepted — 28/09/2026

## Context

ChaChing is used by many freelancers: every quote, invoice and client belongs to one of them, the **seller**. A quote shows the seller's and the client's details (name, address, SIRET / SIREN).

These details change over time: a freelancer moves, a client changes its company name. A sent quote, and even more an invoice, is a **document**: it must show the details as they were on the day it was issued.

## Decision

- **`Seller` is an aggregate** (legal name, address, SIRET, VAT number, VAT regime). `Client`, `Quote` and later `Invoice` carry a `SellerId`.
- **A draft only holds references** (`SellerId`, `ClientId`): it always shows the current details.
- **`Send()` copies a snapshot** of both parties into the quote (`SellerSnapshot` and `ClientSnapshot`, two value objects: the two parties do not hold the same data, and a single type would need fields that only make sense for one of them). From then on, the quote never reads `Seller` or `Client` again for display.

## Consequences

- A sent document never changes after the fact, even if the seller or the client is edited.
- A draft follows the edits, which is what the user expects while writing it.
- **Price paid:** the parties' details are duplicated in every sent document, and the snapshot is one more value object to map in EF Core. An error corrected on the client after sending does not fix the sent quote: a new quote is needed.
- `Client` must gain a `SellerId` and an `Address` (and an optional `Siren` for B2B).
- Alternatives set aside:
  - **always reload** (a join at display time): simple, but a document would change after being sent;
  - **snapshot from creation**: the draft would show outdated details if the client is corrected while writing it;
  - **versioning `Seller` and `Client`**: exact history, but far heavier than copying a few fields.
