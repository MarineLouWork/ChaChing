# 0011 — `ClientId` is a plain `Guid`, its existence is checked by the application layer

**Status:** Accepted — 28/09/2026

## Context

`Quote` references its client by id (ADR 0010). The id can be a plain `Guid` or a dedicated type (`readonly record struct ClientId(Guid Value)`) that lets the compiler reject any other `Guid`, such as a line id or a quote id.

Either way, the real risk is a quote pointing to a client that does not exist. The domain cannot check that: it would need the database.

## Decision

- **`ClientId` is a plain `Guid`.** The domain only rejects `Guid.Empty`.
- **The client's existence is checked by the use case** that creates the quote (application layer, P2). It asks for the client through a repository port; the infrastructure implements the port.

## Consequences

- No extra type, and nothing to map for EF Core.
- An id of the wrong kind is still caught: it matches no client, so the existence check rejects it.
- **Price paid:** that mistake shows up **at runtime**, not at compile time, and only if every entry point goes through the check. A test on the use case must cover the "unknown client" case.
- Alternative set aside: a **strongly typed id** (`ClientId`). Safer at compile time, but a type per entity, and conversions to write, for a confusion the existence check already catches. To be reconsidered if ids start being mixed up in practice.
