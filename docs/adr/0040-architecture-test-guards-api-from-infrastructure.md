# 0040 — An architecture test keeps controllers away from Infrastructure

**Status:** Accepted (to be implemented in phase 4, with the API project)

## Context

Some code has to pair each port with its adapter (`IQuoteRepository` → `SqlQuoteRepository`). That code is the composition root, and it must reference both Application and Infrastructure. Domain and Application cannot do it without depending on EF Core ([ADR 0031](0031-repository-ports-in-application.md)). The project that starts the process can, so in ASP.NET Core this is `Program.cs` in `ChaChing.Api`.

The reference from Api to Infrastructure is declared for the whole project, though. The compiler would therefore also let a controller use `ChaChingDbContext` or `SqlQuoteRepository` directly, bypassing the use cases and their rules. The controllers, contracts and error handler form the primary adapter, and they must only know Application.

## Decision

- `Program.cs` stays in `ChaChing.Api` and is the only composition root: it calls `AddInfrastructure(...)` and nothing else from Infrastructure.
- **An architecture test** fails if any type in `ChaChing.Api` other than the composition root depends on `ChaChing.Infrastructure`. This covers `Controllers`, `Contracts` and `Errors`. The test is written with NetArchTest or plain reflection, and it runs with the other tests in CI.

## Consequences

- The rule is enforced by a failing test instead of relying on code review.
- No extra project: Api keeps its usual ASP.NET Core shape.
- **Price paid:**
  - The guarantee comes at test time, not at compile time. Until the test runs, a controller that uses the `DbContext` builds without error.
  - The test has to be kept in sync with the namespaces of the Api project. If a new folder is not covered, the test does not protect it.
  - With NetArchTest, there is one more test dependency.
- Alternatives set aside:
  - **No guard, code review only:** simplest, but the rule depends on everyone remembering it;
  - **A separate `ChaChing.Host` project** that alone holds `Program.cs` and references both Api and Infrastructure: the compiler enforces the rule, but it adds a project, and Api becomes a class library of controllers that needs extra wiring. That is too much structure for an application of this size.
