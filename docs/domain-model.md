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
    class BillingLine {
        <<entity>>
    }
    class Invoice {
        <<aggregate root - later>>
    }

    Seller "1" <.. "*" Client : SellerId
    Seller "1" <.. "*" Quote : SellerId
    Client "1" <.. "*" Quote : ClientId
    Quote "1" *-- "*" BillingLine : owns
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
        IReadOnlyList~BillingLine~ Lines
        Discount? Discount
        BillingNumber? Number
        DateOnly? IssueDate
        DateOnly? ValidUntil
        SellerSnapshot? SellerSnapshot
        ClientSnapshot? ClientSnapshot
        AddLine(...)
        MarkAsSent(number, issueDate, validUntil)
        BillingTotals Totals
    }

    class BillingLine {
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
    class BillingTotals {
        <<value object>>
        Money TotalNet
        Money TotalWithoutVat
        VatBreakdown
        Money TotalVatAmount
        Money TotalWithVat
    }
    class BillingNumber {
        <<value object>>
        TypeDocument TypeDocument
        int Year
        int Sequence
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

    Quote *-- BillingLine
    Quote --> BillingTotals : computed
    Quote --> BillingNumber
    Quote --> SellerSnapshot
    Quote --> ClientSnapshot
    BillingLine --> Money
    BillingLine --> VatRate
    BillingLine --> Discount
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
| Number, issue date | `Quote` | set by `MarkAsSent()` | BT-1, BT-2 |
| Currency | `Quote` | at creation | BT-5 (ISO 4217, as `Currency`) |
| Lines: description, quantity, unit price excl. tax, VAT rate | `BillingLine` | owned | invoice line (BG-25) |
| Totals excl. tax, VAT, incl. tax | `BillingTotals` (shared with `Invoice`) | **computed** from the lines, never stored | document totals (BG-22) |
| Numbering counter (seller, type, year → last number) | infrastructure | behind a port, in the send transaction | — |

## 4. How the amounts of a quote are computed

**Rule of thumb: every discount comes before the VAT.** Everything stays excl. VAT until the very last step.

```mermaid
flowchart TD
    L["<b>Each line</b><br/>Quantity × UnitPrice<br/>then the line discount"] --> TP["<b>Line.TotalPrice</b><br/>line net amount, excl. VAT"]
    TP --> S["<b>TotalNet</b> (sum of the lines)<br/>excl. VAT, before the quote discount"]
    S --> D["the quote discount<br/>(floored at 0)"]
    D --> TW["<b>TotalWithoutVat</b>"]
    TP --> G["<b>Group the lines by VatRate</b><br/>one base per rate"]
    G --> P["<b>Share TotalWithoutVat pro rata</b><br/>base of the rate ÷ TotalNet × TotalWithoutVat<br/>the last rate takes the remainder"]
    TW --> P
    P --> B["<b>VatBreakdown</b><br/>per rate: TaxableAmount, VatAmount = rate × TaxableAmount"]
    B --> TV["<b>TotalVatAmount</b><br/>sum of the VatAmounts"]
    TW --> TT["<b>TotalWithVat</b><br/>TotalWithoutVat + TotalVatAmount"]
    TV --> TT
```

**Worked example**, with a fixed discount of 155 € on the quote:

| Step | 20 % | 5.5 % | Quote |
|---|---|---|---|
| Lines (`TotalPrice`) | 3 × 500 € − 10 % = **1,350 €** | 1 × 200 € = **200 €** | |
| **TotalNet** (sum of the lines) | | | 1,550 € |
| Quote discount (155 € = 10 % of 1,550 €) | | | − 155 € |
| **TotalWithoutVat** | | | **1,395 €** |
| Share of each rate | 1,350 / 1,550 | 200 / 1,550 | |
| **TaxableAmount** | 1,350 / 1,550 × 1,395 = **1,215 €** | the remainder: 1,395 − 1,215 = **180 €** | 1,395 € ✔ |
| **VatAmount** | 243 € | 9.90 € | **TotalVatAmount: 252.90 €** |
| | | | **TotalWithVat: 1,647.90 €** |

Details and rounding: ADR 0006, 0014, 0020.

## 5. Lifecycle of a quote

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Sent : MarkAsSent() - number, dates, snapshots
    Sent --> Accepted : MarkAsAccepted()
    Sent --> Declined
    Declined --> Draft : ReturnToDraft() - keeps its number
    Accepted --> [*] : converted into an invoice
```

- **Draft**: editable, no number, references only.
- **Sent**: frozen. It has a number and snapshots of both parties.

## 6. Out of the MVP

- **E-invoicing (French reform):** from 1 September 2027, a freelancer must **issue** B2B invoices through an approved platform, in Factur-X, UBL or CII. ChaChing will not become a platform: it will send through one, behind a port (`IInvoiceTransmitter`), one adapter per platform.
- Product catalogue, credit notes, quote versioning (ADR 0010).
