# Layer 0-A — Compiler Pairs (007)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-007-001 — IMPLEMENT + API_CONTRACT + TEST

**Input:**
"Implement the SendGrid email integration connector and write integration tests for the send and bounce-webhook handlers."

**Compiler analysis:**
- "Implement" → IMPLEMENT (Rule 11)
- "integration" → API_CONTRACT (Rule 1: integration vocabulary fires API_CONTRACT)
- "connector" → API_CONTRACT (Rule 4; reinforces)
- "webhook" → API_CONTRACT (Rule 5; reinforces)
- "write integration tests" → TEST (Rule 13: "write tests" fires TEST)
- Integration vocabulary: "integration", "connector", "webhook" → IMPLEMENT routes to 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST"]
**SubPhase:** 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 5.1 (TEST in integration context)
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-007-002 — "write tests" vs "create tests" vs "add tests" — all fire TEST

**Input A:** "Write unit tests for the SendGrid connector."
→ TEST (Rule 13: "write tests" → TEST)

**Input B:** "Create integration tests for the bounce webhook handler."
→ TEST (Rule 13: "create tests" → TEST)

**Input C:** "Add tests for the email sending pipeline."
→ TEST (Rule 13: "add tests" → TEST)

**All three:** CompiledPredicateIds: ["TEST"] (if no other predicates present)
**SubPhase:** 5.1
**TransitionType:** no contribution (TEST alone, no IMPLEMENT)
**DomainId:** software-development

---

## B17-0A-007-003 — TEST with IMPLEMENT + API_CONTRACT in integration context

**Input:**
"Implement the SendGridEmailConnector and create tests for the transactional email send flow."

**Compiler analysis:**
- "Implement" → IMPLEMENT
- "connector" → API_CONTRACT
- "create tests" → TEST
- Integration vocabulary → 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST"]
**SubPhase:** 4.3, 3.3, 5.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-007-004 — "integration tests" phrase: "integration" fires API_CONTRACT AND "write tests" fires TEST

**Input:**
"Build the email connector and write integration tests for it."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT (from "connector")
- "integration" in "integration tests" → also API_CONTRACT signal (reinforces)
- "write integration tests" → TEST
- Three predicates fire: IMPLEMENT, API_CONTRACT, TEST

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST"]
**SubPhase:** 4.3, 3.3, 5.1
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** The word "integration" in "write integration tests" contributes to API_CONTRACT (integration context vocabulary) AND "write tests" triggers TEST. Both predicates fire from a single phrase.
