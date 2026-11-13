# Layer 0-B — Router Pairs (044) — EnvironmentPromotion
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-044-001 — "promote artifact" → gap → knowledge_edge

A: [] | "promote the artifact from staging to production" → GapFlow | knowledge_edge
  Teaching: Environment promotion vocabulary is gap. No flows.

## B28-router-044-002 — IMPLEMENT+QA for promotion service

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA last gate.

## B28-router-044-003 — DOCUMENT+QA for promotion criteria

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-router-044-004 — ARCHITECTURE+IMPLEMENT+TEST+QA: four flows for promotion framework

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "QA"] | [ArchitectureFlow31, ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Four flows. IMPLEMENT drives feature_addition. QA last.

## B28-router-044-005 — Pure gap: "dev to staging to production" → knowledge_edge

A: [] | "move release through dev to staging to production" → GapFlow | knowledge_edge
  Teaching: Full promotion chain vocabulary is gap.
