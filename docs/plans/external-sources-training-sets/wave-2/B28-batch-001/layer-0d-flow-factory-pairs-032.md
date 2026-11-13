# Layer 0-D — Flow Factory Pairs (032) — LicenseComplianceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-032-001 — SECURITY alone → SecurityFlow6x → documentation

A: ["SECURITY"] | [SecurityFlow6x] | documentation
  Teaching: "run LicenseComplianceGate" → SECURITY → documentation.

## B28-factory-032-002 — SECURITY+QA → SecurityFlow6x + ReleaseGateFlow71 → documentation

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-factory-032-003 — DOCUMENT+IMPLEMENT → DocumentationFlow62 + ImplementationFlow41 → feature_addition

A: ["DOCUMENT", "IMPLEMENT"] | [DocumentationFlow62, ImplementationFlow41] | feature_addition
  Teaching: IMPLEMENT drives feature_addition.

## B28-factory-032-004 — SECURITY+TEST → SecurityFlow6x + TestingFlow → documentation

A: ["SECURITY", "TEST"] | [SecurityFlow6x, TestingFlow] | documentation
  Teaching: "secure repo + validate license" → No IMPLEMENT → documentation.

## B28-factory-032-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "check all open-source licenses in bundle" → gap → knowledge_edge.
