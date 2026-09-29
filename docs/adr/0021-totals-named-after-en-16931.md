# 0021 — The document totals take their EN 16931 names

**Status:** Accepted — 29/09/2026 · Applies ADR 0015, renames the totals of ADR 0014 and 0020

## Context

ADR 0015 says the domain uses the EN 16931 name of a concept when there is one. The totals of the quote had been named from common business English: `TotalExclTax`, `TotalVat`, `TotalInclTax`. The standard names them *Invoice total amount without VAT* (BT-109), *Invoice total VAT amount* (BT-110) and *Invoice total amount with VAT* (BT-112).

## Decision

- The totals are renamed **`TotalWithoutVat`**, **`TotalVatAmount`** and **`TotalWithVat`**, on the quote now and on the invoice later.
- ADR 0014 and 0020 keep their old wording; this ADR replaces the names they use.

## Consequences

- ADR 0015 holds with no exception other than `Client`, and the Factur-X export maps the totals one to one.
- **Price paid:** a rename of an existing, tested property (`TotalExclTax`, 10 tests) and of the domain map; "excl. tax / incl. tax" is what most French-speaking developers expect to read.
- Alternative set aside: **keep the common names** and list them as a second exception in ADR 0015 — no rename, but the rule would erode at the first friction.
