# 0018 — Quote lifecycle: guarded transitions, a number kept forever

**Status:** Accepted — 28/09/2026

## Context

ADR 0010 sets the statuses: `Draft` → `Sent` → `Accepted` or `Refused`, and a refused quote can go back to `Draft`. ADR 0013 assigns the number when the quote is sent. Three questions were left open: what is forbidden in each status, what happens to the number of a reworked quote, and how to name the transitions.

## Decision

- **`Refused` becomes `Declined`**, the usual term in quoting tools (Xero, Zoho…): a client *declines* an offer. `Accepted` stays.
- **The transitions are named `MarkAs…`** (`MarkAsSent`, `MarkAsAccepted`, `MarkAsDeclined`), plus `ReturnToDraft`. The domain **records** a fact; it sends no email and receives no answer itself. A method called `Send()` would suggest otherwise.
- **Each transition checks the current status.** A transition that is not allowed throws an `InvalidOperationException`: the arguments are fine, the state is not.
- **`MarkAsSent(number, issueDate, validUntil)`** requires at least one line and `validUntil` not before `issueDate` (the same day is allowed). Every check runs before any change.
- **Only a draft can be edited:** `AddLine`, `UpdateLine` and `RemoveLine` throw outside `Draft`.
- **A quote keeps its number forever.** A declined quote sent back to draft keeps it; when sent again, it must receive the **same** number. The use case asks the counter for a new number only when `quote.Number` is null.

## Consequences

- An accepted quote cannot change any more, through any path.
- The numbering stays gapless and one quote is one number, even after rework.
- **Price paid:** the use case has to know the rule "reuse the number if there is one"; the domain only enforces it. As in ADR 0010, the declined version is not kept: the reworked quote overwrites it.
- Not handled yet: an expired quote (accepted after `ValidUntil`), the quote discount that can only be set at creation.
- Alternatives set aside:
  - **`Send()`, `Accept()`, `Refuse()`**: shorter, but `Send()` reads as if the domain sent the email;
  - **`UpdateStatus…`**: describes the mechanics (a field changes) rather than the business event;
  - **a new number at each send**: simpler for the use case, but one quote would carry several numbers over its life;
  - **a state machine library or the State pattern**: four transitions do not justify it.
