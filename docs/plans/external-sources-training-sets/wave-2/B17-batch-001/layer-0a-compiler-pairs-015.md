# Layer 0-A — Compiler Pairs (015)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: FedEx/UPS shipping connector

---

## B17-0A-015-001 — "build" + "connector" + "endpoint" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the FedEx shipping connector and implement the rate quote, label generation, and tracking API endpoints."

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

## B17-0A-015-002 — "build" UPS connector → IMPLEMENT + API_CONTRACT

**Input:**
"Build the UPS shipping connector, wire up the package rate and shipment creation endpoints, and implement the webhook for tracking status updates."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4)
- "wire up" → API_CONTRACT (Rule 22)
- "endpoints" → API_CONTRACT (reinforces)
- "webhook" → API_CONTRACT (Rule 5)
- Integration vocabulary: connector, wire up, endpoints, webhook → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-015-003 — "integrate" FedEx → API_CONTRACT only

**Input:**
"Integrate the FedEx Ship Manager API to calculate shipping rates at checkout."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1: integrate fires API_CONTRACT ONLY)
- "API" → API_CONTRACT (reinforces)
- No build verb → IMPLEMENT does NOT fire

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

---

## B17-0A-015-004 — "implement" + "write tests" → IMPLEMENT + API_CONTRACT + TEST

**Input:**
"Implement the FedEx and UPS shipping connector adapters, write unit tests for rate calculation, and document the shipping API contract."

**Compiler analysis:**
- "Implement" → IMPLEMENT
- "connector" → API_CONTRACT (fires once for the compound "connector adapters")
- "API" → API_CONTRACT (reinforces)
- "write unit tests" → TEST (Rule 13)
- "document" → DOCUMENT (Rule 12)
- Integration vocabulary: connector, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 5.1, 2.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-015-005 — "create" + "token" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Create the FedEx OAuth connector, implement the API access token refresh logic, and secure the credential store."

**Compiler analysis:**
- "Create" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6)
- "secure" → SECURITY (reinforces; fires once)
- Integration vocabulary: connector, OAuth, token, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** "OAuth connector" — "connector" fires API_CONTRACT (Rule 4), "OAuth" fires AUTHORIZATION (Rule 7), "token" fires SECURITY (Rule 6). Three distinct predicates fire from a single five-word phrase.
