---
name: design-decision
description: Frames any design choice in ChaChing (where a rule lives, a new type, port, project, signature, pattern, HTTP code, contract, EF mapping that touches the domain) and gets the developer's decision BEFORE any code. Use as soon as such a choice appears, however small, and before writing code that depends on it.
---

# Design decision: the criterion, the options, then the developer decides

## 1. Recognise a decision

It is one when neither the existing code nor an ADR imposes the answer. First read `docs/adr/README.md` and the related ADRs, `docs/architecture.md`, `docs/domain-model.md`, `docs/use-cases.md`. If an ADR already decides, apply it and cite it. To challenge it, say so: a new ADR will supersede it.

## 2. Present it, short

1. **The need**, in business words: what the freelancer does.
2. **The criterion**, in one sentence (e.g. "who needs this information to decide?").
3. **Two or three options** in a table: what each gives, and **the price paid**.
4. Only the principles that separate the options (the rules in `CLAUDE.md`, the questions below).
5. **A recommendation**, in one line, with its reason. Between two correct options, the simpler one, saying what it will cost if the need grows.
6. **The question.** Stop and wait for the answer. Group several decisions in one message.

**SOLID, as questions**
- **S**: does this class have more than one reason to change?
- **O**: does adding a variant force a change to existing code?
- **L**: does a subtype throw or require more than its base type promises?
- **I**: does a client depend on methods it never calls?
- **D**: does high-level code depend on an implementation instead of an abstraction?

**DRY** is about knowledge, not text: two similar pieces of code carrying different rules stay apart. **KISS**: the simplest solution to the stated need. **YAGNI**: no abstraction, parameter or port without a real caller; a port appears with the first use case that needs it.

## 3. After the decision

- With permission, write the ADR (see `CLAUDE.md`, *Decisions*) and update `architecture.md`, `use-cases.md` or `roadmap.md` if the decision touches them.
- New calculation: worked numeric example validated by the developer, then the tests.
- Once the code exists, check that it matches the ADR; report any gap.
