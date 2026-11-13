# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B28-transition-005-001 — Six predicates → feature_addition, 6 flows sequential

A: ["QA","IMPLEMENT","TEST","SECURITY","DOCUMENT","ARCHITECTURE"] | feature_addition
  Sequence: ARCHITECTURE → SECURITY → IMPLEMENT → TEST → DOCUMENT → QA gate

## B28-transition-005-002 — QA: parallel execution for independent flows

A: ["QA","TEST","DOCUMENT"] | DOCUMENT ∥ TEST (independent, parallel) → QA gate (sequential last)

## B28-transition-005-003 — QA+IMPLEMENT+TEST: IMPLEMENT → TEST → QA (canonical)

A: ["QA","IMPLEMENT","TEST"] | Sequence: IMPLEMENT → TEST → QA gate | TransitionType: feature_addition

## B28-transition-005-004 — QA+TEST: TEST → QA (no IMPLEMENT)

A: ["QA","TEST"] | Sequence: TEST → QA gate | documentation

## B28-transition-005-005 — QA: never drives TransitionType independently

A: ["QA"] alone | TransitionType: not set | QA gate as single flow
  Teaching: QA itself does not drive TransitionType. TransitionType is set by IMPLEMENT (feature_addition) or design verbs (documentation) or refactor verbs (refactoring).

