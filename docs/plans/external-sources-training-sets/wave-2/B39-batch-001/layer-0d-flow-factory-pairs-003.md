# Layer 0-D — Flow Factory Pairs (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-003-001 — TEST 5.1 → UnitTestingFlow51

A: ["TEST"] | "Write unit tests for the form validation utility"
Factory selection: [UnitTestingFlow51] | TransitionType: documentation
Reason: "unit tests" + pure function context → 5.1, not 5.3.

---

## FF-003-002 — IMPLEMENT+UI → two flows

A: ["IMPLEMENT", "UI"] | "Build the skeleton loader component"
Factory selection: [ImplementationFlow41, UIInteractionFlow15] | TransitionType: feature_addition

---

## FF-003-003 — IMPLEMENT+TEST+UI → three flows

A: ["IMPLEMENT", "TEST", "UI"] | "Build the accordion component and write keyboard navigation tests"
Factory selection: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15] | TransitionType: feature_addition

---

## FF-003-004 — UI+QA → two flows

A: ["UI", "QA"] | "Fix dropdown disabled state and QA review"
Factory selection: [UIInteractionFlow15, QAGatingFlow73] | TransitionType: documentation | QA last

---

## FF-003-005 — IMPLEMENT+UI+QA → three flows (no TEST)

A: ["IMPLEMENT", "UI", "QA"] | "Build the notification toast component and QA sign-off"
Factory selection: [ImplementationFlow41, UIInteractionFlow15, QAGatingFlow73] | TransitionType: feature_addition | QA last
