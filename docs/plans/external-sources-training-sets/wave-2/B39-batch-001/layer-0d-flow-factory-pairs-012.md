# Layer 0-D — Flow Factory Pairs (012)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-012-001 — TEST+UI: visual regression tests

A: ["TEST", "UI"] | "Add visual regression tests for the header component at all breakpoints"
Factory: [E2ETestingFlow53, UIInteractionFlow15]
Sub-phases: 5.3 (E2E/visual regression context) + 1.5 | TransitionType: documentation

---

## FF-012-002 — IMPLEMENT+TEST+UI: search autocomplete

A: ["IMPLEMENT", "TEST", "UI"] | "Build search autocomplete, write interaction tests, fix dropdown position"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15]
TransitionType: feature_addition

---

## FF-012-003 — TEST+UI+QA: search autocomplete QA gate

A: ["TEST", "UI", "QA"] | "Write E2E tests for search, fix result dropdown, QA sign-off"
Factory: [E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
TransitionType: documentation | QA: 7.3 | QA last

---

## FF-012-004 — IMPLEMENT+TEST+UI+QA: password reset flow

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build password reset form, write form tests, fix input focus styling, QA sign-off"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
TransitionType: feature_addition | QA: 7.3 | QA last

---

## FF-012-005 — GapHandler: operational UI task

A: [] | "Update the Figma design tokens"
Factory: GapHandler | SCHEDULE_CAPABILITY_LEARNING
Reason: "update Figma design tokens" is a design tool task, not code implementation or UI predicate trigger. Gap.
