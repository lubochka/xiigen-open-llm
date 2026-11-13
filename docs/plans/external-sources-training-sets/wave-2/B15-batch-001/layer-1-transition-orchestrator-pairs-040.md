# Layer 1 — Transition Orchestrator Pairs (040)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-040-001 — SagaOrchestrator: IMPLEMENT single transition

A: "Build a SagaOrchestrator to coordinate distributed transactions."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true

---

## B15-t1-040-002 — SagaOrchestrator: IMPLEMENT+ARCHITECTURE transition

A: "Build SagaOrchestrator with a saga domain model defining compensation choreography."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "4.1"] | ContextCarryForward: true

---

## B15-t1-040-003 — SagaOrchestrator: IMPLEMENT+API_CONTRACT transition

A: "Build SagaOrchestrator and define the step coordination API contracts."
B: TransitionType: feature_addition | SubPhaseIds: ["3.3", "4.1", "2.1"] | ContextCarryForward: true
"define contracts" → API_CONTRACT + DOCUMENT → "3.3" and "2.1" both in SubPhaseIds.

---

## B15-t1-040-004 — SagaOrchestrator: IMPLEMENT+TEST transition

A: "Build SagaOrchestrator and write tests for rollback and compensation scenarios."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1", "5.1"] | ContextCarryForward: true

---

## B15-t1-040-005 — SagaOrchestrator: full five-phase transition

A: "Build SagaOrchestrator with a saga domain model, define step coordination contracts, write rollback tests, and document the compensation policy."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "3.3", "4.1", "5.1", "2.1"] | ContextCarryForward: true
Note: "2.1" appears twice in predicate set (from "define contracts" + "document") — deduplicated to one "2.1" entry.
