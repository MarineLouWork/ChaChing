# ChaChing — instructions for AI agents

Billing app for freelancers: quotes, invoices, payments, French invoicing rules. .NET 10, C#, xUnit; then EF Core, SQL Server, ASP.NET Core, Angular. Everything in the repository is written in English.

## How we work

- **The agent proposes, the developer decides.** The agent never creates, modifies or deletes a file without explicit permission, and asks for confirmation before every change. When it finds a problem, it reports it; it does not rewrite.
- **The developer writes the new code and makes every design decision.** With permission, the agent researches business rules, lays out options, writes tests from a validated example, writes the ADRs, and writes repetitive code that follows an existing model.
- **An AI does not know, it predicts.** Every claim (an API, a package, a configuration option, a legal rule) is checked against the code, the documentation or the source, and the agent says when it is unsure.
- **A design choice appears** (where a rule lives, a new type, port, signature, pattern, HTTP code, contract): skill `design-decision`. Nothing is coded before the developer decides.
- **Before every commit:** skill `pre-commit-review`, on the diff only.
- Small loops: one task → green tests → review → commit.

```bash
dotnet build
dotnet test
```

## Architecture rules

Details in `docs/architecture.md`; each decision has its ADR in `docs/adr/`.

- Dependencies point to the Domain. Domain references nothing; Application → Domain; Infrastructure → Application; Api → Application, and Infrastructure **only from `Program.cs`** (ADR 0040).
- No EF Core, ASP.NET or `System.Net` in Domain or Application.
- A port is an interface declared in Application (0031). It exchanges aggregates, never commands (0035), and is `async` with a `CancellationToken` (0038). An adapter translates; it never decides (0034).
- An invariant lives in its aggregate, changed only through the root. Collections are exposed as `IReadOnlyList`; no public setter. A rule across aggregates lives in the use case, backed by a database constraint when concurrent requests could break it.
- The domain never reads the clock: today is a parameter. The application gets it from an `IClock` port.
- Errors: `ArgumentException` with `ParamName` for a bad value, `InvalidOperationException` for a forbidden state, `ArgumentNullException.ThrowIfNull`. Domain and use cases throw; the HTTP boundary translates. No `try/catch` that hides a failure.
- Every query is scoped to the authenticated seller (`SellerId` read from the token). Another seller's resource gets the same error as an unknown one (0039).
- `decimal` for money. KISS and YAGNI: no abstraction without a real caller.

## Tests

- Tests come first. For a new calculation: a worked numeric example, validated by the developer, then the tests.
- One test per business rule. The test name tells the truth. Every assert can fail: break the code, the test must go red. Never change an assert to match the code.
- Exceptions: arrange, then `var action = () => …;` on its own line, then `Assert.Throws`, and assert `ParamName`.
- Application tests use the hand-written in-memory fakes in `tests/ChaChing.Application.Tests/InMemoryAggregates`.

## Decisions

Each design decision is recorded in `docs/adr/NNNN-short-title.md` (Context / Decision / Consequences), with **the price paid and the alternatives set aside**, and listed in `docs/adr/README.md`. A replaced decision is never rewritten: a new ADR supersedes it.

## Never

- Commit a secret, or add a package without checking that it exists and who publishes it.
- Log a password, a token or an IBAN.
- Commit or push unless the developer asks.
