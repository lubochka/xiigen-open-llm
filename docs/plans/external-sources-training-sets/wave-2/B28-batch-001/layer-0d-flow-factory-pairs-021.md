# Layer 0-D — Flow Factory Pairs (021)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-021-001 — QA+ARCHITECTURE+IMPLEMENT: DR design + build + gate

A: ["QA", "ARCHITECTURE", "IMPLEMENT"] | [ReleaseGateFlow71, ArchitectureFlow31, ImplementationFlow41] | Three flows | feature_addition

## B28-factory-021-002 — QA+IMPLEMENT+TEST: failover trigger + RTO validation + gate

A: ["QA", "IMPLEMENT", "TEST"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51] | Three flows | feature_addition

## B28-factory-021-003 — QA+DOCUMENT: DR release criteria + runbook

A: ["QA", "DOCUMENT"] | [ReleaseGateFlow71, DocumentationFlow62] | Two flows | documentation

## B28-factory-021-004 — QA+ARCHITECTURE+IMPLEMENT+TEST+DOCUMENT: full DR

A: ["QA", "ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"] | [ReleaseGateFlow71, ArchitectureFlow31, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | Five flows | feature_addition

## B28-factory-021-005 — gap: DR failover execution

A: [] | "trigger failover to secondary region" → GapFlow | knowledge_edge
