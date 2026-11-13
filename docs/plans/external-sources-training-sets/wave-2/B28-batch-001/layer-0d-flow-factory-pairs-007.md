# Layer 0-D — Flow Factory Pairs (007)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-007-001 — QA+IMPLEMENT+TEST+DOCUMENT+SECURITY → 5 flows, feature_addition

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT", "SECURITY"] | 5 flows | feature_addition

## B28-factory-007-002 — QA: deduplication — multiple "QA" tokens → single QAGatingFlow73

A: Multiple QA tokens | One QAGatingFlow73 created | Teaching: Predicate set, not multiset.

## B28-factory-007-003 — QA sub-phase 7.1 (release pipeline gate) vs 7.3 (general gate)

A: ["QA"] with release/deploy vocabulary → ReleaseGateFlow71
  ["QA"] with general acceptance vocabulary → QAGatingFlow73
  Teaching: FlowFactory selects 7.1 or 7.3 based on vocabulary co-occurrence.

## B28-factory-007-004 — QA+IMPLEMENT+TEST: E2E variant

A: ["QA", "TEST"] with smoke/E2E context | [QAGatingFlow73, E2EFlow53] (not UnitTestingFlow51)
  Teaching: TEST sub-phase in factory selection: unit/regression → 5.1; smoke/E2E → 5.3.

## B28-factory-007-005 — gap → GapFlow; QA not invented

A: "Deploy the service." | [] | FlowFactory.CreateGap() — not QAGatingFlow73
  v6-02 corruption: FlowFactory.Create("QA") erroneously for deployment commands → incorrect

