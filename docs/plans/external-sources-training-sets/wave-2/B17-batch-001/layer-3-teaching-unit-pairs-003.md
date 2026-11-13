# Layer 3 — Teaching Unit Pairs (003)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-003-001 — What signals 4.3 vs 4.1 within IMPLEMENT context

**The single decision point:** When IMPLEMENT fires, the router must choose between 4.3 (Integration) and 4.1 (Backend Services). The decision depends entirely on vocabulary co-occurrence.

**Decision rule:** Is integration-context vocabulary present in the same input?
- YES → 4.3
- NO → 4.1

---

## B17-TU-003-002 — Integration-context vocabulary that triggers 4.3 routing

**4.3 trigger vocabulary** (any one token is sufficient):
```
connector    API       integration  webhook
external     third-party  gateway   SDK
```

**4.1 default vocabulary** (triggers 4.1 when no 4.3 token is present):
```
service   class     logic    function
method    repository  domain  entity
handler   (when internal, no external reference)
```

---

## B17-TU-003-003 — Vocabulary co-occurrence examples

**Scenario A — IMPLEMENT → 4.3 (integration token present):**
"Implement the **connector** for Stripe payments." → 4.3
"Implement the **API** client for PayPal." → 4.3
"Implement the **webhook** handler." → 4.3
"Implement the **external** payment gateway." → 4.3

**Scenario B — IMPLEMENT → 4.1 (no integration token):**
"Implement the PaymentService class." → 4.1
"Implement the discount calculation logic." → 4.1
"Implement the UserRepository." → 4.1
"Implement the checkout domain model." → 4.1

---

## B17-TU-003-004 — Edge case: both vocabulary sets present → 4.3 wins

**Input:** "Implement the PaymentConnectorService class with its integration logic."

Both present:
- Integration tokens: "connector", "integration" → 4.3 signal
- Service/logic tokens: "service", "class", "logic" → 4.1 signal

**Result:** 4.3 wins. Integration context overrides service/logic context.

**Rule:** When both vocabulary sets are co-present in one input, 4.3 always wins over 4.1.

**Practical implication:** If a class is named "ConnectorService" or "IntegrationHandler", the integration token in the name is sufficient to route IMPLEMENT to 4.3, even if the rest of the description uses service/logic vocabulary.
