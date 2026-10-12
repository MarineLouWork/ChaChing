# 0008 — One solution, projects created when a layer needs them, folders by who uses the types

**Status:** Accepted

## Context

The target has four production projects (`Domain`, `Application`, `Infrastructure`, `Api`), three test projects and an Angular front end. Questions: one solution or several, every project now or one at a time, and how to sort the files inside the domain.

The domain was first sorted by kind of type (`Discounts/`, `Taxes/`, `Lines/`, `Totals/`…): eleven folders, several holding a single file.

## Decision

- **A single solution**, `ChaChing.slnx`, for all the .NET projects: `src/` for the code, `tests/` for the tests. The Angular front end lives in the same repository (`web/`), outside the solution.
- **A project is created when the layer it holds starts**, not before. A solution only contains; **a project is a dependency boundary checked by the compiler**, and an empty one protects nothing.
- **Domain folders group what changes together**, and namespaces follow folders:

```
ChaChing.Domain/
├── Common/     Money, Currency, Address, ArgumentValidator: used everywhere, owned by no one
├── Parties/    Seller, Client, VatRegime, SellerSnapshot, ClientSnapshot
└── Billing/    BillingLine, BillingTotals, discounts, VatRate, BillingNumber, converter
    ├── Quotes/
    └── Invoices/
```

`Quotes/` and `Invoices/` depend on `Billing/`, never on each other; `Billing/` depends on neither.

## Consequences

- A change to VAT or discounts stays in `Billing/`.
- **Price paid:** the repository does not show the target architecture until every layer exists (the README and `docs/architecture.md` do); each layer is a small construction job (project, references, sometimes moving code); the dependency rule must be checked at each addition. `Seller` and `Client`, two aggregates, share a folder.
- Alternatives set aside:
  - **all seven projects now, empty**: speculative structure, the same over-engineering as an interface with no caller;
  - **several solutions**: only justified for products deployed separately by different teams;
  - **one folder per kind of type**, or **one folder per aggregate** (`Sellers/`, `Clients/`, `Quotes/`): the usual DDD rule, but it leaves the shared billing types without a home.
