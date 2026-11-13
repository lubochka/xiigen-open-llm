# Layer 1 — Transition Orchestrator Pairs (031) — DependencyCheckGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-031-001 — SECURITY+QA for dependency check gate

A: ["SECURITY", "QA"] | SecurityFlow6x(DependencyCheckGate) → ReleaseGateFlow71 (QA last)
  TransitionType: documentation
  Teaching: Dependency check gates fire SECURITY → SecurityFlow6x. QA validates last.

## B28-transition-031-002 — TEST+SECURITY+QA parallel orchestration

A: ["TEST", "SECURITY", "QA"] | parallel(TEST ‖ SECURITY) → QA
  TransitionType: documentation
  Teaching: TEST and SECURITY can run in parallel. QA executes last after both.

## B28-transition-031-003 — Gap: "update dependency lockfile" → no orchestration

A: [] | "update the dependency lockfile and pin versions" → no flows → no orchestration
  Teaching: Dependency update configuration is gap. No orchestration.

## B28-transition-031-004 — IMPLEMENT+SECURITY+TEST+QA orchestration

A: ["IMPLEMENT", "SECURITY", "TEST", "QA"] | parallel(IMPLEMENT ‖ SECURITY ‖ TEST) → QA
  TransitionType: feature_addition
  Teaching: IMPLEMENT, SECURITY, and TEST can all run in parallel. QA is strictly last.

## B28-transition-031-005 — FAIL for high-severity dependency vulnerability

A: ReleaseGateFlow71: FAIL if high-severity known vulnerability in dependency
  Teaching: High-severity dependency vulnerabilities → FAIL. Cannot be normalized.
