# 0018 — Quote lifecycle: guarded transitions, a number kept forever

**Status:** Accepted

## Context

A quote goes from `Draft` to `Sent`, then `Accepted` or `Declined`, and a declined quote can be reworked. Questions: what is forbidden in each status, what happens to the number of a reworked quote, and how to name the transitions.

## Decision

- **Statuses:** `Draft → Sent → Accepted | Declined`, and `Declined → Draft`. `Declined` is the usual term in quoting tools (Xero, Zoho): a client *declines* an offer. An accepted quote is final.
- **Transitions are named `MarkAs…`** (`MarkAsSent`, `MarkAsAccepted`, `MarkAsDeclined`) plus `ReturnToDraft`. The domain **records** a fact; it sends no email and receives no answer itself, which a method called `Send()` would suggest.
- **Each transition checks the current status.** A transition that is not allowed throws `InvalidOperationException`: the arguments are fine, the state is not.
- **`MarkAsSent(number, issueDate, validUntil, seller, client)`** requires at least one line and `validUntil` not before `issueDate` (the same day is allowed). Every check runs before any change.
- **Only a draft can be edited:** `AddLine`, `UpdateLine` and `RemoveLine` throw outside `Draft`.
- **A quote keeps its number forever.** A declined quote sent back to draft keeps it and must receive the **same** number when sent again: the use case asks the counter for a new number only when `quote.Number` is null.
- The document discount is set at creation only.

## Consequences

- An accepted quote cannot change any more, through any path.
- Numbering stays gapless and one quote is one number, even after rework.
- **Price paid:** the use case must know the rule "reuse the number if there is one", the domain only enforces it. The declined version is not kept: the reworked quote overwrites it. Not handled yet: an expired quote (accepted after `ValidUntil`).
- Alternatives set aside:
  - **`Send()`, `Accept()`, `Refuse()`**: shorter, but `Send()` reads as if the domain sent the email;
  - **`UpdateStatus…`**: describes the mechanics (a field changes) rather than the business event;
  - **a new number at each send**: simpler for the use case, but one quote would carry several numbers;
  - **a state machine library or the State pattern**: four transitions do not justify it.
