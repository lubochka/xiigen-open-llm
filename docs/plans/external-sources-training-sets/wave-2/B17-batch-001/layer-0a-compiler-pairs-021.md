# Layer 0-A — Compiler Pairs (021)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Square POS connector

---

## B17-0A-021-001 — "build" + "connector" + "API" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Square POS (Point of Sale) connector and implement the payment creation, refund, and catalog sync API endpoints."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4)
- "API" → API_CONTRACT (reinforces)
- "endpoints" → API_CONTRACT (reinforces)
- Integration vocabulary: connector, API, endpoints → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-021-002 — "build" + "OAuth" + "token" + "authenticate" → IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION

**Input:**
"Build the Square payment connector, implement the OAuth 2.0 merchant authorization flow, and validate the access token on every API request."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6: "access token" fires SECURITY)
- "validate" — validates access token → SECURITY (Rule 8)
- "API" → API_CONTRACT (reinforces)
- Integration vocabulary: connector, OAuth, token, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-021-003 — "integrate" Square → API_CONTRACT only

**Input:**
"Integrate the Square Payments API to support in-person card-present transactions at retail locations."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1)
- "API" → API_CONTRACT (reinforces)
- No build verb → IMPLEMENT does NOT fire

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

---

## B17-0A-021-004 — "build" + "webhook" + "signature" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Build the Square webhook connector for payment event notifications and implement HMAC-SHA256 signature validation to secure the incoming callbacks."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "webhook" → API_CONTRACT (reinforces; fires once)
- "implement" → IMPLEMENT (reinforces)
- "signature validation" → SECURITY (Rule 8)
- "secure" → SECURITY (reinforces; fires once)
- Integration vocabulary: connector, webhook → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-021-005 — "create" + "write tests" + "QA" → IMPLEMENT + API_CONTRACT + TEST + QA

**Input:**
"Create the Square POS connector for in-store payments, write unit tests for the payment and refund handlers, and run a QA pass on the end-to-end checkout flow."

**Compiler analysis:**
- "Create" → IMPLEMENT
- "connector" → API_CONTRACT
- "write unit tests" → TEST (Rule 13)
- "QA" → QA (Rule 21: "QA" abbreviation fires QA; always last in canonical order)
- Integration vocabulary: connector → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "QA"]
**SubPhase:** 4.3, 3.3, 5.1, 7.x
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** "QA" in "run a QA pass" fires the QA predicate because the abbreviation "QA" appears. QA is always last in canonical ordering (Rule 21). "end-to-end checkout flow" does not fire any additional predicate.
