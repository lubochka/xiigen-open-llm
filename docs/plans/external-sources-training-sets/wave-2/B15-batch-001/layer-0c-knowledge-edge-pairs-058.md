# Layer 0-C — Knowledge-Edge Pairs (058)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-058-001 — DeadLetterService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "dead letter domain model" → ARCHITECTURE
- "failure classification model" → ARCHITECTURE
- "bounded context" (dead letter bounded context) → ARCHITECTURE

---

## B15-kedge-058-002 — DeadLetterService: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "integrate with the DLQ API" → API_CONTRACT
- "wire up the dead letter queue binding" → API_CONTRACT
- "dead letter service contracts" → API_CONTRACT

---

## B15-kedge-058-003 — DeadLetterService: TEST trigger

"write integration tests for message capture and replay" → TEST.
"test dead letter queue routing on handler failure" → TEST.

---

## B15-kedge-058-004 — DeadLetterService: DOCUMENT trigger

"document the replay strategy and failure classification schema" → DOCUMENT.
"spec out the dead letter queue policy" → DOCUMENT.

---

## B15-kedge-058-005 — "capture" and "replay" as verbs fire IMPLEMENT (confirmed)

"capture" → IMPLEMENT. "replay" → IMPLEMENT (event replay context).
"Capture failed messages to the DLQ" → IMPLEMENT.
"Replay dead-letter messages for reprocessing" → IMPLEMENT.
Both: feature_addition TransitionType when no "refactor" present.
