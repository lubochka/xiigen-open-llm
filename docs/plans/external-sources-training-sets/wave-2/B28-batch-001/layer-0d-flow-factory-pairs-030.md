# Layer 0-D — Flow Factory Pairs (030) — SecurityScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-030-001 — SECURITY alone → SecurityFlow6x → documentation

A: ["SECURITY"] | [SecurityFlow6x] | documentation
  Teaching: SECURITY alone (no IMPLEMENT) → documentation.

## B28-factory-030-002 — SECURITY+QA → SecurityFlow6x + ReleaseGateFlow71 → documentation

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation. QA adds gate.

## B28-factory-030-003 — SECURITY+IMPLEMENT → SecurityFlow6x + ImplementationFlow41 → feature_addition

A: ["SECURITY", "IMPLEMENT"] | [SecurityFlow6x, ImplementationFlow41] | feature_addition
  Teaching: IMPLEMENT drives feature_addition over SECURITY.

## B28-factory-030-004 — DOCUMENT+QA → DocumentationFlow62 + ReleaseGateFlow71 → documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: "document security criteria + QA sign-off" → documentation.

## B28-factory-030-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "trigger SecurityScanGate on each commit" → gap → knowledge_edge.
