# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: FedEx/UPS shipping connector edges

---

## B17-0C-015-001 — Edge: shipping domain connector vocabulary identical to payment domain

**Assertion:** The integration vocabulary rules apply identically across shipping, payment, email, and all other connector domains. There is no domain-specific vocabulary exception.

**Shipping connector vocabulary that fires API_CONTRACT:**
| Token | Rule |
|---|---|
| connector | Rule 4 |
| webhook | Rule 5 |
| API, endpoint | Reinforcer |
| wire up | Rule 22 |

**Shipping-specific terms that do NOT fire any predicate:**
- shipping, freight, parcel, tracking, label, rate, carrier — domain nouns, no predicate

---

## B17-0C-015-002 — Edge: two connectors (FedEx + UPS) in one request — predicate fires once

**Input:** "Build the FedEx and UPS shipping connector adapters."

**Edge assertion:** "connector" appears conceptually twice (for FedEx and UPS) but the predicate fires once.

**Result:**
- API_CONTRACT: fires once (predicate is a set member, not a counter)
- IMPLEMENT: fires once ("Build")

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**NOT:** ["IMPLEMENT", "API_CONTRACT", "API_CONTRACT"] — predicates are never duplicated

---

## B17-0C-015-003 — Edge: OAuth in shipping connector → same AUTHORIZATION rule as payment connector

**Input:** "Create the FedEx OAuth connector, implement the API access token refresh logic."

**Edge assertions:**
- "Create" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT (Rule 4)
- "OAuth" → AUTHORIZATION (Rule 7) — applies in shipping context exactly as in payment context
- "token" → SECURITY (Rule 6) — applies in shipping context exactly as in payment context

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**Teaching point:** Rule 7 (OAuth → AUTHORIZATION) and Rule 6 (token → SECURITY) are not restricted to payment connectors. They fire in any connector domain where the trigger words appear.

---

## B17-0C-015-004 — Edge: "wire up" in shipping context → API_CONTRACT (Rule 22)

**Input:** "Build the UPS connector and wire up the package rate and shipment creation endpoints."

**Edge assertions:**
- "Build" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT (Rule 4)
- "wire up" → API_CONTRACT (Rule 22: fires once even with two firings from connector and wire up)
- "endpoints" → API_CONTRACT (reinforces)

**Result:** ["IMPLEMENT", "API_CONTRACT"]
**Note:** Three separate API_CONTRACT triggers (connector, wire up, endpoints) — predicate fires once.
