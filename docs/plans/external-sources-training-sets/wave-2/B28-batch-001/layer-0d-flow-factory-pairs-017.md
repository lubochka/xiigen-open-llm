# Layer 0-D — Flow Factory Pairs (017)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-017-001 — QA+ARCHITECTURE: infra release design + gate

A: ["QA", "ARCHITECTURE"] | [ReleaseGateFlow71, ArchitectureFlow31] | Two flows | documentation

## B28-factory-017-002 — QA+IMPLEMENT: infra gate service

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition

## B28-factory-017-003 — QA+ARCHITECTURE+IMPLEMENT: design + build + gate

A: ["QA", "ARCHITECTURE", "IMPLEMENT"] | [ReleaseGateFlow71, ArchitectureFlow31, ImplementationFlow41] | Three flows | feature_addition

## B28-factory-017-004 — QA+IMPLEMENT+DOCUMENT: IaC pipeline + criteria

A: ["QA", "IMPLEMENT", "DOCUMENT"] | [ReleaseGateFlow71, ImplementationFlow41, DocumentationFlow62] | Three flows | feature_addition

## B28-factory-017-005 — gap: infrastructure provisioning

A: [] | "provision K8s cluster" → GapFlow | knowledge_edge
