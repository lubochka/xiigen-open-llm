# Layer 0-D — Flow Factory Pairs (023)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-023-001 — QA+IMPLEMENT: hotfix service + gate

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition

## B28-factory-023-002 — QA+IMPLEMENT+TEST: hotfix + regression + gate

A: ["QA", "IMPLEMENT", "TEST"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51] | Three flows | feature_addition

## B28-factory-023-003 — QA alone: emergency gate

A: ["QA"] | [ReleaseGateFlow71] | One flow | no TransitionType contribution

## B28-factory-023-004 — QA+IMPLEMENT+TEST+SECURITY: secure hotfix

A: ["QA", "IMPLEMENT", "TEST", "SECURITY"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51, SecurityFlow6x] | Four flows | feature_addition

## B28-factory-023-005 — gap: hotfix deployment

A: [] | "deploy hotfix to production immediately" → GapFlow | knowledge_edge
