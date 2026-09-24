# 0002 — Private constructor + `Create` / `Rehydrate` factories

**Status:** Accepted — 21/09/2026

## Context

Two distinct operations were hiding behind a single constructor: *creating* a client that does not exist yet (the `Id` is generated) and *rehydrating* a client that already exists (the `Id` is provided).

An optional parameter `Guid? id = null` was first chosen for its simplicity. It was rejected: it mixes both intentions behind a `null`, and nothing prevents a caller from passing an `Id` when they meant to create.

## Decision

A `private` constructor and two public static factories. `Rehydrate` rejects `Guid.Empty`.

## Consequences

- The name of the call carries the intention, and the invariants of each entry point are kept apart.
- More ceremony than a public constructor: **it is only justified because the invariants differ**. On an object with a single way of being built, it would be over-engineering.
- In practice, `Rehydrate` will only be used by tests: EF Core rebuilds entities through reflection, bypassing constructors.
