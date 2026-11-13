# Layer 0-D — Flow Factory Pairs (010)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-010-001 — All five flows: IMPLEMENT+TEST+UI+DOCUMENT+QA

A: ["IMPLEMENT", "TEST", "UI", "DOCUMENT", "QA"] | "Build the design system button, write tests, fix variants, document usage, QA sign-off"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, DocumentationFlow62, QAGatingFlow73]
TransitionType: feature_addition | QA last | Execution: IMPLEMENT → TEST+UI (parallel) → DOCUMENT → QA

---

## FF-010-002 — IMPLEMENT refactoring: overrides feature_addition

A: ["IMPLEMENT", "UI"] | "Refactor the modal component to use the new animation library"
Factory: [ImplementationFlow41, UIInteractionFlow15]
TransitionType: refactoring (refactor verb overrides feature_addition)

---

## FF-010-003 — TEST alone: UnitTestingFlow51 for pure function

A: ["TEST"] | "Write unit tests for the color contrast utility function"
Factory: [UnitTestingFlow51] | Sub-phase 5.1 | TransitionType: documentation

---

## FF-010-004 — UI alone: UIInteractionFlow15

A: ["UI"] | "Add dark mode support to the notification toast"
Factory: [UIInteractionFlow15] | Sub-phase 1.5 | TransitionType: documentation

---

## FF-010-005 — IMPLEMENT+TEST+UI+QA: login form complete

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build login form validation, write validation tests, fix input error styling, QA sign-off"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
TransitionType: feature_addition | QA: 7.3 | QA last
