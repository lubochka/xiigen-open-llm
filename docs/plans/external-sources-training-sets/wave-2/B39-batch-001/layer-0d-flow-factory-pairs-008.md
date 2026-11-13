# Layer 0-D — Flow Factory Pairs (008)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-008-001 — Refactoring TransitionType with TEST+UI

A: ["TEST", "UI"] | "Refactor the form component and update interaction tests"
Factory: [E2ETestingFlow53, UIInteractionFlow15] | TransitionType: refactoring ("refactor" verb)

---

## FF-008-002 — UI alone: refactoring TransitionType

A: ["UI"] | "Refactor the button component CSS to use the design system tokens"
Factory: [UIInteractionFlow15] | TransitionType: refactoring

---

## FF-008-003 — DOCUMENT+UI+QA

A: ["UI", "DOCUMENT", "QA"] | "Document design system components, get QA sign-off on documentation"
Factory: [UIInteractionFlow15, DocumentationFlow62, QAGatingFlow73]
TransitionType: documentation | QA last

---

## FF-008-004 — IMPLEMENT+TEST+UI: sequential execution

A: ["IMPLEMENT", "TEST", "UI"] | "Implement the progress bar, write progress animation tests, fix animation timing"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15]
Execution: IMPLEMENT → TEST (parallel with UI fix) | TransitionType: feature_addition

---

## FF-008-005 — Factory resolution: "write tests" vs "run tests"

A: ["TEST"] | "Write integration tests for the cart UI"
Factory: [E2ETestingFlow53] (correct — test-writing verb)

A: [] | "Run integration tests for the cart UI"
Factory: GapHandler (correct — "run" is operational gap)

Teaching: Factory selection requires TEST predicate from test-writing verb. "run tests" → gap → GapHandler.
