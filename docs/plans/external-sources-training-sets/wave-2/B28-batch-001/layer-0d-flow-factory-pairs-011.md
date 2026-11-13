# Layer 0-D — Flow Factory Pairs (011)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-011-001 — QA+IMPLEMENT+TEST+SECURITY+DOCUMENT+ARCHITECTURE → 6 flows

A: ["QA","IMPLEMENT","TEST","SECURITY","DOCUMENT","ARCHITECTURE"] | 6 flows | feature_addition

## B28-factory-011-002 — QA: "quality assurance" → gap, no flow created

A: "quality assurance" spelled out | [] | GapFlow (not QAGatingFlow73)
  v6-02 corruption: QAGatingFlow73 created erroneously → incorrect

## B28-factory-011-003 — QA+TEST: E2E smoke test → E2EFlow53 (not UnitTestingFlow51)

A: ["QA","TEST"] with smoke/E2E | [QAGatingFlow73, E2EFlow53]

## B28-factory-011-004 — QA: TransitionType not set by QA predicate alone

A: ["QA"] | TransitionType: undefined until IMPLEMENT or design verb determines it

## B28-factory-011-005 — QA: v6-03 — QA suppression by any other predicate is incorrect

A: Any multi-predicate with QA | QAGatingFlow73 always created independently
  Teaching: QA is never suppressed by IMPLEMENT, TEST, SECURITY, or any other predicate.

