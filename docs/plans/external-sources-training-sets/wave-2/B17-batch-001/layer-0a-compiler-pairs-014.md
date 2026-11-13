# Layer 0-A — Compiler Pairs (014)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Mailgun email provider connector

---

## B17-0A-014-001 — "build" + "webhook" + "connector" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Mailgun email connector and implement the delivery, bounce, and unsubscribe webhook handlers."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4)
- "webhook" → API_CONTRACT (Rule 5: webhook fires API_CONTRACT)
- Integration vocabulary: connector, webhook → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-014-002 — "integrate Mailgun" alone → API_CONTRACT only

**Input:**
"Integrate Mailgun into the notification service for transactional email delivery."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1: integrate fires API_CONTRACT ONLY)
- No build verb present → IMPLEMENT does NOT fire
- "notification service" — no predicate (internal service noun)
- Result: API_CONTRACT only

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

**Teaching point:** "Mailgun" is a proper noun with no predicate assignment. "service" in "notification service" is an internal service term — it does not add integration vocabulary in this context because no build verb is present.

---

## B17-0A-014-003 — "write" + "API" + "schema" → IMPLEMENT + API_CONTRACT + DOCUMENT

**Input:**
"Write the Mailgun email sending connector, implement the API client, and document the request schema for transactional templates."

**Compiler analysis:**
- "Write" → IMPLEMENT (Rule 11: write fires IMPLEMENT)
- "implement" → IMPLEMENT (reinforces)
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

## B17-0A-014-004 — "create" + "write tests" → IMPLEMENT + API_CONTRACT + TEST

**Input:**
"Create the Mailgun webhook connector, write integration tests for delivery event handling, and validate the HMAC signature."

**Compiler analysis:**
- "Create" → IMPLEMENT (Rule 11: create fires IMPLEMENT)
- "connector" → API_CONTRACT
- "webhook" → API_CONTRACT (reinforces)
- "write integration tests" → TEST (Rule 13: write tests fires TEST)
- "validate" — not a SECURITY trigger alone in this context (no "signature" by itself — "validate the HMAC signature" does include "signature" which is a SECURITY-adjacent term, but the primary SECURITY trigger is "token")
- Note: "HMAC signature" validation → SECURITY fires on "secure" / "validate signature" per Rule 8

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"]
**SubPhase:** 4.3, 3.3, 3.4, 5.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-014-005 — Negative: "build email sender" without connector vocabulary → 4.1 not 4.3

**Input:**
"Build the EmailSenderService and implement the send and retry logic."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "logic" — internal logic (4.1 context)
- NO integration vocabulary: no connector, no webhook, no API, no external/third-party
- Result: IMPLEMENT routes to 4.1

**CompiledPredicateIds:** ["IMPLEMENT"]
**SubPhase:** 4.1 (IMPLEMENT without integration vocabulary → 4.1, not 4.3)
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** Adding "Mailgun" as a proper noun does NOT automatically supply integration vocabulary. The vocabulary trigger must be an integration-context term: connector, webhook, API, external service, third-party, gateway, SDK, or endpoint.
