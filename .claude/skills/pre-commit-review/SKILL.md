---
name: pre-commit-review
description: Demanding review of the ChaChing diff before a commit - architecture, DDD, SOLID, DRY, KISS, YAGNI, tests and security. Use before every commit, when code is finished or tests turn green, and on the agent's own code before proposing it. Reports; never rewrites.
---

# Pre-commit review

The same standard applies to the developer's code and to the agent's. **Report, never rewrite.**

## 1. Read before judging

- `git status` and `git diff HEAD`: review **the diff only**, plus the tests it touches and the related ADRs.
- `dotnet build` (any warning is reported) and `dotnet test` (give the number of green tests).

## 2. Checklist

**Architecture and DDD** (the rules in `CLAUDE.md`). Red flags:
- an EF Core, ASP.NET or `System.Net` `using` in Domain or Application; a `new` of an adapter outside `Program.cs` or the tests;
- a port taking a command or a DTO, or a `bool` hiding a business rule (`ExistsActive…`);
- a `public List<>`, a public setter, a method bypassing the aggregate root;
- a rule copied into the use case when the domain already holds it; a rule in a controller or an adapter;
- an interface, base class, factory or generic "just in case"; an `if (type == …)` that grows with each variant;
- a `try/catch` returning `null`, `false` or `Guid.Empty`.

**C#**: `decimal` for money; `async` all the way, `CancellationToken` passed on; `&&` vs `||`; enums start at 1; `Guid?` for an absent value, not `Guid.Empty`.

**Names**: a name that lies is a blocking issue.

**Tests**: each assert can fail; the name tells the truth; no assert changed to fit the code; one test per rule; the exception pattern of `CLAUDE.md`; tests written before the code.

**Security**
- Access: `[Authorize]` on every endpoint; every query filtered by `SellerId` from the token, in the query itself; another seller's resource → same error as unknown.
- Input and output: request and response contracts, never a domain entity bound from or returned to HTTP (mass assignment, overexposure).
- Injection: LINQ or interpolated `FromSql` only; never concatenated SQL or `FromSqlRaw` with user input.
- Secrets and data: no secret in code or in a committed `appsettings`; passwords hashed by Identity, IBAN encrypted; no password, token or IBAN in logs or in a JWT.
- Packages: every new package exists, has a known publisher, no known vulnerability.
- `Program.cs`: `UseAuthentication` before `UseAuthorization`; no developer page or Swagger outside Development; HTTPS; CORS with explicit origins.
- Front end: no `innerHTML` or `bypassSecurityTrust…` on data.

**Scope**: the diff touches only what was asked.

## 3. Report

- One grouped pass: what is right first, then **blocking issues** (rule in one sentence, file and line), then minor notes.
- A design choice found during the review goes through `design-decision`.
