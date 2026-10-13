# 0037 — The seller's VAT number: French format, consistent with the SIRET

**Status:** Accepted

## Context

`Seller.VatNumber` is a mandatory mention on the invoice of a VAT-liable seller. It used to be only normalised (spaces removed, upper case): `"20"` was accepted, and a wrong number could be printed on an issued invoice, which only a credit note corrects ([ADR 0029](0029-invoice-lifecycle.md)).

A seller is identified by a SIRET, so it is a French business. A French VAT number is `FR` + a 2-character key + the 9 digits of the SIREN, which are the first 9 digits of the SIRET.

## Decision

After normalisation, in `Seller` (no other aggregate or outside call is needed):

1. **Format:** `FR`, then 2 characters (the key), then 9 digits; 13 characters in all.
2. **Consistency:** the 9 digits are the first 9 digits of the seller's own SIRET.

`null` stays valid (no number); a blank value stays refused, the caller converts an empty form field to `null`.

## Consequences

- An invalid number is refused at creation, with the aggregate's own data and without a database.
- **Price paid:**
  - **A seller with a non-French number is refused.** Acceptable while the SIRET already ties a seller to France; to revisit if foreign sellers are ever admitted.
  - **The key is not checked**, so a number can have a valid format and a wrong key, or not exist at all.
- Alternatives set aside:
  - **Key check** (computable from the SIREN): more rigour, but a formula to maintain and test for a small gain, since a mistyped key is also caught by the VAT authority;
  - **Existence check against VIES** (European Commission service): the only real proof, but an outside call that can fail, so it is a port and an adapter in the Application and Infrastructure layers, not a domain rule. Not built;
  - **Format only**, without the SIRET consistency: simpler, but leaves out the strongest rule, which costs nothing since both values belong to the same aggregate.
