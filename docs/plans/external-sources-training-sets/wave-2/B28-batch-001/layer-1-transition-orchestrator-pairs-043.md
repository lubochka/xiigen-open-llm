# Layer 1 — Transition Orchestrator Pairs (043) — HotfixPipeline
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-043-001 — IMPLEMENT+QA for hotfix implementation

A: ["IMPLEMENT", "QA"] | IMPLEMENT → QA
  TransitionType: feature_addition
  Teaching: Implement the hotfix, then QA gate validates before deployment.

## B28-transition-043-002 — IMPLEMENT+TEST+QA for hotfix with regression

A: ["IMPLEMENT", "TEST", "QA"] | IMPLEMENT → TEST → QA
  TransitionType: feature_addition
  Teaching: Implement hotfix, add regression tests, QA gate validates. Strictly sequential.

## B28-transition-043-003 — SECURITY+TEST+QA for security hotfix

A: ["SECURITY", "TEST", "QA"] | parallel(SECURITY ‖ TEST) → QA
  TransitionType: documentation
  Teaching: SECURITY and TEST in parallel for security hotfix. QA last.

## B28-transition-043-004 — Gap: "cut hotfix branch" → no orchestration

A: [] | "cut a hotfix branch and push to production" → no flows → no orchestration
  Teaching: Hotfix branch operations are gap.

## B28-transition-043-005 — FAIL for hotfix introducing new regression

A: ReleaseGateFlow71: FAIL if hotfix introduces new regression in previously passing tests
  Teaching: New regression from hotfix → FAIL. Cannot be normalized even for hotfix urgency.
