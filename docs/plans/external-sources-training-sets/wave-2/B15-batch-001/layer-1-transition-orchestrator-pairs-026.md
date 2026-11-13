# Layer 1 — Transition Orchestrator Pairs (026)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-026-001 — SubscriptionService: single IMPLEMENT transition

A: "Create a SubscriptionService that manages recurring billing plans."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | TransitionId: "transition-B15-t1-026-001-4.1-feature_addition"

---

## B15-t1-026-002 — SubscriptionService: four-predicate transition

A: "Create SubscriptionService + domain model + integrate billing provider + write integration tests."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "3.3", "4.1", "5.1"] | TransitionId: "transition-B15-t1-026-002-multi-feature_addition"

---

## B15-t1-026-003 — SubscriptionService: refactoring TransitionType

A: "Refactor the SubscriptionService to support tiered billing plans."
B: TransitionType: refactoring | SubPhaseIds: ["4.1"] | TransitionId: "transition-B15-t1-026-003-4.1-refactoring"

---

## B15-t1-026-004 — TransitionId format rule

Format: "transition-{batch_id}-t1-{file_num}-{run_id}-{sub_phases}-{transition_type}"
Single sub-phase: sub_phases = "4.1"
Multi sub-phase: sub_phases = "multi"
Examples: "transition-B15-t1-026-001-4.1-feature_addition", "transition-B15-t1-026-002-multi-feature_addition"

---

## B15-t1-026-005 — ContextCarryForward: true for all B15 transitions

ContextCarryForward: true is the constant for all B15 transitions.
Architecture outputs carry forward into API contract design; API contract definitions carry into implementation.
This value is never false in B15.
