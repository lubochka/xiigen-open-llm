# Layer 0-B — Router Pairs (047) — TaggingStrategy
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-047-001 — "git tag" → gap → knowledge_edge

A: [] | "tag the release with v2.5.0 and push the git tag" → GapFlow | knowledge_edge
  Teaching: Git tagging vocabulary is gap. No flows.

## B28-router-047-002 — DOCUMENT+QA for tagging strategy criteria

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-router-047-003 — IMPLEMENT+DOCUMENT for automated tagger service

A: ["IMPLEMENT", "DOCUMENT"] | [ImplementationFlow41, DocumentationFlow62] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. No QA = no ReleaseGateFlow71.

## B28-router-047-004 — IMPLEMENT+TEST+DOCUMENT+QA: four flows for tagging framework

A: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] | [ImplementationFlow41, TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Four flows. IMPLEMENT drives feature_addition. QA absolute last.

## B28-router-047-005 — Pure gap: "semver tagging policy" → knowledge_edge

A: [] | "apply semver tagging and enforce versioning policy" → GapFlow | knowledge_edge
  Teaching: Versioning policy configuration vocabulary is gap.
