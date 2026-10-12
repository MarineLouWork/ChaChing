# 0012 — Multi-seller, references while drafting, snapshots when sent

**Status:** Accepted

## Context

ChaChing is used by many freelancers: every client, quote and invoice belongs to one of them, the **seller**. A document shows the seller's and the client's details (name, address, SIRET / SIREN). These details change over time (a freelancer moves, a client changes its company name), but a sent quote, and even more an invoice, is a **document**: it must show the details as they were the day it was issued.

## Decision

- **`Seller` is an aggregate** (legal name, address, SIRET, VAT number, VAT regime). `Client`, `Quote` and `Invoice` carry a `SellerId`.
- **A draft only holds references** (`SellerId`, `ClientId`), so it follows the edits.
- **Marking the document as sent or issued copies a snapshot of both parties** into it: `SellerSnapshot` and `ClientSnapshot`, two value objects, since the two parties do not hold the same data. From then on the document never reads `Seller` or `Client` again for display.
- `Client` has a `SellerId`, an `Address`, and an optional `Siren` (B2B).
- How the snapshots are built and checked: ADR 0019.

## Consequences

- A sent document never changes after the fact; a draft follows the edits, which is what the user expects while writing it.
- **Price paid:** the parties' details are duplicated in every sent document, the snapshot is one more value object to map in EF Core, and an error corrected on the client after sending does not fix the sent document (a new one is needed).
- Alternatives set aside:
  - **always reload** (a join at display time): simple, but a document would change after being sent;
  - **snapshot from creation**: a draft would show outdated details while being written;
  - **versioning `Seller` and `Client`**: exact history, far heavier than copying a few fields.
