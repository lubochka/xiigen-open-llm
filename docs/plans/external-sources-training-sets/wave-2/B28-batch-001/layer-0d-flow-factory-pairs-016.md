# Layer 0-D — Flow Factory Pairs (016)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-016-001 — QA+DOCUMENT: feature flag criteria

A: ["QA", "DOCUMENT"] | [ReleaseGateFlow71, DocumentationFlow62] | Two flows | documentation

## B28-factory-016-002 — QA+IMPLEMENT: flag service + gate

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition

## B28-factory-016-003 — QA+IMPLEMENT+TEST: flag evaluation validation

A: ["QA", "IMPLEMENT", "TEST"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51] | Three flows | feature_addition

## B28-factory-016-004 — QA alone: feature flag acceptance gate

A: ["QA"] | [ReleaseGateFlow71] | One flow | no TransitionType contribution

## B28-factory-016-005 — gap: flag toggle

A: [] | "enable dark-mode flag" → GapFlow | knowledge_edge
