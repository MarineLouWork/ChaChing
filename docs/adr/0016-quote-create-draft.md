# 0016 — `Quote.CreateDraft`, no `Rehydrate` yet

**Status:** Accepted — 28/09/2026

## Context

ADR 0002 sets the rule: factories are justified when two entry points have different invariants. `QuoteLine` has none (ADR 0010). `Quote` has two: a **new** quote is always an empty draft with a generated id; a quote **read back** from storage has an existing id, any status, lines, and, once sent, a number, dates and snapshots.

## Decision

- **A private constructor and a public factory `CreateDraft(sellerId, clientId, currency)`.** The name says that a quote is always born as a draft.
- **No `Rehydrate` for now.** EF Core rebuilds objects through a private constructor and reflection, so nothing would call it. It will be added in P3 if the chosen persistence needs it (Dapper, a separate persistence model…).

## Consequences

- `new Quote(...)` no longer compiles outside the class: the only way in is the one that guarantees an empty draft.
- **Price paid:** an asymmetry with `Client`, which has both factories. A test that needs a sent quote must reach it through `Send()`, not build it directly.
- Alternatives set aside:
  - **a public constructor**, as for `QuoteLine`: works, but says nothing about the draft status;
  - **`Create` + `Rehydrate` now**, for symmetry: a dozen parameters and the full revalidation of a sent quote, with no caller.
