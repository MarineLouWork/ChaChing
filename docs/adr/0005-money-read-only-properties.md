# 0005 — `Money` properties are read-only

**Status:** Accepted — 22/09/2026

## Context

Validation placed in a property initializer is **not replayed by `with`**: the generated copy constructor copies the fields, then writes the listed properties through their `init` accessor. `money with { Currency = (Currency)888 }` therefore produced an invalid `Money`.

Behaviour checked in a throwaway project. The `init` accessor workaround works, but it _also_ requires an initializer fed by the primary constructor parameter; otherwise that parameter is never read (warning `CS8907`) and the value stays at `default`.

## Decision

`Amount` and `Currency` become `{ get; }`. `with` on these members no longer compiles (`CS0200`). Operations rebuild through `new Money(...)`.

## Consequences

- An invalid `Money` becomes **impossible to write**, not just detectable at runtime.
- The test that guarded against the `with` bypass goes away: the bug no longer exists. A test removed because the defect can no longer be expressed is a win, not a loss of coverage.
- Changing the currency of an amount would be a **conversion** (it needs an exchange rate), not a copy. That operation has no reason to exist.
- Price paid: we lose the elegant `this with { ... }` in `Add`, `Subtract` and `Multiply`.
