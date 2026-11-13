# Layer 0-B — Router Pairs (046) — ChangelogGeneration
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-046-001 — DOCUMENT: "generate changelog" → DocumentationFlow62

A: ["DOCUMENT"] | [DocumentationFlow62] | documentation
  Teaching: "generate changelog" fires DOCUMENT → DocumentationFlow62. TransitionType: documentation.

## B28-router-046-002 — IMPLEMENT+DOCUMENT+QA for changelog generator service

A: ["IMPLEMENT", "DOCUMENT", "QA"] | [ImplementationFlow41, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-router-046-003 — DOCUMENT: "write release notes" → DocumentationFlow62

A: ["DOCUMENT"] | [DocumentationFlow62] | documentation
  Teaching: "release notes" is documentation artifact → DOCUMENT → DocumentationFlow62.

## B28-router-046-004 — TEST+DOCUMENT+QA for changelog with validation

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-router-046-005 — Pure gap: "pull commit log and format" → knowledge_edge

A: [] | "pull commit log and format for distribution" → GapFlow | knowledge_edge
  Teaching: "commit log" raw vocabulary is gap. Only "changelog" or "release notes" fire DOCUMENT.
