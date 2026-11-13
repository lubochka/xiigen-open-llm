# Layer 1 — Transition Orchestrator Pairs (034) — StaticAnalysisGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-034-001 — TEST+QA for static analysis gate

A: ["TEST", "QA"] | TestingFlow(StaticAnalysisGate) → ReleaseGateFlow71 (QA last)
  TransitionType: documentation
  Teaching: Static analysis gate → TEST flow. QA validates results last.

## B28-transition-034-002 — IMPLEMENT+TEST+QA sequential

A: ["IMPLEMENT", "TEST", "QA"] | IMPLEMENT → TEST → QA
  TransitionType: feature_addition
  Teaching: Code must be written (IMPLEMENT) before it can be analyzed (TEST). QA last.

## B28-transition-034-003 — Gap: "configure linting rules" → no orchestration

A: [] | "configure ESLint (JavaScript linting tool) rules and enable prettier" → no flows → no orchestration
  Teaching: Linting and static analysis tool configuration is gap. No orchestration.

## B28-transition-034-004 — SECURITY+TEST+QA for security-focused static analysis

A: ["SECURITY", "TEST", "QA"] | parallel(SECURITY ‖ TEST) → QA
  TransitionType: documentation
  Teaching: Security-focused static analysis: SECURITY and TEST in parallel. QA last.

## B28-transition-034-005 — FAIL for critical static analysis findings

A: ReleaseGateFlow71: FAIL if static analysis finds critical code quality or security violations
  Teaching: Critical static analysis findings → FAIL. Cannot be normalized.
