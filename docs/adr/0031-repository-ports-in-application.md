# 0031 — Repository ports live in the Application layer

**Status:** Accepted

## Context

The use cases (`ConvertToInvoice`, `RecordPayment`…) must load and save aggregates without knowing where they are stored (SQL Server in production, an in-memory list in tests). That need is expressed as interfaces (ports), implemented by adapters, following the hexagonal architecture. The question is where the ports are declared.

In the current code, `Quote`, `Invoice`, `Money`… never load or save anything. Only the use cases call a repository.

## Decision

- **Ports (`IClientRepository`, `IQuoteRepository`, `IInvoiceRepository`…) are declared in the Application project**, next to the use cases that call them. The domain holds objects and business rules; the application orchestrates and talks to the outside.
- **Adapters live in the project of their technology:** the SQL Server adapter (EF Core) in Infrastructure; an in-memory fake in the test project.
- **Dependencies point inward:** Infrastructure → Application → Domain. The domain references nothing; Application never references Infrastructure.
- **A port appears with the first use case that needs it, not before.**

## Consequences

- The domain stays free of persistence: no EF Core `using`, no repository, nothing to mock to test an aggregate.
- Use cases are tested with a fake repository plugged on the port, with no database.
- **Price paid:** if a domain service ever needs to load an aggregate itself, the interface has to move to the Domain (or the service becomes a use case). Nothing needs it today.
- Alternatives set aside:
  - **Ports in the Domain** (Evans' original placement): closer to textbook DDD, but the domain would declare a contract none of its own code calls;
  - **Ports in Infrastructure, beside EF Core**: the application would have to reference Infrastructure to see them, which reverses the dependency and defeats the purpose.
