# 0039 — "The client belongs to the seller" is checked by the use case, with one error for both cases

**Status:** Accepted

## Context

A quote references a client and a seller by id ([ADR 0010](0010-quote-aggregate.md)). `Quote.CreateDraft` only checks that the ids are not empty: the domain does not load other aggregates. The use case has to make sure that the client exists **and** belongs to the seller who creates the quote, otherwise a seller could quote another seller's client.

## Decision

- `IClientRepository` exposes `ExistsForSellerAsync(clientId, sellerId)`. `CreateDraftQuote` needs a yes or no, not the client, so it does not load a `Client`.
- A single check. Whether the client is unknown or belongs to another seller, the use case raises the **same** `ArgumentException`, with `ParamName` `ClientId` and a neutral message ("The client doesn't exist."). Nothing is stored.
- `CreateDraftQuote` does **not** use `ISellerRepository`: a client that belongs to the seller implies that the seller exists, since `CreateClient` refuses an unknown seller ([ADR 0036](0036-seller-existence-checked-by-use-case.md)).
- `IClientRepository.ExistsAsync(id)` was removed: no use case needs it (a port appears with the first use case that needs it).
- Creating a quote and adding lines to it are two use cases: `CreateDraftQuote` and `AddQuoteLine`. The VAT default of the seller's regime belongs to the second.

## Consequences

- The tenant rule of P4 ("every query is scoped to the authenticated seller") already has its shape: a filter on the seller in the adapter.
- An attacker cannot learn that an id exists under another seller: both cases look the same.
- **Price paid:**
  - The use case relies on an invariant: a seller is never deleted, and a client is only created for an existing seller. If sellers become deletable, this use case must check the seller again.
  - The port carries a little of the rule (the filter on the seller); the foreign key in P3 stays the real guarantee for existence.
  - As with [ADR 0036](0036-seller-existence-checked-by-use-case.md), the API must read `ParamName` to tell a missing client from another invalid argument.
- Alternatives set aside:
  - **`GetAsync(clientId)` then compare `SellerId`**: loads the whole aggregate to read one field, and the "same error" has to be remembered by every caller;
  - **Three checks (client, seller, ownership) with distinct errors**: more precise, but the error type reveals that the id exists under another seller;
  - **`InvalidOperationException` for ownership**: no `ParamName`, and a different type from "unknown", which leaks the same information.
