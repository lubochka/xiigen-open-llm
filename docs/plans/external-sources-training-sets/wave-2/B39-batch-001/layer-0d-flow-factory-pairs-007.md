# Layer 0-D — Flow Factory Pairs (007)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-007-001 — UI alone: documentation TransitionType

A: ["UI"] | "Update the color theme to dark mode"
Factory: [UIInteractionFlow15] | TransitionType: documentation (UI-only, no IMPLEMENT, TEST, or QA)

---

## FF-007-002 — IMPLEMENT alone: feature_addition

A: ["IMPLEMENT"] | "Build the multi-step form wizard"
Factory: [ImplementationFlow41] | TransitionType: feature_addition

---

## FF-007-003 — IMPLEMENT+UI: two flows, feature_addition

A: ["IMPLEMENT", "UI"] | "Build the file upload drag-and-drop component"
Factory: [ImplementationFlow41, UIInteractionFlow15] | TransitionType: feature_addition

---

## FF-007-004 — TEST+UI+QA: QA 7.1 when staging vocabulary

A: ["TEST", "UI", "QA"] | "Write smoke tests, verify UI on staging, QA the release"
Factory: [E2ETestingFlow53, UIInteractionFlow15, ReleaseGateFlow71]
Sub-phases: 5.3 + 1.5 + 7.1 (staging/release vocabulary → 7.1)

---

## FF-007-005 — IMPLEMENT+TEST+UI+QA: full feature cycle with QA 7.3

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build autocomplete search, write tests, fix dropdown styling, QA sign-off on feature"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
Sub-phases: 4.1 + 5.3 + 1.5 + 7.3 | TransitionType: feature_addition | QA last
