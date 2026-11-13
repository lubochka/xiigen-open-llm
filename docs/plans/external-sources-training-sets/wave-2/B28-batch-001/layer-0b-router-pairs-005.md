# Layer 0-B — Router Pairs (005)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-005-001 — QA: TransitionType not set by QA alone

A: ["QA"] | B: 7.3 | TransitionType: determined by IMPLEMENT (feature_addition) or design verbs (documentation). QA alone does not set TransitionType.

## B28-router-005-002 — QA+IMPLEMENT → feature_addition

A: ["QA", "IMPLEMENT"] | TransitionType: feature_addition (IMPLEMENT present).

## B28-router-005-003 — QA+TEST (no IMPLEMENT) → documentation

A: ["QA", "TEST"] | TransitionType: documentation (no IMPLEMENT). TEST without IMPLEMENT → documentation.

## B28-router-005-004 — QA+DOCUMENT → documentation

A: ["QA", "DOCUMENT"] | TransitionType: documentation.

## B28-router-005-005 — v6-03: QA suppressed by TEST

A: ["QA", "TEST"] | Corrupt: [UnitTestingFlow51] only (QA suppressed) → v6-03
  Correct: [QAGatingFlow73, UnitTestingFlow51] | Teaching: QA is never suppressed by TEST.

