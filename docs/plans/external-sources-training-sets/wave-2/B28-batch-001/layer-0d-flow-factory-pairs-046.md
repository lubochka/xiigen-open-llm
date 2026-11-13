# Layer 0-D — Flow Factory Pairs (046) — ChangelogGeneration
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-046-001 — DOCUMENT: "generate changelog" → DocumentationFlow62, documentation

A: ["DOCUMENT"] | [DocumentationFlow62] | documentation
  Teaching: "changelog" fires DOCUMENT → DocumentationFlow62. One flow. TransitionType: documentation.

## B28-factory-046-002 — IMPLEMENT+DOCUMENT+QA → three flows, feature_addition

A: ["IMPLEMENT", "DOCUMENT", "QA"] | [ImplementationFlow41, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-046-003 — DOCUMENT: "release notes" → DocumentationFlow62, documentation

A: ["DOCUMENT"] | [DocumentationFlow62] | documentation
  Teaching: "release notes" fires DOCUMENT → DocumentationFlow62.

## B28-factory-046-004 — TEST+DOCUMENT+QA → three flows, documentation

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-046-005 — Gap: "commit log formatting" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: "commit log" raw vocabulary is gap.
