# 0001 — Entities are identified by their `Id` and built through factories

**Status:** Accepted

## Context

Two clients can share the same name and email, and a client stays the same after changing their email. By default `Equals` on a `class` compares references: the same client loaded twice from the database would be two different objects.

Creating an entity and rebuilding one that already exists are also two different operations: the first generates the `Id`, the second receives it. A single constructor with an optional `Guid? id = null` mixes both intentions behind a `null`.

## Decision

- **Equality is on the `Id` only** (`Equals` and `GetHashCode` overridden together). `==` is not overloaded.
- **A private constructor and named static factories** carry the intention: `Client.Create` / `Client.Rehydrate`, `Seller.Create`, `Quote.CreateDraft`, `Invoice.CreateDraft`. The name `CreateDraft` says that a document is always born as an empty draft.
- **A factory is only justified when the entry points have different invariants.** `BillingLine` has a single way of being built, so it keeps one public constructor.
- **No `Rehydrate` on `Quote` and `Invoice` yet.** EF Core rebuilds objects through a private constructor and reflection, so nothing would call it. It is added if the persistence adapter needs it.

## Consequences

- Two instances of the same client are equal; `Dictionary`, `HashSet` and `Distinct()` behave.
- **Price paid:**
  - `==` stays a reference comparison, which is worth knowing;
  - more ceremony than a public constructor, and an asymmetry: `Client` has both factories, `Quote` only one;
  - a test that needs a sent quote must reach it through `MarkAsSent`, it cannot build one directly.
- Alternatives set aside:
  - **equality on `Name` / `Email`**: two namesakes would become one client, and a client would stop being themselves after an email change;
  - **an optional `id` parameter**: simple, but nothing stops a caller from passing an id when they meant to create;
  - **`Create` + `Rehydrate` on every aggregate, for symmetry**: a dozen parameters and a full revalidation, with no caller.
