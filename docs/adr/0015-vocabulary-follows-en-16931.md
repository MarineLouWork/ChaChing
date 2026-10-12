# 0015 — The domain vocabulary follows EN 16931

**Status:** Accepted

## Context

The French e-invoicing reform relies on the European standard **EN 16931**, implemented by the Factur-X, UBL and CII formats. It names every concept of an invoice. ChaChing will have to export its invoices in one of these formats after the MVP. A vocabulary made up from common usage (`Issuer` for the freelancer) appears nowhere in the standard and would have to be translated at every export.

## Decision

- **When EN 16931 names a concept, the domain uses that name** (ubiquitous language): `Seller`, `VatRate` with its categories, ISO 4217 currencies (ADR 0004), and the totals:

| Property | EN 16931 | Meaning |
|---|---|---|
| `TotalNet` | BT-106, *sum of invoice line net amounts* | sum of the lines, **before** the document discount |
| `TotalWithoutVat` | BT-109 | after the document discount |
| `TotalVatAmount` | BT-110 | sum of the VAT of each rate |
| `TotalWithVat` | BT-112 | what the client pays |

- **One exception: `Client`**, not `Buyer`: it is the freelancer's everyday word.
- **The domain does not depend on the standard:** no Factur-X library, no field code (BT-…) in the code. Mapping to the formats is the job of an export adapter in the infrastructure.
- Each of these properties has an XML doc saying "before" or "after" the document discount.

## Consequences

- The future export becomes a mechanical translation, and the domain map states the EN 16931 counterpart of every data item.
- **Price paid:**
  - some names are less natural for a freelancer ("seller" for someone who sells services); "without tax / with tax" is what most French-speaking developers expect to read;
  - `TotalNet` does not say *before which discount*: in everyday French the *net à payer* is the final amount, so only the XML doc tells the difference;
  - the `Client` → `Buyer` translation must not be forgotten in the adapter.
- Alternatives set aside:
  - **free vocabulary**: more natural at first, a translation to maintain at every export;
  - **`Buyer` everywhere**: fully consistent, but renames a tested entity for no business gain;
  - **`LinesTotal`** instead of `TotalNet`: unambiguous, but it does not match the other `Total…` names;
  - **the exact standard name** (`SumOfLineNetAmounts`): unambiguous, but long.
