# Layer 0-D — Flow Factory Pairs (014)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-014-001 — QA+IMPLEMENT: canary controller + gate

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition

## B28-factory-014-002 — QA+IMPLEMENT+TEST: canary validation

A: ["QA", "IMPLEMENT", "TEST"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51] | Three flows | feature_addition

## B28-factory-014-003 — QA+DOCUMENT: canary criteria

A: ["QA", "DOCUMENT"] | [ReleaseGateFlow71, DocumentationFlow62] | Two flows | documentation

## B28-factory-014-004 — QA+IMPLEMENT+TEST+DOCUMENT: full canary

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | Four flows | feature_addition

## B28-factory-014-005 — gap: canary percentage

A: [] | "increase canary to 20%" → GapFlow | knowledge_edge
