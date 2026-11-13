# Layer 0-A — Compiler Pairs (013)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Stripe payment connector

---

## B17-0A-013-001 — "build" + "connector" + "endpoint" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Stripe payment connector and implement the charge, capture, and dispute webhook endpoints."

**Compiler analysis:**
- "Build" → IMPLEMENT (Rule 11: build verb fires IMPLEMENT)
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4: connector fires API_CONTRACT)
- "webhook" → API_CONTRACT (Rule 5: webhook fires API_CONTRACT)
- "endpoints" → API_CONTRACT (reinforces)
- Integration vocabulary: connector, webhook, endpoints → 4.3 context (Rule 3)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-013-002 — "integrate Stripe" alone → API_CONTRACT only (no IMPLEMENT)

**Input:**
"Integrate the Stripe payment API into the checkout service."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1: integrate fires API_CONTRACT ONLY, never IMPLEMENT)
- "API" → API_CONTRACT (reinforces)
- No build verb present → IMPLEMENT does NOT fire
- Result: API_CONTRACT only

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3 (API_CONTRACT alone → 3.3, not 4.3 — Rule 10)
**TransitionType:** documentation
**DomainId:** software-development

**Teaching point:** "integrate" alone yields API_CONTRACT only → 3.3. The word "Stripe" is a proper noun — it does not fire any predicate. The word "payment" is a domain noun — it does not fire any predicate.

---

## B17-0A-013-003 — "build" + "token" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Build the Stripe connector and implement the idempotency key and payment intent token handling."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "token" → SECURITY (Rule 6: token fires SECURITY unconditionally)
- Integration vocabulary: connector, token → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 3.4 (SECURITY)
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-013-004 — "build" + "OAuth" + "token" → IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION

**Input:**
"Build the Stripe Connect OAuth connector, implement the token exchange flow, and secure the connected account credentials."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7: OAuth fires AUTHORIZATION)
- "token" → SECURITY (Rule 6)
- "secure" → SECURITY (reinforces; fires once)
- Integration vocabulary: connector, OAuth, token → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-013-005 — "refactor" existing Stripe connector → IMPLEMENT + API_CONTRACT + refactoring TransitionType

**Input:**
"Refactor the Stripe payment connector to use the new Stripe API v2024-06-20 webhook schema."

**Compiler analysis:**
- "Refactor" → IMPLEMENT (Rule 14: refactor fires IMPLEMENT AND sets TransitionType: refactoring)
- "connector" → API_CONTRACT
- "webhook" → API_CONTRACT (reinforces)
- "API" → API_CONTRACT (reinforces)
- "schema" → API_CONTRACT context
- Integration vocabulary: connector, webhook, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3, 3.3
**TransitionType:** refactoring (not feature_addition — "refactor" verb is present)
**DomainId:** software-development

**Teaching point:** "refactor" fires IMPLEMENT and changes TransitionType to refactoring. All other predicate rules remain the same.
