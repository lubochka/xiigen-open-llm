# Layer 0-D — Flow Factory Pairs (047) — TaggingStrategy
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-047-001 — Gap: "git tag / semver" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Git tagging vocabulary is gap. GapFlow only.

## B28-factory-047-002 — DOCUMENT+QA → two flows, documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-047-003 — IMPLEMENT+DOCUMENT → two flows, feature_addition

A: ["IMPLEMENT", "DOCUMENT"] | [ImplementationFlow41, DocumentationFlow62] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. No QA = no ReleaseGateFlow71.

## B28-factory-047-004 — IMPLEMENT+TEST+DOCUMENT+QA → four flows, feature_addition

A: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] | [ImplementationFlow41, TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Four flows. IMPLEMENT drives feature_addition. QA absolute last.

## B28-factory-047-005 — Gap: "versioning policy enforcement" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Versioning policy configuration vocabulary is gap.
