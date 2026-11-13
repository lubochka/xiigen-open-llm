# Layer 0-D — Flow Factory Pairs (009)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-009-001 — IMPLEMENT+UI+TEST+QA: tab component complete cycle

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build the tab panel component, write keyboard tests, fix tab styling, QA sign-off"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
TransitionType: feature_addition | QA: 7.3 (feature acceptance) | Execution: sequential, QA last

---

## FF-009-002 — IMPLEMENT+UI: carousel component

A: ["IMPLEMENT", "UI"] | "Build the image carousel with swipe gestures"
Factory: [ImplementationFlow41, UIInteractionFlow15]
TransitionType: feature_addition

---

## FF-009-003 — TEST+UI: accessibility tests for carousel

A: ["TEST", "UI"] | "Write accessibility tests for the carousel keyboard controls"
Factory: [E2ETestingFlow53, UIInteractionFlow15]
TransitionType: documentation (no IMPLEMENT)

---

## FF-009-004 — UI+QA: carousel QA gate

A: ["UI", "QA"] | "Fix carousel touch handling on mobile and QA sign-off"
Factory: [UIInteractionFlow15, QAGatingFlow73]
TransitionType: documentation | QA last

---

## FF-009-005 — IMPLEMENT+TEST+UI+QA: carousel complete cycle

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build carousel, write swipe tests, fix mobile animation, QA sign-off"
Factory: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
TransitionType: feature_addition | QA: 7.3 | QA last
