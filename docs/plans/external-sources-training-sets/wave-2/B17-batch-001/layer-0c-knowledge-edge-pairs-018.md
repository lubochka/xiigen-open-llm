# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Zuora billing connector edges

---

## B17-0C-018-001 — Edge: billing domain does not introduce new predicate triggers

**Assertion:** Billing-specific vocabulary (subscription, renewal, invoice, proration) fires no predicates. Integration vocabulary rules are unchanged.

**Billing terms — no predicate:**
- subscription, billing, invoice, proration, renewal, churn, dunning, metering — domain nouns, no predicate

**Integration vocabulary in billing context:**
| Input fragment | Predicate |
|---|---|
| "Zuora REST connector" | API_CONTRACT (connector — Rule 4) |
| "billing API endpoint" | API_CONTRACT (API, endpoint — reinforcers) |
| "subscription webhook" | API_CONTRACT (webhook — Rule 5) |
| "OAuth client credentials" | AUTHORIZATION (OAuth — Rule 7) + SECURITY (implicitly from client credentials context — only if "token" is present) |

---

## B17-0C-018-002 — Edge: "REST" in "REST connector" does not fire independently

**Input:** "Build the Zuora REST connector."

**Edge assertions:**
- "Build" → IMPLEMENT
- "REST" — technology label, not a predicate trigger by itself
- "connector" → API_CONTRACT (fires because of "connector")
- "REST" reinforces the integration context but does not independently fire any predicate

**Result:** ["IMPLEMENT", "API_CONTRACT"]

**Teaching point:** "REST" (REpresentational State Transfer) is a technology term. It reinforces the understanding that an external API is involved, but the predicate fires from "connector", not from "REST" itself.

---

## B17-0C-018-003 — Edge: "subscription lifecycle" without API/connector vocabulary → 4.1

**Input:** "Implement the subscription renewal logic and billing cycle state machine."

**Edge assertions:**
- "Implement" → IMPLEMENT
- "subscription renewal logic" — billing domain, internal logic
- "state machine" — internal implementation pattern
- NO integration vocabulary: no connector, no API, no webhook, no external service

**Result:** ["IMPLEMENT"] → 4.1

**Teaching point:** Billing domain requests that describe internal logic (state machine, renewal logic) without external service vocabulary route to 4.1, not 4.3.

---

## B17-0C-018-004 — Edge: "OAuth client credentials" fires AUTHORIZATION (via "OAuth") and may fire SECURITY (via "token")

**Input:** "Build the Zuora REST connector, implement the OAuth 2.0 client credentials flow, and secure the bearer token for all billing API calls."

**Step-by-step:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7: "oauth" any case → AUTHORIZATION)
- "token" → SECURITY (Rule 6: "bearer token" contains "token" → SECURITY)
- "secure" → SECURITY (Rule 8: reinforces; fires once)
- "API" → API_CONTRACT (reinforces)

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**AUTHORIZATION fires from "OAuth", not from "client credentials".** "Client credentials" is the OAuth grant type name — it does not independently trigger AUTHORIZATION.
