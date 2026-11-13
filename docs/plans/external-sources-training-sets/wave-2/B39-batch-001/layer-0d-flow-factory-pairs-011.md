# Layer 0-D — Flow Factory Pairs (011)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-011-001 — QA alone → single QA flow

A: ["QA"] | "QA the updated checkout flow UI"
Factory: [QAGatingFlow73] | Sub-phase 7.3 | QA does not drive TransitionType
TransitionType: determined by co-predicates. QA alone → no TransitionType set by QA.

---

## FF-011-002 — IMPLEMENT+QA (no TEST, no UI)

A: ["IMPLEMENT", "QA"] | "Implement the retry logic and QA sign-off"
Factory: [ImplementationFlow41, QAGatingFlow73] | TransitionType: feature_addition | QA last

---

## FF-011-003 — TEST+QA (no UI) → two flows

A: ["TEST", "QA"] | "Write regression tests and QA sign-off before merge"
Factory: [E2ETestingFlow53 or UnitTestingFlow51, QAGatingFlow73]
Sub-phase for TEST: depends on test type (E2E default for ambiguous QA gate context) | QA last

---

## FF-011-004 — v6-01: UIFlow15 + E2ETestingFlow15 invented

A: ["TEST", "UI"]
Correct: [E2ETestingFlow53, UIInteractionFlow15]
Corrupt: [UIE2EFlow15] ← v6-01 (merged UIE2EFlow15 does not exist)
Teaching: No merged UI+TEST flow exists. Each predicate independently instantiates its own flow.

---

## FF-011-005 — IMPLEMENT+TEST+UI+QA: registration form complete

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build registration form, add form validation tests, fix label styling, QA sign-off"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
TransitionType: feature_addition | QA: 7.3 | Four flows | QA last
