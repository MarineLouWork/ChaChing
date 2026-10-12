# 0035 — Ports exchange aggregates, the use case builds them from the command

**Status:** Accepted

## Context

`CreateClient` is the first use case written in the Application layer. It receives a `CreateClientCommand` (the input data) and must end with a client stored. Two designs were tried on the same day:

1. a port `Guid Add(CreateClientCommand command)`, the repository building the `Client` itself;
2. a port `void Add(Client client)`, the use case building the `Client`.

The first one made the use case a pass-through, and the in-memory fake called `Client.Create`, which a real adapter would have had to repeat.

## Decision

- **A repository port takes and returns aggregates** (`void Add(Client client)`), never commands. It offers only what a use case calls today: no `Get`, `Update` or `Delete` until one needs them.
- **The use case builds the aggregate** from the command with the domain factory (`Client.Create`), hands it to the port, and returns the new `Id`.
- **No `try`/`catch` in the use case.** An invalid command makes the domain throw its own exception, which reaches the caller untouched; translating it is the job of the boundary (the API, later).
- **The test observes the fake repository** (`Clients`), not the returned `Id` alone: the result is checked against what was stored.

## Consequences

- Construction and its invariants live in one place (the domain). Each adapter (in-memory, EF Core) only stores and retrieves.
- The use case is the one place where a command becomes an aggregate. The port contract stays stable if the command's shape changes.
- **Price paid:** the use case carries a small mapping (`command.Name` → `Client.Create(…)`), repeated for each creation use case, and the command duplicates `Client.Create`'s parameters.
- Alternatives set aside:
  - **Port taking the command** (tried): construction moves into every adapter, and Application's input type leaks into the contract the infrastructure implements;
  - **A generic `IRepository<T>` with full CRUD** from the start: methods nobody calls, and two of them were stubbed with `NotImplementedException`;
  - **Wrapping failures in a generic `Exception`** in the use case: it hides whether the cause is invalid data or a failure, which the caller must tell apart.
