# Layer 0-D — Flow Factory Pairs (040) — ApprovalGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-040-001 — Gap: "approval" alone → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: "approval" alone is gap. GapFlow only.

## B28-factory-040-002 — IMPLEMENT+QA → two flows, feature_addition

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA adds ReleaseGateFlow71 as last gate.

## B28-factory-040-003 — AUTHORIZATION+QA → two flows, documentation

A: ["AUTHORIZATION", "QA"] | [AuthorizationFlow, ReleaseGateFlow71] | documentation
  Teaching: AUTHORIZATION without IMPLEMENT → documentation. QA last.

## B28-factory-040-004 — Gap: "sign-off approval" (no QA prefix) → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: "approve" without QA prefix is gap. GapFlow only.

## B28-factory-040-005 — Gap: "manual approval gates configuration" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Approval gate configuration vocabulary is gap.
