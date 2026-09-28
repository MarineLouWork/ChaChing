# Domain model

The map of the domain: which aggregate owns which data, and how they refer to each other.
The database schema (P3) will be derived from it, not the other way round.
The decisions behind it are in [`adr/`](adr/README.md).

**Vocabulary:** names follow the European e-invoicing standard **EN 16931** (the one behind Factur-X, UBL and CII), with one exception: `Client`, not `Buyer` (ADR 0015).

## 1. Aggregates and references

```mermaid
classDiagram
    direction LR

    class Seller {
        <<aggregate root>>
    }
    class Client {
        <<aggregate root>>
    }
    class Quote {
        <<aggregate root>>
    }
    class QuoteLine {
        <<entity>>
    }
    class Invoice {
        <<aggregate root - later>>
    }

    Seller "1" <.. "*" Client : SellerId
    Seller "1" <.. "*" Quote : SellerId
    Client "1" <.. "*" Quote : ClientId
    Quote "1" *-- "*" QuoteLine : owns
    Quote "1" <.. "0..1" Invoice : QuoteId
```

- **Solid line (`*--`)**: the quote **owns** its lines. They are reached through the quote only, and live and die with it.
- **Dotted line (`<..`)**: a reference **by id** to another aggregate, never an object. Each aggregate is loaded and saved on its own.
- Every aggregate except `Seller` carries a `SellerId`: that is what separates the data of each freelancer.

## 2. Inside each aggregate

```mermaid
classDiagram
    direction TB

    class Seller {
        <<aggregate root>>
        Guid Id
        string LegalName
        Address Address
        string Siret
        string? VatNumber
        VatRegime VatRegime
    }

    class Client {
        <<aggregate root>>
        Guid Id
        Guid SellerId
        string Name
        string Email
        Address Address
        string? Siren
    }

    class Quote {
        <<aggregate root>>
        Guid Id
        Guid SellerId
        Guid ClientId
        Currency Currency
        QuoteStatus Status
        IReadOnlyList~QuoteLine~ Lines
        Discount? Discount
        QuoteNumber? Number
        DateOnly? IssueDate
        DateOnly? ValidUntil
        SellerSnapshot? SellerSnapshot
        ClientSnapshot? ClientSnapshot
        AddLine(...)
        Send(number, issueDate, validUntil, seller, client)
        Money TotalExclTax
        Money TotalVat
        Money TotalInclTax
    }

    class QuoteLine {
        <<entity>>
        Guid Id
        string Description
        decimal Quantity
        Money UnitPrice
        VatRate VatRate
        Discount? Discount
        Money TotalPrice
    }

    class Money {
        <<value object>>
        decimal Amount
        Currency Currency
    }
    class Discount {
        <<value object - abstract>>
    }
    class Address {
        <<value object>>
        string Line
        string PostalCode
        string City
        string CountryCode
    }
    class VatRate {
        <<value object>>
        decimal Percent
        bool IsExempt
    }
    class QuoteNumber {
        <<value object>>
        string Value
    }
    class SellerSnapshot {
        <<value object>>
        string LegalName
        Address Address
        string Siret
        string? VatNumber
        VatRegime VatRegime
    }
    class ClientSnapshot {
        <<value object>>
        string Name
        Address Address
        string? Siren
    }

    Quote *-- QuoteLine
    Quote --> QuoteNumber
    Quote --> SellerSnapshot
    Quote --> ClientSnapshot
    QuoteLine --> Money
    QuoteLine --> VatRate
    QuoteLine --> Discount
    Seller --> Address
    Client --> Address
    SellerSnapshot --> Address
    ClientSnapshot --> Address
```

## 3. Where each piece of data goes

| Data | Owner | How the quote gets it | EN 16931 |
|---|---|---|---|
| Seller: legal name, address, SIRET, VAT number, VAT regime | `Seller` | `SellerId`, then a **snapshot** when sent | Seller (BG-4) |
| Client: name, address, SIREN (B2B) | `Client` | `ClientId`, then a **snapshot** when sent | Buyer (BG-7) |
| Number, issue date | `Quote` | set by `Send()` | BT-1, BT-2 |
| Currency | `Quote` | at creation | BT-5 (ISO 4217, as `Currency`) |
| Lines: description, quantity, unit price excl. tax, VAT rate | `QuoteLine` | owned | invoice line (BG-25) |
| Totals excl. tax, VAT, incl. tax | `Quote` | **computed**, never stored | document totals (BG-22) |
| Numbering counter (seller, type, year → last number) | infrastructure | behind a port, in the send transaction | — |

## 4. Lifecycle of a quote

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Sent : Send() - number and snapshots
    Sent --> Accepted
    Sent --> Refused
    Refused --> Draft : reworked
    Accepted --> [*] : converted into an invoice
```

- **Draft**: editable, no number, references only.
- **Sent**: frozen. It has a number and snapshots of both parties.

## 5. Out of the MVP

- **E-invoicing (French reform):** from 1 September 2027, a freelancer must **issue** B2B invoices through an approved platform, in Factur-X, UBL or CII. ChaChing will not become a platform: it will send through one, behind a port (`IInvoiceTransmitter`), one adapter per platform.
- Product catalogue, credit notes, quote versioning (ADR 0010).
