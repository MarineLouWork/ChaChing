# 0001 — `Client` is identified by its `Id`, not by its data

**Status:** Accepted — 21/09/2026

## Context

Two clients can have the same name and the same email. Conversely, a client remains the same after changing their email. By default, `Equals` on a `class` compares references: the same client loaded twice from the database would be seen as two different objects.

## Decision

`Client` is an **entity**: `Equals` and `GetHashCode` are overridden on the `Id` only.

## Consequences

- Two instances of the same client are equal, however fresh their data is.
- `GetHashCode` must follow `Equals`, otherwise `Dictionary`, `HashSet` and `Distinct()` break.
- `==` is **not** overloaded: it remains a reference comparison. An accepted difference, worth knowing.
- Comparing on `Name` / `Email` is explicitly rejected: two namesakes would become the same client, and a client would stop being themselves after an email change.
