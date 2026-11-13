# Layer 1 — Transition Orchestrator Pairs (042) — RollbackTrigger
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-042-001 — IMPLEMENT+TEST: IMPLEMENT then TEST (no QA)

A: ["IMPLEMENT", "TEST"] | IMPLEMENT → TEST → done
  TransitionType: feature_addition
  Teaching: Rollback trigger service: IMPLEMENT then TEST. No QA gate absent QA predicate.

## B28-transition-042-002 — SECURITY+QA: SECURITY then QA

A: ["SECURITY", "QA"] | SECURITY → QA
  TransitionType: documentation
  Teaching: Secure rollback endpoint, then QA validates permission model.

## B28-transition-042-003 — IMPLEMENT+DOCUMENT+QA: parallel IMPLEMENT/DOCUMENT then QA

A: ["IMPLEMENT", "DOCUMENT", "QA"] | parallel(IMPLEMENT ‖ DOCUMENT) → QA
  TransitionType: feature_addition
  Teaching: IMPLEMENT and DOCUMENT can run in parallel. QA last.

## B28-transition-042-004 — Gap: "rollback policy configuration" → no orchestration

A: [] | "set rollback threshold and configure rollback policy" → no flows → no orchestration
  Teaching: Rollback configuration is gap.

## B28-transition-042-005 — FAIL for rollback trigger not activating on detected failure

A: ReleaseGateFlow71: FAIL if rollback trigger fails to fire on known failure condition
  Teaching: Rollback trigger failure to activate → FAIL. Not normalized.
