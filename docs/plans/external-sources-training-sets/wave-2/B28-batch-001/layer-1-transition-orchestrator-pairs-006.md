# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B28-transition-006-001 — QA+IMPLEMENT: "build" verb → feature_addition

A: ["QA","IMPLEMENT"] | "build the QA harness" | feature_addition

## B28-transition-006-002 — QA+IMPLEMENT: "write" verb → feature_addition

A: ["QA","IMPLEMENT"] | "write the QA automation suite" | feature_addition

## B28-transition-006-003 — QA+TEST: "write tests" → documentation (TEST without IMPLEMENT)

A: ["QA","TEST"] | "write smoke tests + QA sign-off" | documentation (TEST = writing tests, not IMPLEMENT)
  Teaching: "write tests" fires TEST, not IMPLEMENT. IMPLEMENT fires on writing production code.

## B28-transition-006-004 — QA+IMPLEMENT+TEST: "implement tests" → feature_addition

A: ["QA","IMPLEMENT","TEST"] | "implement the QA test suite" | feature_addition
  Teaching: "implement tests" fires both IMPLEMENT and TEST. IMPLEMENT drives feature_addition.

## B28-transition-006-005 — QA: TransitionType summary

  QA alone → no independent TransitionType
  QA + IMPLEMENT → feature_addition
  QA + TEST (no IMPLEMENT) → documentation
  QA + DOCUMENT → documentation
  QA + IMPLEMENT + anything → feature_addition

