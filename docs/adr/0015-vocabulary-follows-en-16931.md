# 0015 — The domain vocabulary follows EN 16931

**Status:** Accepted — 28/09/2026

## Context

The French e-invoicing reform relies on the European standard **EN 16931**, implemented by the Factur-X, UBL and CII formats. It names every concept of an invoice: *Seller*, *Buyer*, invoice line, VAT category, totals… ChaChing will have to export its invoices in one of these formats (after the MVP).

The first name chosen for the freelancer, `Issuer`, came from common usage and appears nowhere in the standard.

## Decision

- **When EN 16931 names a concept, the domain uses that name** (ubiquitous language): `Seller`, `VatRate` with its categories, currency as ISO 4217 codes (already the case, ADR 0004)…
- **One exception: `Client`**, not `Buyer`. It is the freelancer's everyday word, and the entity already exists.
- **The domain does not depend on the standard**: no reference to a Factur-X library, no field codes (BT-…) in the code. Mapping to the formats will be the job of an export adapter, in the infrastructure.

## Consequences

- The future Factur-X export becomes a mechanical translation: almost every concept has a direct counterpart.
- The domain map (`docs/domain-model.md`) states the EN 16931 counterpart of each data item.
- **Price paid:** some names are less natural for a freelancer ("seller" for someone who sells services), and the `Client` → `Buyer` translation must not be forgotten in the adapter.
- Alternatives set aside:
  - **free vocabulary** (`Issuer`…): more natural at first, but a translation to maintain at every export;
  - **`Buyer` everywhere**: fully consistent, but it forces renaming an existing, tested entity for no business gain.
