# Layer 0-D — Flow Factory Pairs (035) — ContainerScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-035-001 — SECURITY alone → SecurityFlow6x → documentation

A: ["SECURITY"] | [SecurityFlow6x] | documentation
  Teaching: "pass ContainerScanGate" → SECURITY → documentation.

## B28-factory-035-002 — SECURITY+QA → SecurityFlow6x + ReleaseGateFlow71 → documentation

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-factory-035-003 — IMPLEMENT+SECURITY → ImplementationFlow41 + SecurityFlow6x → feature_addition

A: ["IMPLEMENT", "SECURITY"] | [ImplementationFlow41, SecurityFlow6x] | feature_addition
  Teaching: "build container + service + secure scan" → IMPLEMENT drives feature_addition.

## B28-factory-035-004 — DOCUMENT+QA → DocumentationFlow62 + ReleaseGateFlow71 → documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: "document scan criteria + QA approve" → documentation.

## B28-factory-035-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "push container to artifact registry" → gap → knowledge_edge.
