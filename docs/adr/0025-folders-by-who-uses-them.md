# 0025 — Domain folders grouped by who uses the types

**Status:** Accepted — 29/09/2026

## Context

The domain was sorted by kind of type (`Discounts/`, `Taxes/`, `Lines/`, `Totals/`, `Sellers/`, `Clients/`…): eleven folders, several holding a single file, and `Money` and `Currency` alone at the root of `Billing/`. Marine found it scattered.

## Decision

Folders group what **changes together**, and namespaces follow folders:

```
ChaChing.Domain/
├── Common/     Money, Currency, Address — used everywhere, owned by no one
├── Parties/    Seller, Client, VatRegime, SellerSnapshot, ClientSnapshot
└── Billing/    BillingLine, BillingTotals, discounts, VatRate, VatSubtotal
    ├── Quotes/
    └── Invoices/
```

`Quotes/` and `Invoices/` depend on `Billing/`, never on each other; `Billing/` depends on neither.

## Consequences

- Five folders instead of eleven, none with a single file; a change to VAT or discounts stays in `Billing/`.
- **Price paid:** `Seller` and `Client`, two separate aggregates, share a folder (their repositories will join them); `VatRegime` sits in `Parties/` as a seller attribute although documents read it; every `using` changed once.
- Alternatives set aside:
  - **one folder per kind of type** (the former layout): scattered, single-file folders;
  - **one folder per aggregate** (`Sellers/`, `Clients/`, `Quotes/`…): the usual DDD rule, but leaves the shared billing types without a home.
