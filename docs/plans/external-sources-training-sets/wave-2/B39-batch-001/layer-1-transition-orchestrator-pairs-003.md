# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-003-001 — TransitionType determination: IMPLEMENT present → feature_addition

A: ["IMPLEMENT", "UI", "QA"] | "Build the stepper component and QA sign-off"
TransitionType: feature_addition (IMPLEMENT present). QA does not drive TransitionType.
Rule: IMPLEMENT → feature_addition (highest priority). TEST+DOCUMENT (no IMPLEMENT) → documentation. QA alone → no TransitionType contribution.

---

## TO-003-002 — TransitionType determination: refactor → refactoring

A: ["IMPLEMENT", "UI", "TEST"] | "Refactor the button component to use tokens and update tests"
TransitionType: refactoring ("refactor" verb overrides feature_addition)
Rule: "refactor" verb present → refactoring TransitionType overrides IMPLEMENT's default feature_addition.

---

## TO-003-003 — TransitionType determination: TEST+UI without IMPLEMENT → documentation

A: ["TEST", "UI"] | "Write tests for existing carousel component and fix animation timing"
TransitionType: documentation (IMPLEMENT absent; testing existing functionality → documentation context)

---

## TO-003-004 — QA gate scope: validates all preceding flows

A: ["IMPLEMENT", "TEST", "UI", "QA"]
QA gate validates:
  - IMPLEMENT output: is the feature complete and code reviewed?
  - TEST output: do all tests pass? Is coverage adequate?
  - UI output: does the component meet design specs?
  Only after all three pass → QA verdict PASS/PASS_WITH_WARNINGS/FAIL.

---

## TO-003-005 — Parallel execution model

A: ["IMPLEMENT", "TEST", "UI", "QA"]
Execution model:
  Phase 1 (sequential): IMPLEMENT (implementation complete)
  Phase 2 (parallel): E2ETestingFlow53 + UIInteractionFlow15 (tests + UI fixes can proceed simultaneously)
  Phase 3 (sequential): QAGatingFlow73 (after both Phase 2 flows complete)
Constraint: QA always after all other flows. IMPLEMENT always before TEST.
