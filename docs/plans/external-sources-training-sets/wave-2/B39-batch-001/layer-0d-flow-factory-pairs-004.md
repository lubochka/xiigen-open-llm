# Layer 0-D — Flow Factory Pairs (004)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-004-001 — TEST+QA (no UI) → two flows

A: ["TEST", "QA"] | "Write API integration tests and QA sign-off"
Factory selection: [E2ETestingFlow53, QAGatingFlow73] | TransitionType: documentation | QA last
Reason: No UI vocabulary → UI does not fire. Two flows only.

---

## FF-004-002 — UI+DOCUMENT → two flows

A: ["UI", "DOCUMENT"] | "Document the carousel component API"
Factory selection: [UIInteractionFlow15, DocumentationFlow62] | TransitionType: documentation

---

## FF-004-003 — DOCUMENT+TEST+UI+QA → four flows (no IMPLEMENT)

A: ["TEST", "UI", "DOCUMENT", "QA"] | "Write tests, fix styling, document component, QA sign-off"
Factory selection: [E2ETestingFlow53, UIInteractionFlow15, DocumentationFlow62, QAGatingFlow73] | TransitionType: documentation (no IMPLEMENT) | QA last

---

## FF-004-004 — v6-01: UITesting invented

A: ["TEST", "UI"]
Correct: [E2ETestingFlow53, UIInteractionFlow15]
Corrupt: [UITestingFlow] ← v6-01 (UITestingFlow is not a real flow; no merged TEST+UI flow exists)
Teaching: TEST and UI are independent predicates that independently instantiate their own flows. No merged "UITestingFlow" exists.

---

## FF-004-005 — v6-03: IMPLEMENT+TEST+UI+QA collapsed to IMPLEMENT+QA

A: ["IMPLEMENT", "TEST", "UI", "QA"]
Correct: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
Corrupt: [ImplementationFlow41, QAGatingFlow73] ← v6-03 (TEST and UI flows suppressed)
Teaching: No cap on flow count. All four predicates independently instantiate flows.
