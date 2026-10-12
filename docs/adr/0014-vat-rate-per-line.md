# 0014 — VAT: a rate per line, the regime on the seller

**Status:** Accepted

## Context

A French document shows the totals without tax, the VAT, and the totals with tax, and an invoice must break the VAT down **by rate** (20 %, 10 %, 5.5 %, 2.1 %). Many freelancers are exempt (*franchise en base*): they charge no VAT and must print "TVA non applicable, art. 293 B du CGI".

## Decision

- **Unit prices are stored without tax.** VAT and totals with tax are **computed**, never stored (ADR 0010).
- **Each `BillingLine` has a `VatRate`** (a value object), 20 % by default.
- **The VAT regime belongs to the `Seller`** (`Subject` or `Exempt`). An exempt seller's lines carry `VatRate.Exempt` (EN 16931 category "E", 0 %), and the document shows the 293 B mention.
- **The use case picks the default rate** of new lines from the seller's regime.
- **A document refuses to leave the draft with an inconsistent line:** `BillingLine.IsAllowedFor(VatRegime)` holds the knowledge of which rate goes with which regime, and `Quote.MarkAsSent` and `Invoice.MarkedAsIssued` each throw their own `InvalidOperationException` when an exempt seller carries a line with VAT. A subject seller may still bill an exempt line.
- How the VAT is computed and rounded: ADR 0020.

## Consequences

- A document mixing rates is possible, and the per-rate breakdown required on invoices comes for free.
- The 293 B mention can never sit next to a VAT amount.
- **Price paid:** one more field on every line, and the "which rate for which regime" rule is checked in two places (one per aggregate), through a single definition on the line.
- Alternative set aside: **one rate per document**, simpler and enough for most freelancers, but moving to a rate per line later would change the model of every stored line.
