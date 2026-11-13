# Layer 0-B — Router Pairs (040) — ApprovalGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-040-001 — "approval" alone → gap → knowledge_edge

A: [] | "wait for approval before production" → GapFlow | knowledge_edge
  Teaching: "approval" alone is gap. No flows. No TransitionType.

## B28-router-040-002 — IMPLEMENT+QA for approval gate service

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA adds ReleaseGateFlow71.

## B28-router-040-003 — AUTHORIZATION+QA for approval gate access control

A: ["AUTHORIZATION", "QA"] | [AuthorizationFlow, ReleaseGateFlow71] | documentation
  Teaching: AUTHORIZATION without IMPLEMENT → documentation. QA last.

## B28-router-040-004 — "approve" without "QA" prefix → gap → knowledge_edge

A: [] | "get sign-off approval from release manager" → GapFlow | knowledge_edge
  Teaching: "approve" without QA abbreviation prefix is gap.

## B28-router-040-005 — Pure gap: "manual approval gates" → knowledge_edge

A: [] | "set up manual approval gates and automated approval thresholds" → GapFlow | knowledge_edge
  Teaching: Approval gate configuration vocabulary is gap.
