# Layer 0-A — Compiler Pairs (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-004-001 — IMPLEMENT + API_CONTRACT + SECURITY: webhook signature

**Input:**
"Build the GitHub webhook listener and implement signature validation to secure incoming payloads."

**Compiler analysis:**
- "Build" → IMPLEMENT (Rule 11)
- "implement" → IMPLEMENT (reinforces)
- "webhook" → API_CONTRACT (Rule 5)
- "signature validation" → SECURITY (Rule 8: "validation" in security context)
- "secure" → SECURITY (Rule 8: "secure" fires SECURITY)
- Integration vocabulary: "webhook", "listener" → IMPLEMENT routes to 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 3.4 (SECURITY)
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-004-002 — "webhook" fires API_CONTRACT regardless of position

**Input:**
"Implement the webhook handler for GitHub push events."

**Compiler analysis:**
- "Implement" → IMPLEMENT
- "webhook" → API_CONTRACT (Rule 5: webhook always fires API_CONTRACT)
- Integration vocabulary: "webhook" → IMPLEMENT routes to 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-004-003 — "secure" + "validate signature" = two SECURITY triggers

**Input:**
"Build the GitHub webhook endpoint, validate the HMAC signature, and secure the payload processing pipeline."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "webhook" → API_CONTRACT
- "validate the HMAC signature" → SECURITY (signature validation is SECURITY pattern)
- "secure" → SECURITY (reinforces; SECURITY fires once even with two triggers)
- Integration vocabulary present → 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 3.4 (SECURITY)
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** Multiple SECURITY-triggering words still fire SECURITY once (predicates don't stack).

---

## B17-0A-004-004 — "authentication" fires SECURITY (not AUTHORIZATION)

**Input:**
"Build the GitHub webhook listener and implement authentication for the incoming requests."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "webhook" → API_CONTRACT
- "authentication" → SECURITY (Rule 8: authentication fires SECURITY)
- "authentication" does NOT fire AUTHORIZATION (AUTHORIZATION fires on "oauth" only — Rule 7)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** "authentication" → SECURITY. Only "oauth" (case-insensitive) → AUTHORIZATION. Do not conflate.
