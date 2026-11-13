# Architecture Decision Records

One file per structuring decision: the **context**, the **decision**, and above all the **consequences**, including the **price paid** and the **alternatives set aside**. An ADR describes the decision as it stands today, not the road that led to it.

Format: `NNNN-short-title.md`. When a decision is replaced, a new ADR supersedes the old one, which then gets the status `Superseded by NNNN`.

The numbers are stable identifiers (the code refers to them), so there are gaps: the decisions that were merged into another one or judged not structuring (a rename, a rounding detail) were dropped on 2026-10-09, before publication. Their earlier wording is in the git history.

## Domain modelling

| # | Decision |
|---|---|
| [0001](0001-entities-identity-and-factories.md) | Entities are identified by their `Id` and built through factories |
| [0003](0003-money-value-object.md) | `Money` is an immutable, signed value object, rounded at construction |
| [0004](0004-currency-iso-enum.md) | `Currency` is an `enum` of ISO 4217 numeric codes |
| [0007](0007-discount-abstract-class.md) | `Discount` is an abstract class (template method), its contract announces the currency exception |
| [0010](0010-quote-aggregate.md) | `Quote` is an aggregate root; lines edited through it, other aggregates referenced by id |
| [0015](0015-vocabulary-follows-en-16931.md) | The domain vocabulary follows EN 16931 |
| [0027](0027-argument-validator.md) | `ArgumentValidator` for technical preconditions, business rules stay in the aggregates |
| [0037](0037-seller-vat-number-format-and-siret-consistency.md) | The seller's VAT number: French format, consistent with its SIRET |

## Billing rules

| # | Decision |
|---|---|
| [0012](0012-multi-seller-and-snapshots.md) | Multi-seller, references while drafting, snapshots when sent |
| [0019](0019-snapshots-built-from-aggregates.md) | Snapshots are built from the aggregates and checked by the document |
| [0013](0013-gapless-numbering.md) | Gapless numbering, assigned when sent, one `BillingNumber` for every document |
| [0014](0014-vat-rate-per-line.md) | VAT: a rate per line, the regime on the seller |
| [0020](0020-vat-breakdown-pro-rata.md) | VAT breakdown per rate, the document discount shared pro rata |
| [0018](0018-quote-lifecycle.md) | Quote lifecycle: guarded transitions, a number kept forever |
| [0024](0024-invoice-aggregate-shared-lines-and-totals.md) | `Invoice` is its own aggregate; lines and totals are shared types |
| [0029](0029-invoice-lifecycle.md) | Invoice lifecycle: four stored statuses, overdue is derived |
| [0032](0032-quote-to-invoice-converter.md) | A domain converter turns an accepted quote into a draft invoice |

## Architecture and application layer

| # | Decision |
|---|---|
| [0008](0008-solution-and-folder-layout.md) | One solution, projects created when a layer needs them, folders by who uses the types |
| [0031](0031-repository-ports-in-application.md) | Repository ports live in the Application layer |
| [0034](0034-already-invoiced-rule-in-use-case.md) | "Already invoiced" is decided by the use case; the port returns statuses |
| [0035](0035-ports-exchange-aggregates-not-commands.md) | Ports exchange aggregates; the use case builds them from the command |
| [0036](0036-seller-existence-checked-by-use-case.md) | "The seller exists" is checked by the use case through `ISellerRepository.Exists` |
| [0038](0038-repository-ports-are-asynchronous.md) | Repository ports are asynchronous from the start |
| [0039](0039-client-ownership-checked-by-use-case.md) | "The client belongs to the seller" is checked by the use case, one error for both cases |
| [0040](0040-architecture-test-guards-api-from-infrastructure.md) | An architecture test keeps controllers away from Infrastructure |

See also: [architecture](../architecture.md) · [domain model](../domain-model.md) · [use cases](../use-cases.md) · [roadmap](../roadmap.md)
