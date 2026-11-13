# Layer 0-D — Flow Factory Pairs (005)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-005-001 — QA alone: documentation TransitionType

A: ["QA"] | [QAGatingFlow73] | TransitionType: not driven by QA alone; follows other predicates

## B28-factory-005-002 — QA+TEST: documentation (no IMPLEMENT)

A: ["QA", "TEST"] | [QAGatingFlow73, UnitTestingFlow51] | documentation TransitionType

## B28-factory-005-003 — QA+IMPLEMENT: feature_addition

A: ["QA", "IMPLEMENT"] | [QAGatingFlow73, ImplementationFlow41] | feature_addition

## B28-factory-005-004 — QA: smoke test (E2E) → sub-phase 5.3

A: ["QA", "TEST"] with smoke/E2E context | [QAGatingFlow73, E2EFlow53] | smoke tests route to 5.3

## B28-factory-005-005 — QA: sequential placement — QA gate always last

A: ["QA", "IMPLEMENT", "TEST"] | Sequence: ImplementationFlow41 → UnitTestingFlow51 → QAGatingFlow73 (last)
  Teaching: QA gating always last in sequential ordering.

