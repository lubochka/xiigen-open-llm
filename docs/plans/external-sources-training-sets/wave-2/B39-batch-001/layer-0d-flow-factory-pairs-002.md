# Layer 0-D — Flow Factory Pairs (002)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-002-001 — TEST alone → E2ETestingFlow53

A: ["TEST"] | "Write E2E tests for the checkout form"
Factory selection: [E2ETestingFlow53] | TransitionType: documentation | DomainId: software-development

---

## FF-002-002 — UI alone → UIInteractionFlow15

A: ["UI"] | "Fix the modal focus trap behavior"
Factory selection: [UIInteractionFlow15] | TransitionType: documentation | DomainId: software-development

---

## FF-002-003 — TEST+UI → two flows

A: ["TEST", "UI"] | "Write interaction tests for the modal and fix modal styling"
Factory selection: [E2ETestingFlow53, UIInteractionFlow15] | TransitionType: documentation | DomainId: software-development

---

## FF-002-004 — TEST+UI+QA → three flows

A: ["TEST", "UI", "QA"] | "Write E2E tests, fix button styling, QA sign-off"
Factory selection: [E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73] | TransitionType: documentation | Execution: E2E → UI → QA (QA last)

---

## FF-002-005 — IMPLEMENT+TEST+UI+QA → four flows

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build date picker, write tests, fix calendar CSS, QA sign-off"
Factory selection: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73] | TransitionType: feature_addition | QA last
