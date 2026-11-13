# Layer 0-D — Flow Factory Pairs (015)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-015-001 — QA+IMPLEMENT: rollback service + gate

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition

## B28-factory-015-002 — QA+IMPLEMENT+TEST: rollback validation

A: ["QA", "IMPLEMENT", "TEST"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51] | Three flows | feature_addition

## B28-factory-015-003 — QA+DOCUMENT: rollback runbook

A: ["QA", "DOCUMENT"] | [ReleaseGateFlow71, DocumentationFlow62] | Two flows | documentation

## B28-factory-015-004 — QA+IMPLEMENT+TEST+DOCUMENT: full rollback lifecycle

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | Four flows | feature_addition

## B28-factory-015-005 — gap: rollback execution

A: [] | "roll back to v2.1" → GapFlow | knowledge_edge
