# Layer 0-A — Compiler Pairs (016)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: HubSpot CRM connector

---

## B17-0A-016-001 — "build" + "connector" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the HubSpot CRM (Customer Relationship Management) connector and implement the contact sync and deal pipeline API endpoints."

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

## B17-0A-016-002 — "build" + "token" + "authenticate" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Build the HubSpot private app connector, implement the OAuth token refresh, and authenticate all API requests with bearer token validation."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6: fires twice — "token refresh" and "bearer token" — but predicate fires once)
- "authenticate" → SECURITY (Rule 8: authentication fires SECURITY)
- Integration vocabulary: connector, OAuth, token, API → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-016-003 — "integrate" HubSpot → API_CONTRACT only

**Input:**
"Integrate HubSpot CRM with the lead management workflow for automatic contact creation."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1)
- No build verb → IMPLEMENT does NOT fire
- "CRM" — acronym for Customer Relationship Management — not a predicate trigger
- "workflow" — internal term, no predicate

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

---

## B17-0A-016-004 — "create" + "webhook" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Create the HubSpot webhook connector for deal stage change events and implement request signature validation to secure the inbound payloads."

**Compiler analysis:**
- "Create" → IMPLEMENT
- "connector" → API_CONTRACT
- "webhook" → API_CONTRACT (reinforces; fires once)
- "implement" → IMPLEMENT (reinforces)
- "signature validation" → SECURITY (Rule 8: validate signature fires SECURITY)
- "secure" → SECURITY (reinforces; fires once)
- Integration vocabulary: connector, webhook → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-016-005 — "develop" + "document" + "write tests" → IMPLEMENT + API_CONTRACT + TEST + DOCUMENT

**Input:**
"Develop the HubSpot contact and deal API connector, document the integration contract, and write unit tests for each endpoint handler."

**Compiler analysis:**
- "Develop" → IMPLEMENT (Rule 11: develop fires IMPLEMENT)
- "connector" → API_CONTRACT
- "API" → API_CONTRACT (reinforces)
- "document" → DOCUMENT (Rule 12)
- "write unit tests" → TEST (Rule 13)
- "endpoint" → API_CONTRACT (reinforces)
- Integration vocabulary: connector, API, endpoint → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 5.1, 2.1
**TransitionType:** feature_addition
**DomainId:** software-development
