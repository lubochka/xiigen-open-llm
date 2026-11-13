# Layer 0-D — Flow Factory Pairs (002–012 combined summary)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-002-001 — QA alone → QAGatingFlow73

A: ["QA"] | FlowFactory.Create("QA") → QAGatingFlow73

## B28-factory-002-002 — QA+TEST → QAGatingFlow73, UnitTestingFlow51

A: ["QA", "TEST"] | FlowFactory.CreateMulti(["QA","TEST"]) → [QAGatingFlow73, UnitTestingFlow51]

## B28-factory-002-003 — QA+IMPLEMENT → QAGatingFlow73, ImplementationFlow41

A: ["QA", "IMPLEMENT"] | TransitionType: feature_addition

## B28-factory-002-004 — QA+DOCUMENT → QAGatingFlow73, DocumentationFlow62

A: ["QA", "DOCUMENT"] | TransitionType: documentation

## B28-factory-002-005 — gap → GapFlow

A: [] | FlowFactory.CreateGap() → GapFlow | SCHEDULE_CAPABILITY_LEARNING emitted

