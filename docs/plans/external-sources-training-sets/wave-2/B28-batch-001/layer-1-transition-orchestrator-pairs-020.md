# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-020-001 — A/B testing: QA alone — no transition contribution

A: ["QA"] only → ReleaseGateFlow71 only → no TransitionType
  Teaching: QA alone gates the A/B testing release. No TransitionType contributed.

## B28-transition-020-002 — A/B testing: QA+IMPLEMENT+DOCUMENT orchestration

A: (IMPLEMENT ‖ DOCUMENT) → QA gate
  Teaching: Experiment framework and criteria documentation may run in parallel. QA is final gate.

## B28-transition-020-003 — A/B testing: QA+TEST — documentation TransitionType

A: TEST only (from "verify variant assignment") → documentation | QA gate at end
  Teaching: TEST alone (no IMPLEMENT) → documentation. QA is final gate.

## B28-transition-020-004 — TransitionType: feature_addition for QA+IMPLEMENT

A: IMPLEMENT present → feature_addition (A/B experiment service is a feature)
  Teaching: Building an experiment assignment service fires IMPLEMENT → feature_addition.

## B28-transition-020-005 — Gap: A/B configuration → no transition

A: [] → "configure A/B split 50/50" → no flow → knowledge_edge
  Teaching: A/B test configuration is gap.
