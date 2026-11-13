# Layer 0-B — Router Pairs (024) — Complete Predicate Router Reference for 7.1
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-024-001 — "service" → IMPLEMENT → routes to 4.1

A: ["QA", "IMPLEMENT"] | "review the deployment service + QA" | [ReleaseGateFlow71, ImplementationFlow41]
  Teaching: "service" routes to ImplementationFlow41 (sub-phase 4.1) regardless of context modifier.

## B28-router-024-002 — "criteria" → DOCUMENT → routes to DocumentationFlow62

A: ["QA", "DOCUMENT"] | "release criteria + QA sign-off" | [ReleaseGateFlow71, DocumentationFlow62]
  Teaching: "criteria" routes to DocumentationFlow62 regardless of what precedes it.

## B28-router-024-003 — "deployment" alone → gap → knowledge_edge

A: [] | "plan the deployment" → GapFlow | knowledge_edge
  Teaching: "deployment" alone never routes to any flow. Pure gap → knowledge_edge (not SCHEDULE_CAPABILITY_LEARNING).

## B28-router-024-004 — QA alone: no contribution flow — gate-only

A: ["QA"] | ReleaseGateFlow71 only | no TransitionType | no contribution TransitionType from QA alone
  Teaching: QA alone → ReleaseGateFlow71; QA does not drive TransitionType.

## B28-router-024-005 — IMPLEMENT+QA: feature_addition driven by IMPLEMENT

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | feature_addition
  Teaching: TransitionType: feature_addition is driven by IMPLEMENT. QA never overrides TransitionType.
