# 0008 — One solution, projects created only when a layer needs them

**Status:** Accepted — 24/09/2026

## Context

The target architecture has four production projects (`Domain`, `Application`, `Infrastructure`, `Api`), three test projects and an Angular front end (see the README). Today, only the domain has code: `Client`, `Money` and the discounts.

Two questions:
- one solution or several?
- create every project now, or only the ones that have code?

## Decision

- **A single solution**, `ChaChing.slnx`, for all the .NET projects. `src/` for the code, `tests/` for the tests. The Angular front end lives in the same repository (`web/`), outside the solution.
- **Only `ChaChing.Domain` and `ChaChing.Domain.Tests` are created.** Each other project is added when the phase that needs it starts: `Application` with the use cases, `Infrastructure` with persistence, `Api` with the HTTP layer.

## Consequences

- A solution is only a container for the IDE and `dotnet build`: it enforces nothing. **A project is a dependency boundary checked by the compiler.** An empty project protects nothing, so it has no reason to exist yet.
- The layering is not frozen before it is studied: the hexagonal architecture comes before the application layer, and may change the split.
- **Price paid:** the repository does not show the target architecture yet, only the README does. Each new layer will be a small construction job: create the project, add references, sometimes move code. And the dependency rule (everything points towards the Domain) must be remembered and checked at each addition.
- Alternatives set aside:
  - **all seven projects now, empty**: speculative structure, the same over-engineering as an interface with no caller;
  - **several solutions** (one per layer or per app): only justified for products deployed separately and owned by different teams.
