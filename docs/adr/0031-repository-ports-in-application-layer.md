# 0031 — Repository ports live in the Application layer

**Status:** Accepted — 08/10/2026. Opens P2 (use cases); follows the hexagonal reading (Cockburn) done the same day.

## Context

The use cases (`ConvertToInvoice`, `RecordPayment`…) must load and save `Quote` and `Invoice` without knowing where they are stored (SQL Server later, an in-memory list in tests). That need is expressed as interfaces (ports), implemented by adapters. The question is where the ports are declared.

Fact in the current code: `Quote`, `Invoice`, `Money`… never load or save anything. Only the use cases will call a repository.

## Decision

- **Ports (`IQuoteRepository`, `IInvoiceRepository`) are declared in the Application project**, next to the use cases that call them. Chosen by Marine: the domain holds objects and business rules, the application orchestrates and talks to the outside.
- **Adapters live in the project of their technology.** The SQL Server adapter (EF Core, P3) goes in Infrastructure; an in-memory fake goes in the test project, or in Infrastructure if it is reused elsewhere.
- **Dependencies point inward:** Infrastructure → Application → Domain. The domain references nothing; Application never references Infrastructure.

## Consequences

- The domain stays free of any persistence concept: no `using` of EF Core, no repository, nothing to mock to test an aggregate.
- Use cases are tested with a fake repository plugged on the port, with no database.
- **Price paid:** if one day a domain service needs to load an aggregate itself, the interface has to move to the Domain project (or the service becomes a use case). Accepted: nothing in the domain needs it today.
- Alternatives set aside:
  - **Ports in the Domain** (Evans' original placement, the plan's first wording): closer to textbook DDD, but the domain would declare a contract that none of its own code calls;
  - **Ports in Infrastructure, beside EF Core**: the application would have to reference Infrastructure to see them, which reverses the dependency and defeats the purpose.
