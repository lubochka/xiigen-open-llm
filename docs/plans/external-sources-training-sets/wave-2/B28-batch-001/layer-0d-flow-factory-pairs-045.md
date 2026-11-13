# Layer 0-D — Flow Factory Pairs (045) — ArtifactPublishing
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-045-001 — Gap: "publish artifact" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Artifact publishing vocabulary is gap. GapFlow only.

## B28-factory-045-002 — IMPLEMENT+QA → two flows, feature_addition

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA last gate.

## B28-factory-045-003 — SECURITY+QA → two flows, documentation

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: SECURITY without IMPLEMENT → documentation. QA last.

## B28-factory-045-004 — IMPLEMENT+DOCUMENT+QA → three flows, feature_addition

A: ["IMPLEMENT", "DOCUMENT", "QA"] | [ImplementationFlow41, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-045-005 — Gap: "push to registry, upload artifact" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: All artifact upload/push vocabulary is gap.
