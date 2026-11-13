# Layer 0-B — Router Pairs (045) — ArtifactPublishing
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-045-001 — "publish artifact" → gap → knowledge_edge

A: [] | "publish release artifact to registry" → GapFlow | knowledge_edge
  Teaching: Artifact publishing vocabulary is gap. No flows.

## B28-router-045-002 — IMPLEMENT+QA for artifact publishing service

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA last gate.

## B28-router-045-003 — SECURITY+QA for artifact signing

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: SECURITY without IMPLEMENT → documentation. QA last.

## B28-router-045-004 — IMPLEMENT+DOCUMENT+QA for artifact service with criteria

A: ["IMPLEMENT", "DOCUMENT", "QA"] | [ImplementationFlow41, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-router-045-005 — Pure gap: "push Docker image to registry" → knowledge_edge

A: [] | "push Docker image to container registry and upload npm artifact" → GapFlow | knowledge_edge
  Teaching: Artifact upload/push vocabulary is gap.
