# Layer 0-D — Flow Factory Pairs (019)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-019-001 — QA+IMPLEMENT+DOCUMENT: coordinator + dependency map + gate

A: ["QA", "IMPLEMENT", "DOCUMENT"] | [ReleaseGateFlow71, ImplementationFlow41, DocumentationFlow62] | Three flows | feature_addition

## B28-factory-019-002 — QA+IMPLEMENT+TEST: orchestration + integration tests + gate

A: ["QA", "IMPLEMENT", "TEST"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51] | Three flows | feature_addition

## B28-factory-019-003 — QA+DOCUMENT: release criteria + QA checklist

A: ["QA", "DOCUMENT"] | [ReleaseGateFlow71, DocumentationFlow62] | Two flows | documentation

## B28-factory-019-004 — QA+IMPLEMENT+TEST+DOCUMENT: full multi-service lifecycle

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | Four flows | feature_addition

## B28-factory-019-005 — gap: deployment sequencing instruction

A: [] | "deploy UserService then OrderService in sequence" → GapFlow | knowledge_edge
