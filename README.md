# ChaChing

Quotes and invoicing for freelancers.

**Stack:** .NET 10 · C# · xUnit — then SQL Server, EF Core, ASP.NET Core and Angular.

## Architecture

The target architecture. Only the **Domain** exists today: the other layers are created when a feature needs them.

```
                    ┌──────────────────────┐
                    │  web                 │  Angular, outside the .NET solution
                    │  (front end)         │  calls the API over HTTP
                    └──────────┬───────────┘
                               │ HTTP / JSON
┌──────────────────────────────▼───────────────────────────────┐
│  ChaChing.Api              ASP.NET Core                      │
│  endpoints, validation, auth, composition root               │
│  (wires the implementations to the ports)                    │
└───────────┬──────────────────────────────────┬───────────────┘
            │                                  │
┌───────────▼──────────────┐     ┌─────────────▼───────────────┐
│  ChaChing.Application    │     │  ChaChing.Infrastructure    │
│  use cases               │◄────┤  EF Core, SQL Server,       │
│  CreateQuote,            │     │  payment provider…          │
│  ConvertToInvoice…       │     │  = the adapters that        │
│  + the ports (interfaces)│     │  implement the ports        │
└───────────┬──────────────┘     └─────────────┬───────────────┘
            │                                  │
┌───────────▼──────────────────────────────────▼───────────────┐
│  ChaChing.Domain                                             │
│  Client, Money, Discount, then Quote and Invoice             │
│  depends on NOTHING: no EF Core, no ASP.NET, no database     │
└──────────────────────────────────────────────────────────────┘
```

**The rule:** dependencies point towards the Domain, never the other way round. The Domain knows nothing about the database or HTTP, so business rules are tested without starting anything.

## Solution layout

```
ChaChing/
├── ChaChing.slnx
├── src/
│   ├── ChaChing.Domain/             ✅
│   ├── ChaChing.Application/        (to come)
│   ├── ChaChing.Infrastructure/     (to come)
│   └── ChaChing.Api/                (to come)
├── tests/
│   ├── ChaChing.Domain.Tests/       ✅
│   ├── ChaChing.Application.Tests/  (to come)
│   └── ChaChing.IntegrationTests/   (to come)
├── web/                             (to come: Angular, same repository, outside the solution)
└── docs/adr/                        design decisions
```

## Getting started

```bash
dotnet build
dotnet test
```

## Design decisions

Every design decision is recorded as an ADR, with the alternatives set aside and the price paid: see [`docs/adr`](docs/adr/README.md).
