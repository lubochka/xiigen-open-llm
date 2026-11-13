# Layer 1 — Transition Orchestrator Pairs (028) — PerformanceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-028-001 — TEST+QA sequential ordering for performance gate

A: ["TEST", "QA"] | TEST → QA (sequential; QA always after TEST)
  TransitionType: documentation
  Teaching: QA always executes after TEST. No predicate follows QA.

## B28-transition-028-002 — IMPLEMENT+TEST+QA with IMPLEMENT first

A: ["IMPLEMENT", "TEST", "QA"] | IMPLEMENT → TEST → QA
  TransitionType: feature_addition
  Teaching: Strictly sequential. IMPLEMENT → TEST → QA. IMPLEMENT drives feature_addition.

## B28-transition-028-003 — Gap: "set performance threshold" → no orchestration

A: [] | "set the performance gate threshold" → no flows → no orchestration
  Teaching: Performance threshold configuration is gap. No orchestration.

## B28-transition-028-004 — DOCUMENT runs in parallel with IMPLEMENT

A: ["IMPLEMENT", "DOCUMENT", "QA"] | ARCHITECTURE absent → parallel(IMPLEMENT ‖ DOCUMENT) → QA
  TransitionType: feature_addition
  Teaching: Without ARCHITECTURE, IMPLEMENT and DOCUMENT can run in parallel. QA last.

## B28-transition-028-005 — FAIL is valid for performance gate exceeding threshold

A: ReleaseGateFlow71: FAIL if p99 latency exceeds threshold | not PASS_WITH_WARNINGS if threshold is hard
  Teaching: Hard performance thresholds → FAIL. Models must not normalize FAIL to PASS_WITH_WARNINGS.
