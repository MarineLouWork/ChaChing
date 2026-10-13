# 0036 — "The seller exists" is checked by the use case through `ISellerRepository.Exists`

**Status:** Accepted

## Context

A client belongs to a seller (`Client.SellerId`, [ADR 0012](0012-multi-seller-and-snapshots.md)). `Client.Create` only checks that the id is not empty: the domain does not load other aggregates, so it cannot know whether the seller exists. The rule has to be decided where the ports are, in the use case ([architecture §5](../architecture.md#5-where-a-rule-lives)).

## Decision

- `ISellerRepository` exposes `bool Exists(Guid id)`. **`CreateClient` needs to know whether the seller exists, not to read it**, so it does not load a `Seller`. A `Get` will come with the first use case that reads seller data (`AddQuoteLine`, for the VAT regime).
- `CreateClient` checks before building the client. An unknown seller raises an `ArgumentException` whose `ParamName` is `SellerId`; nothing is stored.
- The tests put a seller **directly in the fake repository**, not through `SetUpSeller`: a use case never calls another use case, and tests stay independent.

## Consequences

- The domain stays free of other aggregates; the rule is tested with the fake, no database.
- The port stays minimal: one query that answers exactly the question asked.
- **Price paid:**
  - **The check and the insert are not atomic**: the seller could disappear in between. In P3 the foreign key on `Client.SellerId` is the real guarantee; the check exists to give a clear error.
  - **A plain `ArgumentException` is not a type of its own**: the API will have to look at `ParamName` to tell a missing seller from another invalid argument, such as a malformed email. A dedicated exception in Application will replace it if this becomes awkward.
- Alternatives set aside:
  - **`Seller? Get(Guid id)`**: loads data nobody uses and invites later use cases to depend on it;
  - **Calling `SetUpSeller` from `CreateClient`**: couples two entry points; shared state goes through ports, not through other use cases;
  - **A dedicated `SellerNotFoundException` now**: cleaner for the API, but there is no API yet;
  - **A `Result` instead of an exception**: the project's rule is that the domain and the use cases throw, and the boundary translates.
