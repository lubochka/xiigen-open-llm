# Layer 0-B — Router Pairs (042) — RollbackTrigger
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-042-001 — "rollback" alone → gap → knowledge_edge

A: [] | "trigger a rollback if health check fails" → GapFlow | knowledge_edge
  Teaching: "rollback", "trigger", "health check" are all gap. No flows.

## B28-router-042-002 — IMPLEMENT+TEST for rollback trigger service

A: ["IMPLEMENT", "TEST"] | [ImplementationFlow41, TestingFlow] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. TEST → TestingFlow.

## B28-router-042-003 — SECURITY+QA for rollback authorization

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: SECURITY without IMPLEMENT → documentation. QA last.

## B28-router-042-004 — IMPLEMENT+DOCUMENT+QA for rollback service with criteria

A: ["IMPLEMENT", "DOCUMENT", "QA"] | [ImplementationFlow41, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-router-042-005 — Pure gap: "rollback threshold and policy" → knowledge_edge

A: [] | "set rollback threshold and configure rollback policy" → GapFlow | knowledge_edge
  Teaching: Rollback configuration vocabulary is gap.
