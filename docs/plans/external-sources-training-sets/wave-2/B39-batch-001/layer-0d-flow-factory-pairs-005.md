# Layer 0-D — Flow Factory Pairs (005)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-005-001 — Zero predicates → GapHandler

A: [] | "Automate the UI tests with Playwright"
Factory selection: GapHandler | SCHEDULE_CAPABILITY_LEARNING emitted | DomainId: software-development

---

## FF-005-002 — IMPLEMENT+TEST+UI: feature_addition TransitionType

A: ["IMPLEMENT", "TEST", "UI"] | "Build infinite scroll and write render tests"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15] | TransitionType: feature_addition (IMPLEMENT present)

---

## FF-005-003 — TEST+UI: documentation TransitionType (no IMPLEMENT)

A: ["TEST", "UI"] | "Write accessibility tests for existing form controls"
Factory: [E2ETestingFlow53, UIInteractionFlow15] | TransitionType: documentation (no IMPLEMENT)

---

## FF-005-004 — QA 7.1 → ReleaseGateFlow71

A: ["QA"] | "QA the release branch before production cut"
Factory: [ReleaseGateFlow71] | Sub-phase 7.1 (release vocabulary)
Contrast: "QA sign-off on feature" → [QAGatingFlow73] | Sub-phase 7.3

---

## FF-005-005 — Full five-predicate: IMPLEMENT+TEST+UI+DOCUMENT+QA

A: ["IMPLEMENT", "TEST", "UI", "DOCUMENT", "QA"] | "Build tab component, write tests, fix styling, document API, QA sign-off"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, DocumentationFlow62, QAGatingFlow73]
TransitionType: feature_addition | Execution: IMPLEMENT → TEST + UI (parallel) → DOCUMENT → QA
