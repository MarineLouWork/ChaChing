# 0038 — Repository ports are asynchronous from the start

**Status:** Accepted

## Context

A port is a contract: the use case must not know what stands behind it ([ADR 0031](0031-repository-ports-in-application.md)). Today the only implementations are in-memory fakes, which have nothing to wait for. In P3 the adapter will talk to SQL Server, which is input/output and should not block a thread.

A synchronous signature would tell every implementation to block, and would leak the assumption that "this does no input/output" into the contract. Changing it later breaks every adapter, every use case and every test at once.

## Decision

- Every port method returns a `Task` or a `Task<T>`, is named with the `Async` suffix, and takes a trailing `CancellationToken cancellationToken = default`.
- Use cases follow: `Task<Guid> CreateClientAsync(command, cancellationToken = default)`, and pass the token to every port call.
- The fakes are **not** `async`: they return `Task.CompletedTask` or `Task.FromResult(value)` and ignore the token.
- Tests are `async Task`, use `await`, and `Assert.ThrowsAsync`.

## Consequences

- The contract does not depend on the adapter: EF Core in P3 and the HTTP endpoints in P4 plug in without changing a signature.
- The token is already in place, so request cancellation in P4 needs no refactoring.
- **Price paid:**
  - `async` propagates up to the test: every caller must `await`. A forgotten `await` in a test makes it pass without checking anything (the compiler warns for a call, not for `Assert.Throws`).
  - One more parameter on every signature, which no fake uses today.
  - The two first use cases and their tests had to be rewritten.
- Alternatives set aside:
  - **Synchronous ports now, asynchronous in P3**: simpler to read today, but a breaking change on every adapter and caller, for no benefit in the meantime;
  - **No `CancellationToken` yet**: adding it later is a signature change too, so there is no saving.

The earlier ADRs name the port methods without the suffix (`Exists`, `Add`); the current names are `ExistsAsync`, `AddAsync`.
