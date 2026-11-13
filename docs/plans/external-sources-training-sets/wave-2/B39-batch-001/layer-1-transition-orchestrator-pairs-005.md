# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-005-001 — v6-03 corruption: IMPLEMENT+TEST+UI+QA collapsed to two

Correct: ["IMPLEMENT", "TEST", "UI", "QA"] → four flows, QA last
Corrupt: ["IMPLEMENT", "QA"] only ← TEST and UI flows suppressed
Teaching: Every predicate independently instantiates its own flow. No flow is suppressible. QA gate validates output of ALL preceding flows.

---

## TO-005-002 — v6-01 corruption: parallel orchestration invented

Correct: ["TEST", "UI", "QA"] → sequential with QA last
Corrupt: All three flows run in parallel ← wrong (QA must be last; it validates other flows' output)
Teaching: QA always executes after all other flows. Parallel execution allowed only for flows that QA needs to validate.

---

## TO-005-003 — TransitionType: "add" as IMPLEMENT trigger

A: ["IMPLEMENT", "UI"] | "Add the loading spinner to the checkout button"
TransitionType: feature_addition
Reason: "add" in feature-addition context → IMPLEMENT. "checkout button", "loading spinner" → UI. feature_addition TransitionType.

---

## TO-005-004 — Execution order for TEST+UI: UI fix before or after tests?

A: ["TEST", "UI"] | "Write tests for the button states, then fix the disabled button styling"
Orchestration: E2ETestingFlow53 runs first (tests define what is correct). UIInteractionFlow15 runs after (fix based on test findings). No QA → no ordering requirement from QA.
Teaching: Without QA, TEST and UI can be parallel or sequential. With QA, both complete before QA.

---

## TO-005-005 — QA gate validates UI output specifically

A: ["UI", "QA"] | "Fix all modal accessibility issues and QA sign-off"
Orchestration:
  1. UIInteractionFlow15 — fix modal ARIA labels, focus trap, keyboard navigation
  2. QAGatingFlow73 — QA verifies: does modal meet accessibility requirements? (PASS/FAIL)
Teaching: QA gate evaluates the specific outputs of co-flow(s). UI gate → QA validates UI conformance.
