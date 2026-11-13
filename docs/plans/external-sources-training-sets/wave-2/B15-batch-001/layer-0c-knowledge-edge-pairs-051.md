# Layer 0-C — Knowledge-Edge Pairs (051)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-051-001 — SagaOrchestrator: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "saga domain model" → ARCHITECTURE
- "compensation choreography model" → ARCHITECTURE
- "bounded context" (distributed transaction bounded context) → ARCHITECTURE
- "architect the saga state machine" → ARCHITECTURE

---

## B15-kedge-051-002 — SagaOrchestrator: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "define the step coordination API contracts" → API_CONTRACT (+ DOCUMENT from "define")
- "saga API contracts" → API_CONTRACT
- "wire up the step execution API" → API_CONTRACT

---

## B15-kedge-051-003 — SagaOrchestrator: TEST trigger

"write tests for rollback and compensation scenarios" → TEST.
"test saga step failure and recovery" → TEST.

---

## B15-kedge-051-004 — SagaOrchestrator: DOCUMENT trigger

"document the compensation policy" → DOCUMENT.
"spec out the saga step definitions" → DOCUMENT.

---

## B15-kedge-051-005 — "orchestrate" and "coordinate" are IMPLEMENT trigger verbs

"orchestrate" → IMPLEMENT. "coordinate" → IMPLEMENT (distributed coordination context).
"Orchestrate the checkout distributed transaction" → IMPLEMENT.
"Coordinate saga step execution and compensation" → IMPLEMENT.
