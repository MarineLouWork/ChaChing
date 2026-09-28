# Architecture Decision Records (ADR)

One file per structuring decision: the **context**, the **decision**, and above all the **consequences**, including the price paid.

Format: `NNNN-short-title.md`, status `Proposed` / `Accepted` / `Superseded by NNNN`.
An outdated ADR is never rewritten: a new one supersedes it. The history of decisions is part of the value.

| # | Decision | Status |
|---|---|---|
| [0001](0001-client-identity-by-id.md) | `Client` is identified by its `Id`, not by its data | Accepted |
| [0002](0002-client-create-rehydrate.md) | Private constructor + `Create` / `Rehydrate` | Accepted |
| [0003](0003-money-signed-value-object.md) | `Money` is a signed value object | Accepted |
| [0004](0004-money-currency-iso-enum.md) | `Currency` is an `enum` of ISO 4217 codes | Accepted |
| [0005](0005-money-read-only-properties.md) | `Money` properties are read-only | Accepted |
| [0006](0006-money-rounding-at-construction.md) | Rounding to 2 decimals, at construction | Accepted |
| [0007](0007-discount-abstract-class.md) | `Discount` is an abstract class; its contract announces the currency exception | Accepted |
| [0008](0008-projects-created-when-needed.md) | One solution, projects created only when a layer needs them | Accepted |
| [0009](0009-discount-business-rules.md) | Discounts: zero accepted, rates from 0 to 100 % | Accepted |
| [0010](0010-quote-aggregate.md) | `Quote` is an aggregate, `QuoteLine` a local entity | Accepted |
| [0011](0011-client-id-as-guid.md) | `ClientId` is a plain `Guid`, its existence is checked by the application layer | Accepted |
| [0012](0012-seller-and-snapshots-on-send.md) | Multi-seller, references while drafting, snapshots when sent | Accepted |
| [0013](0013-gapless-numbering-on-send.md) | Gapless numbering, assigned when sent | Accepted |
| [0014](0014-vat-rate-per-line.md) | VAT: a rate per line, the regime on the seller | Accepted |
| [0015](0015-vocabulary-follows-en-16931.md) | The domain vocabulary follows EN 16931 (except `Client`) | Accepted |
| [0016](0016-quote-create-draft.md) | `Quote.CreateDraft`, no `Rehydrate` yet | Accepted |
| [0017](0017-quote-update-line-through-root.md) | Lines are edited through the root, with a single `UpdateLine` | Accepted |
| [0018](0018-quote-lifecycle.md) | Quote lifecycle: guarded transitions, a number kept forever | Accepted |

The overall map of aggregates is in [`../domain-model.md`](../domain-model.md).

ADRs 0001 to 0007 were written in French in the `Katas` repository, then imported and translated on 24/09/2026. The code names were translated at the same time (`Remise` → `Discount`, `Devise` → `Currency`, `Creer` → `Create`…).
