# Layer 0-D — Flow Factory Pairs (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-006-001 — TEST+UI+QA: sub-phase 5.3 + 1.5 + 7.3

A: ["TEST", "UI", "QA"] | "Write responsive tests, fix breakpoint layout, QA sign-off"
Factory: [E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
Sub-phases: 5.3 + 1.5 + 7.3 | Execution: TEST(5.3) → UI(1.5) → QA(7.3)

---

## FF-006-002 — TEST 5.1 vs 5.3 factory selection

A: ["TEST"] | "Write unit tests for the price formatter function"
Factory: [UnitTestingFlow51] (5.1) — pure function context

A: ["TEST"] | "Write browser tests for the price display on the product page"
Factory: [E2ETestingFlow53] (5.3) — browser/UI context

---

## FF-006-003 — IMPLEMENT+UI with refactoring TransitionType

A: ["IMPLEMENT", "UI"] | "Refactor the button component to use CSS custom properties"
Factory: [ImplementationFlow41, UIInteractionFlow15] | TransitionType: refactoring (refactor verb present)
Note: "refactor" overrides default feature_addition with refactoring TransitionType.

---

## FF-006-004 — v6-02: gap → TEST (automation context incorrectly triggers)

A: []
Correct: GapHandler
Corrupt: [E2ETestingFlow53] ← v6-02 ("automate UI tests" incorrectly maps to TEST flow)
Teaching: "automate" without test-writing verb → gap. E2ETestingFlow53 requires TEST predicate from test-writing verbs.

---

## FF-006-005 — SECURITY+TEST+UI → three flows (XSS + UI context)

A: ["SECURITY", "TEST", "UI"] | "Write tests for XSS prevention in form inputs and validate sanitization"
Factory: [SecurityAuditFlow62, E2ETestingFlow53, UIInteractionFlow15]
TransitionType: documentation (no IMPLEMENT) | Three flows for security testing in UI context
