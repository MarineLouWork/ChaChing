# 0010 — `Quote` is an aggregate root; its lines are edited through it, other aggregates are referenced by id

**Status:** Accepted

## Context

A quote holds lines and rules that span several objects: a single currency, a total computed from lines and discounts, a lifecycle. If any code could modify a line directly, nobody would guarantee these rules. `Lines` being read-only only forbids adding or removing: anyone holding a `BillingLine` could still call its methods, even on a sent quote. And the rule "only a draft can be edited" belongs to the quote, since a line does not know the status of its quote.

A quote also points to a client, and the real risk is a quote pointing to a client that does not exist, which the domain cannot check without the database.

## Decision

- **`Quote` is the aggregate root.** Lines are reached through it only: `AddLine`, `UpdateLine(lineId, …)`, `RemoveLine(lineId)`, never `quote.Lines.Add(…)`. They are exposed read-only.
- **`BillingLine` is an entity local to its document** (shared type, ADR 0024): a line is edited over time, so it has an `Id`, meaningful inside its document.
- **`BillingLine.Update(…)` is `internal`**: only the Domain can call it. It validates its own rules, and the root checks the document's rules (the line exists, the currency matches, the quote is a draft). **All or nothing:** every value is validated before any is assigned.
- **One `UpdateLine` for the whole line**, not one method per field: no field has a behaviour of its own, and the user's action is "edit the line".
- **Other aggregates are referenced by a plain `Guid`** (`SellerId`, `ClientId`), the domain only rejects `Guid.Empty`. The existence of the client is checked by the use case that creates the quote, through a repository port.
- **Invariants of the quote:** one currency for all lines and discounts; one client; at least one line to be sent (an empty draft is allowed); no maximum number of lines; a total that is never negative (a refund is a separate document, never a quote); totals **computed, never stored**, so they cannot drift from the lines.
- **Invariants of a line:** a description, a quantity `> 0` in `decimal` (half a day can be billed), a unit price `≥ 0`. A line at €0 is a non-billable line shown on the quote; offering a service is a 100 % discount, not a price of 0.
- **Lines are free text**; no product catalogue in the MVP.
- Lifecycle: ADR 0018.

## Consequences

- Every rule is checked in one place, the root; a line in dollars cannot slip into a quote in euros, and a sent quote cannot be altered through one of its lines.
- Saving is all or nothing: one transaction, one aggregate.
- **Price paid:**
  - more code than public lists (`AddLine`, `RemoveLine`, read-only exposure);
  - `UpdateLine` receives every field, even unchanged ones, and says less about the intent than `ChangeLineQuantity` would. If a field gains a rule of its own, a dedicated method is extracted;
  - a wrong kind of `Guid` (a line id where a client id is expected) is only caught **at runtime**, by the existence check, and only if every entry point goes through it. A test on the use case must cover the "unknown client" case;
  - a declined quote sent back to draft loses the trace of what the client declined (no versioning).
- Alternatives set aside:
  - **`BillingLine` as a value object** replaced on each edit: "change the quantity of line 3" would mean rebuilding it and finding it by position;
  - **one method per field**: four relay methods on the root and four on the line, for no difference in behaviour;
  - **the line holds a reference to its quote** to check the status itself: a two-way dependency;
  - **a strongly typed `ClientId`**: safer at compile time, but a type per entity and conversions to write for a confusion the existence check already catches. To reconsider if ids get mixed up in practice;
  - **a product catalogue**: a feature of its own and a second aggregate, after the MVP.
