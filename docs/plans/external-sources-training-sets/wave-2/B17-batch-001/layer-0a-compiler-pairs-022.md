# Layer 0-A — Compiler Pairs (022)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Avalara tax connector

---

## B17-0A-022-001 — "build" + "connector" + "API" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Avalara tax connector and implement the tax calculation, commit, and void transaction API endpoints."

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

## B17-0A-022-002 — "integrate" Avalara → API_CONTRACT only

**Input:**
"Integrate the Avalara AvaTax API for real-time sales tax calculation at checkout."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1)
- "API" → API_CONTRACT (reinforces)
- No build verb → IMPLEMENT does NOT fire

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

---

## B17-0A-022-003 — "implement" + "token" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Implement the Avalara REST connector and include bearer token authentication with automatic credential token renewal."

**Compiler analysis:**
- "Implement" → IMPLEMENT
- "connector" → API_CONTRACT
- "REST" — not a standalone predicate trigger; reinforces integration context
- "token" → SECURITY (Rule 6: "bearer token" fires SECURITY; "credential token" fires again — predicate fires once)
- "authentication" → SECURITY (Rule 8: reinforces; fires once)
- Integration vocabulary: connector, token → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-022-004 — "write" + "document" → IMPLEMENT + API_CONTRACT + DOCUMENT

**Input:**
"Write the Avalara tax connector adapter and document the API integration contract for the ERP (Enterprise Resource Planning) system team."

**Compiler analysis:**
- "Write" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT
- "API" → API_CONTRACT (reinforces)
- "document" → DOCUMENT (Rule 12)
- Integration vocabulary: connector, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 2.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-022-005 — "develop" + "write tests" + "document" → IMPLEMENT + API_CONTRACT + TEST + DOCUMENT

**Input:**
"Develop the Avalara connector for the order processing pipeline, write integration tests for tax calculation and void scenarios, and document the API schema for the finance team."

**Compiler analysis:**
- "Develop" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT
- "API" → API_CONTRACT (reinforces)
- "write integration tests" → TEST (Rule 13)
- "document" → DOCUMENT (Rule 12)
- "schema" — reinforces API_CONTRACT context
- Integration vocabulary: connector, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 5.1, 2.1
**TransitionType:** feature_addition
**DomainId:** software-development
