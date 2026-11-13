# Layer 0-D — Flow Factory Pairs (039) — ConditionalStageGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-039-001 — Gap: "conditional stage gate" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Conditional pipeline vocabulary is gap. GapFlow only.

## B28-factory-039-002 — IMPLEMENT+TEST+QA → three flows, feature_addition

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition.

## B28-factory-039-003 — SECURITY+QA → two flows, documentation

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: SECURITY without IMPLEMENT → documentation. QA last.

## B28-factory-039-004 — ARCHITECTURE+IMPLEMENT+DOCUMENT+QA → four flows, feature_addition

A: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT", "QA"] | [ArchitectureFlow31, ImplementationFlow41, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Four flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-039-005 — Gap: "if-branch fallback configuration" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Branch logic configuration vocabulary is gap.
