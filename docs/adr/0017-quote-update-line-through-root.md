# 0017 — Lines are edited through the root, with a single `UpdateLine`

**Status:** Accepted — 28/09/2026

## Context

A line of a draft quote must be editable (description, quantity, unit price, discount) and removable. `Lines` is read-only, but that only forbids adding or removing: anyone holding a reference to a `QuoteLine` could still call its public methods, even on a sent or accepted quote.

The rule "only a draft can be edited" belongs to `Quote`: a line does not know the status of its quote.

## Decision

- **`Quote.UpdateLine(lineId, description, quantity, unitPrice, discount)` and `Quote.RemoveLine(lineId)` are public.** The root checks the quote's rules: the line exists, the currency matches, and (with the lifecycle) the quote is a draft.
- **`QuoteLine.Update(...)` is `internal`**: only the Domain project can call it. It checks the line's own rules, reusing the validation of the constructor.
- **All or nothing:** every value is validated before any is assigned. A failed update leaves the line unchanged.
- **One method for the whole line**, not one per field: no field has a behaviour of its own, and the user's action is "edit the line" (a form, one save).

## Consequences

- There is one way in to modify a quote: its root. A sent quote cannot be altered through one of its lines.
- **Price paid:** the caller sends every field, even those that did not change. The method says less about the intent than `ChangeLineQuantity` would.
- If a field gains a rule of its own (a traced price change, for instance), a dedicated method will be extracted.
- Alternatives set aside:
  - **one method per field** (`ChangeLineQuantity`, `ChangeLinePrice`…): four relay methods on the root and four on the line, for no difference in behaviour;
  - **the line holds a reference to its quote** to check the status itself: a two-way dependency;
  - **`QuoteLine` as a value object** replaced on each edit: set aside by ADR 0010.
