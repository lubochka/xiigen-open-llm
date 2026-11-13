# Layer 0-A — Compiler Pairs (020)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Intercom customer messaging connector

---

## B17-0A-020-001 — "build" + "connector" + "webhook" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Intercom customer messaging connector and implement the conversation created, message replied, and user event webhook handlers."

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

## B17-0A-020-002 — "integrate" Intercom → API_CONTRACT only

**Input:**
"Integrate the Intercom messaging API to push support ticket updates to the customer chat widget."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1)
- "API" → API_CONTRACT (reinforces)
- No build verb → IMPLEMENT does NOT fire

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

---

## B17-0A-020-003 — "write" + "endpoint" + "token" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Write the Intercom REST connector, implement the message send endpoint, and include bearer token authentication for all outbound API calls."

**Compiler analysis:**
- "Write" → IMPLEMENT (Rule 11)
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "endpoint" → API_CONTRACT (reinforces)
- "API" → API_CONTRACT (reinforces)
- "token" → SECURITY (Rule 6: "bearer token" fires SECURITY)
- "authentication" → SECURITY (Rule 8: reinforces; fires once)
- Integration vocabulary: connector, endpoint, API, token → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-020-004 — "implement" + "document" + "write tests" → IMPLEMENT + API_CONTRACT + TEST + DOCUMENT

**Input:**
"Implement the Intercom connector for customer lifecycle events, write unit tests for the conversation routing logic, and document the inbound webhook schema."

**Compiler analysis:**
- "Implement" → IMPLEMENT
- "connector" → API_CONTRACT
- "webhook" → API_CONTRACT (reinforces)
- "write unit tests" → TEST (Rule 13)
- "document" → DOCUMENT (Rule 12)
- "schema" — reinforces API_CONTRACT context
- Integration vocabulary: connector, webhook → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 5.1, 2.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-020-005 — "build" + "token" + "validate" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Build the Intercom webhook connector and implement signature validation to authenticate inbound payloads using the Intercom shared secret."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "webhook" → API_CONTRACT (reinforces)
- "signature validation" → SECURITY (Rule 8: validate signature fires SECURITY)
- "authenticate" → SECURITY (Rule 8: authentication/authenticate fires SECURITY)
- "secret" — proximity to SECURITY but not a direct trigger; SECURITY already fires from "validate signature" and "authenticate"
- Integration vocabulary: connector, webhook → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development
