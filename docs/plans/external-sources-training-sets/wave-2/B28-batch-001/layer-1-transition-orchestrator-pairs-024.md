# Layer 1 — Transition Orchestrator Pairs (024) — Complete Orchestrator Reference for 7.1
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-024-001 — QA always executes last in any predicate set

A: Any multi-predicate set including QA | QA gate always executes as the final step
  Sequential examples:
    IMPLEMENT → QA | IMPLEMENT → TEST → QA | ARCHITECTURE → IMPLEMENT → TEST → DOCUMENT → QA
  Teaching: No predicate executes after QA in sequential ordering.

## B28-transition-024-002 — TransitionType decision tree for 7.1

A: IMPLEMENT present → feature_addition (QA does not override)
  No IMPLEMENT, ARCHITECTURE present → documentation
  No IMPLEMENT, DOCUMENT present → documentation
  No IMPLEMENT, TEST present → documentation
  QA alone → no TransitionType
  Gap → no TransitionType

## B28-transition-024-003 — Parallel execution rules for 7.1

A: Allowed parallels: IMPLEMENT ‖ SECURITY | TEST ‖ DOCUMENT | ARCHITECTURE ‖ SECURITY
  Not parallel: ARCHITECTURE before IMPLEMENT | TEST after IMPLEMENT | QA always after everything

## B28-transition-024-004 — FAIL and PASS_WITH_WARNINGS are valid gate verdicts

A: ReleaseGateFlow71 verdicts: PASS | PASS_WITH_WARNINGS | FAIL
  Teaching: Models must not normalize all verdicts to PASS. FAIL is expected for any release context.

## B28-transition-024-005 — Gap produces no transition and no flow

A: Pure gap (any release execution vocabulary) → no flow instantiated → no TransitionType → knowledge_edge output
  Teaching: Pure gap → knowledge_edge (not GapHandler, not SCHEDULE_CAPABILITY_LEARNING).
