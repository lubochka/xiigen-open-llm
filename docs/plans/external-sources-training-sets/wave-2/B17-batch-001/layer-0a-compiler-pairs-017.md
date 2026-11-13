# Layer 0-A — Compiler Pairs (017)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Segment analytics connector

---

## B17-0A-017-001 — "build" + "connector" + "API" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Segment analytics connector and implement the track, identify, and group event API endpoints."

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

## B17-0A-017-002 — "integrate" Segment → API_CONTRACT only

**Input:**
"Integrate the Segment analytics API to capture page views and user actions."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1)
- "API" → API_CONTRACT (reinforces)
- No build verb → IMPLEMENT does NOT fire

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

---

## B17-0A-017-003 — "create" + "write tests" → IMPLEMENT + API_CONTRACT + TEST

**Input:**
"Create the Segment analytics connector, wire up the event sink API endpoints, and write integration tests for the track and identify calls."

**Compiler analysis:**
- "Create" → IMPLEMENT
- "connector" → API_CONTRACT
- "wire up" → API_CONTRACT (Rule 22; reinforces; fires once)
- "API" → API_CONTRACT (reinforces)
- "endpoints" → API_CONTRACT (reinforces)
- "write integration tests" → TEST (Rule 13)
- Integration vocabulary: connector, wire up, API, endpoints → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST"]
**SubPhase:** 4.3, 3.3, 5.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-017-004 — "implement" + "document" → IMPLEMENT + API_CONTRACT + DOCUMENT

**Input:**
"Implement the Segment connector for the data pipeline, and document the event schema and API contract for the analytics integration."

**Compiler analysis:**
- "Implement" → IMPLEMENT
- "connector" → API_CONTRACT
- "API" → API_CONTRACT (reinforces)
- "document" → DOCUMENT (Rule 12)
- "schema" → API_CONTRACT context (reinforces)
- Integration vocabulary: connector, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 2.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-017-005 — Negative: "add analytics tracking" without connector vocabulary → 4.1 not 4.3

**Input:**
"Add analytics tracking calls to the OrderService and implement the event batching logic."

**Compiler analysis:**
- "Add" — not in the standard IMPLEMENT lexicon (build, implement, write, create, develop, code)
- "implement" → IMPLEMENT
- "logic" — internal term (4.1 context)
- NO integration vocabulary: no connector, no webhook, no API, no external/third-party, no gateway
- IMPLEMENT routes to 4.1

**CompiledPredicateIds:** ["IMPLEMENT"]
**SubPhase:** 4.1
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** "analytics tracking" and "event batching" are internal service patterns without integration vocabulary. "Add" is not a standard IMPLEMENT trigger in this corpus — "implement" is the trigger here. Without integration vocabulary, IMPLEMENT routes to 4.1.
