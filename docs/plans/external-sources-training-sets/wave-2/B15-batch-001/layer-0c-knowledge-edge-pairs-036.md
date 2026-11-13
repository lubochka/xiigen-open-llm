# Layer 0-C — Knowledge-Edge Pairs (036)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-036-001 — Complete ARCHITECTURE trigger reference (Round 2 confirmation)

Confirmed ARCHITECTURE triggers across Round 2 services:
- "domain model" (payment / subscription / currency / billing cycle / proration / dispute resolution)
- "bounded context" (wallet bounded context)
- "architect" verb (architect the tax jurisdiction model, architect the credit ledger)
- "[domain noun] model" pattern (tax model, jurisdiction model, calculation model, currency model)

---

## B15-kedge-036-002 — Complete API_CONTRACT trigger reference (Round 2 confirmation)

Confirmed API_CONTRACT triggers across Round 2 services:
- "integrate" (integrate with Stripe, integrate with forex provider, integrate with subscription system)
- "wire up" (wire up the forex API, wire up the dispute API)
- "connect" (connect to OpenExchangeRates)
- "contracts" (API contracts, service contracts, data contracts, invoice data contracts, chargeback API contracts)

---

## B15-kedge-036-003 — Complete SECURITY trigger reference (Round 2 confirmation)

Confirmed SECURITY triggers across Round 2 services:
- "secure" (secure the payment endpoints, secure the credit balance)
- "harden" (harden the withdrawal endpoint)
- "protect" (protect balance operations, protect the conversion rate cache)
- "encrypt" (encrypt the invoice storage, encrypt the API credentials)
- "authenticate" (authenticate dispute resolution requests)

---

## B15-kedge-036-004 — Domain nouns that must NOT suppress IMPLEMENT (Round 2)

"payment", "billing", "invoice", "tax", "currency", "exchange rate", "wallet", "credit", "chargeback", "proration", "subscription" — all domain nouns.
None suppress IMPLEMENT. IMPLEMENT fires when any IMPLEMENT trigger verb is present.

---

## B15-kedge-036-005 — "strategy pattern" and "observer pattern" do NOT fire ARCHITECTURE

Design pattern names (strategy, observer, factory, decorator) do NOT fire ARCHITECTURE.
ARCHITECTURE fires only from: domain model / bounded context / architect verb / architecture noun / [domain] model pattern.
"use the strategy pattern" → IMPLEMENT only (feature_addition).
