# 0010 — `Quote` is an aggregate, `QuoteLine` a local entity

**Status:** Accepted — 25/09/2026

## Context

A quote holds lines, and rules that span several objects at once: a single currency, a total computed from the lines and the discounts, a lifecycle (draft, sent, accepted…). If any code could modify a line directly, nobody would guarantee these rules.

## Decision

- **`Quote` is the aggregate root.** Its lines are only reached through it: `quote.AddLine(...)`, never `quote.Lines.Add(...)`. Lines are exposed read-only.
- **`QuoteLine` is an entity, local to its quote.** A line is edited over time (change its quantity, remove it), so it must be found again: it has an `Id`, meaningful only inside its quote.
- **`Quote` references its client by `ClientId`**, not by a `Client` object. `Client` is its own aggregate.
- **Invariants of `Quote`:**
  - a single currency for all the lines and discounts;
  - always one client (`ClientId` set);
  - the total is **computed**, never stored, so it cannot drift from the lines;
  - at least one line to be **sent**; an empty draft is allowed;
  - no maximum number of lines.
- **Invariants of `QuoteLine`:** a description, a unit price ≥ 0.
- **Discounts live at both levels:** on a line and on the quote total. Line discounts are applied first, then the total's.
- **Lifecycle:** `Draft` → `Sent` → `Accepted` or `Refused`. Only a draft can be edited. An **accepted** quote is final. A **refused** quote can go back to `Draft` to be reworked.
- **Lines are free text** (description, quantity, unit price). No product catalogue for the MVP.

## Consequences

- Every rule of the quote is checked in one place, the root. A line in dollars cannot slip into a quote in euros.
- Saving is all or nothing: one transaction = one aggregate.
- Computing the total on every read costs a loop over the lines, negligible for a quote.
- **Price paid:** more code than public lists (`AddLine`, `RemoveLine`, read-only exposure), and the order of discounts is now a business rule to test.
- A refused quote that goes back to draft loses the trace of what the client refused. Accepted for the MVP; versioning quotes would fix it later.
- Alternatives set aside:
  - **`QuoteLine` as a value object**: simpler, but "change the quantity of line 3" would mean rebuilding the line and finding it by position;
  - **a product catalogue** with preset prices: a feature of its own, and a second aggregate. Postponed after the MVP.
