# 0033 — `ArgumentValidator` replaces `Guard`, without the `Validate` prefix

**Status:** Accepted — 08/10/2026. Renames the helper introduced by [ADR 0027](0027-guard-for-technical-preconditions.md); its decision (technical checks shared, business rules in the aggregates) stands.

## Context

`Guard` is a known name (guard clauses), but the class does more than stop an invalid call: each method **returns the validated value**, and one of them normalises it (`Trim`). At the call site, `Guard.ValidateGuid(...)` also says the same thing twice and does not say what is validated.

## Decision

- The class is **`ArgumentValidator`** (`Common/ArgumentValidator.cs`), chosen by Marine: the name says what it does.
- The methods lose their `Validate` prefix, since the class name already says it:
  - `ValidateGuid` → `ArgumentValidator.Guid`
  - `ValidateNullableGuid` → `ArgumentValidator.NullableGuid`
  - `ValidateEnum<TEnum>` → `ArgumentValidator.Enum`
  - `ValidateAndTrimEmptyString` → `ArgumentValidator.NonBlankText` (refuses null, empty and blank text, returns it trimmed)
- No behaviour change: same checks, same exceptions, same messages. 229 tests stay green.

## Consequences

- Call sites read as a sentence: `SellerId = ArgumentValidator.Guid(sellerId, nameof(sellerId));`.
- **Price paid:** methods named `Guid` and `Enum` hide the `System.Guid` and `System.Enum` types inside the class, which therefore writes `System.Guid.Empty` and `System.Enum.IsDefined`. The ADRs 0026 to 0028 still say `Guard`: they record what was decided at the time and are not rewritten.
- Alternatives set aside:
  - **keep `Guard`**: recognised by every .NET developer, but the name hides the `Trim` and the returned value;
  - **`Preconditions`**: names the concept, but says nothing about arguments;
  - **keep the `Validate…` method names** (`ArgumentValidator.ValidateGuid`): redundant.
