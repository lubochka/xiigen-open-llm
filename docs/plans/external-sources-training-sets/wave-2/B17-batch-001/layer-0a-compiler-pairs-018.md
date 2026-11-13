# Layer 0-A — Compiler Pairs (018)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Zuora subscription billing connector

---

## B17-0A-018-001 — "build" + "connector" + "webhook" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Zuora subscription billing connector and implement the subscription create, amend, and renewal webhook handlers."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4)
- "webhook" → API_CONTRACT (Rule 5)
- Integration vocabulary: connector, webhook → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-018-002 — "integrate" Zuora → API_CONTRACT only

**Input:**
"Integrate the Zuora billing API to manage recurring subscription lifecycle events."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1)
- "API" → API_CONTRACT (reinforces)
- No build verb → IMPLEMENT does NOT fire

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

---

## B17-0A-018-003 — "build" + "OAuth" + "token" → IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION

**Input:**
"Build the Zuora REST connector, implement the OAuth 2.0 client credentials flow, and secure the bearer token for all billing API calls."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "REST" — reinforces integration context; not a standalone predicate trigger
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6: token fires SECURITY; fires once even though "bearer token" is another occurrence)
- "secure" → SECURITY (reinforces; fires once)
- Integration vocabulary: connector, OAuth, token, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-018-004 — "create" + "write tests" + "document" → IMPLEMENT + API_CONTRACT + TEST + DOCUMENT

**Input:**
"Create the Zuora subscription connector, write end-to-end tests for the billing cycle, and document the API contract for subscription management."

**Compiler analysis:**
- "Create" → IMPLEMENT
- "connector" → API_CONTRACT
- "write end-to-end tests" → TEST (Rule 13)
- "document" → DOCUMENT (Rule 12)
- "API" → API_CONTRACT (reinforces)
- Integration vocabulary: connector, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 5.1, 2.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-018-005 — "refactor" Zuora connector → IMPLEMENT + API_CONTRACT + refactoring

**Input:**
"Refactor the Zuora billing connector to support the Zuora API v1.3 schema changes and update the webhook handler contracts."

**Compiler analysis:**
- "Refactor" → IMPLEMENT (Rule 14: refactor fires IMPLEMENT + sets TransitionType: refactoring)
- "connector" → API_CONTRACT
- "API" → API_CONTRACT (reinforces)
- "webhook" → API_CONTRACT (reinforces)
- Integration vocabulary: connector, API, webhook → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3, 3.3
**TransitionType:** refactoring
**DomainId:** software-development
