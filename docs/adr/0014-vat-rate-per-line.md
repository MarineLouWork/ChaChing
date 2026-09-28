# 0014 — VAT: a rate per line, the regime on the seller

**Status:** Accepted — 28/09/2026

## Context

Prices so far are plain `Money` amounts, without any notion of tax. A French quote or invoice shows the totals excluding tax, the VAT, and the totals including tax, and an invoice must break the VAT down **by rate**. Rates in France: 20 %, 10 %, 5.5 %, 2.1 %.

Many freelancers are exempt (*franchise en base*): they charge no VAT and must print "TVA non applicable, art. 293 B du CGI".

## Decision

- **Unit prices are stored excluding tax.** VAT and totals including tax are **computed**, never stored (as the total already is, ADR 0010).
- **Each `QuoteLine` has a `VatRate`** (a value object), 20 % by default.
- **The VAT regime belongs to the `Seller`** (`Subject` or `Exempt`). An exempt seller's lines carry `VatRate.Exempt` (EN 16931 VAT category "E", 0 %), and the document shows the 293 B mention.
- **The use case picks the default rate** of new lines from the seller's regime. **`Send()` checks consistency** against the seller snapshot: an exempt seller cannot send a line with VAT.
- The quote exposes `TotalExclTax`, `TotalVat` (broken down by rate) and `TotalInclTax`.

## Consequences

- A quote mixing rates is possible, and the per-rate breakdown required on invoices comes for free.
- **Price paid:** one more field on every line, and a rounding rule to settle: round the VAT per line, or per rate on the total? (ADR 0006 is to be revisited on totals.)
- Alternative set aside: **one rate per quote**. Simpler, and enough for most freelancers, but moving to a rate per line later would change the model of every stored line.
