# Layer 0-A — Compiler Pairs (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-002-001 — "wire up" + "build" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the PayPal payment gateway and wire up the payment capture and refund APIs."

**Compiler analysis:**
- "Build" → IMPLEMENT (Rule 11: build verb fires IMPLEMENT)
- "wire up" → API_CONTRACT (Rule 22: "wire up" fires API_CONTRACT)
- "APIs" → API_CONTRACT (reinforces API_CONTRACT)
- Integration-context vocabulary present: "payment gateway", "APIs" → IMPLEMENT routes to 4.3 (Rule 3)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition (Rule 16: IMPLEMENT present, no refactor verb)
**DomainId:** software-development

---

## B17-0A-002-002 — "wire up" signals API_CONTRACT, not a generic phrase

**Input:**
"Wire up the Stripe webhook endpoint to the order fulfillment service."

**Compiler analysis:**
- "Wire up" → API_CONTRACT (Rule 22)
- "webhook" → API_CONTRACT (Rule 5: webhook fires API_CONTRACT)
- No build/implement/write/create verb present → IMPLEMENT does NOT fire
- Result: API_CONTRACT only

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3 (API_CONTRACT alone routes to 3.3, NOT 4.3 — Rule 10)
**TransitionType:** documentation (Rule 17: API_CONTRACT alone → documentation)
**DomainId:** software-development

**Teaching point:** "wire up" alone (without a build verb) yields API_CONTRACT only → 3.3 routing.

---

## B17-0A-002-003 — Both "build" and "wire up" present in same sentence

**Input:**
"Build the PayPal refund handler and wire up the PayPal REST API v2 endpoints."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "wire up" → API_CONTRACT
- "API" → API_CONTRACT (reinforces)
- Integration vocabulary: "PayPal REST API", "endpoints" → sub-phase 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-002-004 — Negative: "build" without integration vocabulary → 4.1 not 4.3

**Input:**
"Build the PaymentCalculatorService and implement the discount logic."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- Vocabulary present: "service", "logic" → service/logic context (Rule 3)
- NO integration-context vocabulary: no connector, integration, webhook, API, external service, third-party
- IMPLEMENT routes to 4.1 (backend services/logic), NOT 4.3

**CompiledPredicateIds:** ["IMPLEMENT"]
**SubPhase:** 4.1
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** IMPLEMENT routes to 4.3 only when integration-context vocabulary is co-present. Without it, IMPLEMENT routes to 4.1.
