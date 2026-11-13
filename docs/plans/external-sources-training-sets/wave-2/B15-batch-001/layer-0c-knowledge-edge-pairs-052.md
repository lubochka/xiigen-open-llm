# Layer 0-C — Knowledge-Edge Pairs (052)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-052-001 — CompensationService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "compensation domain model" → ARCHITECTURE
- "undo action model" → ARCHITECTURE
- "bounded context" (compensation bounded context) → ARCHITECTURE

---

## B15-kedge-052-002 — CompensationService: TEST trigger

"write tests for all defined undo actions" → TEST.
"test compensation chain execution order" → TEST.

---

## B15-kedge-052-003 — CompensationService: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "wire up the saga step rollback API" → API_CONTRACT
- "compensation service contracts" → API_CONTRACT

---

## B15-kedge-052-004 — "rollback" and "undo" are IMPLEMENT trigger verbs (confirmed)

"rollback" → IMPLEMENT. "undo" → IMPLEMENT.
"Rollback the payment step on saga failure" → IMPLEMENT.
"Undo the inventory reservation when order cancels" → IMPLEMENT.

---

## B15-kedge-052-005 — CompensationService refactoring — knowledge edge

"refactor" present → TransitionType: refactoring (overrides feature_addition).
ImplementationFlow41 runs backward-compatibility check in refactoring mode.
Knowledge edge: refactoring TransitionType does NOT change which predicates are emitted.
IMPLEMENT is still emitted; only the TransitionType metadata field changes.
