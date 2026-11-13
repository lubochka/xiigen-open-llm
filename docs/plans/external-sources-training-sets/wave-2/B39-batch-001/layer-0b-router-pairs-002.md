# Layer 0-B — Router Pairs (002)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-002-001 — TEST routes to 5.3 for E2E/UI tests

A: ["TEST"] | "Write E2E tests for the checkout form"
B: Sub-phase 5.3 → E2ETestingFlow53
Reason: E2E/browser/UI test context → 5.3. Unit test context → 5.1. "E2E" keyword disambiguates to 5.3.

---

## C0B-002-002 — UI routes to 1.5 always

A: ["UI"] | "Fix the modal focus trap behavior"
B: Sub-phase 1.5 → UIInteractionFlow15
Reason: UI predicate always routes to sub-phase 1.5 regardless of context (testing, implementation, or pure UI fix).

---

## C0B-002-003 — TEST+UI: two separate sub-phases

A: ["TEST", "UI"] | "Write tests for modal interaction and fix modal focus trap"
B: TEST → sub-phase 5.3 (E2ETestingFlow53); UI → sub-phase 1.5 (UIInteractionFlow15). Two flows.
Reason: Each predicate routes to its own sub-phase independently. UI is always 1.5; E2E test context is always 5.3.

---

## C0B-002-004 — QA sub-phase 7.3 (feature acceptance)

A: ["QA"] | "QA sign-off on the registration form UI"
B: Sub-phase 7.3 → QAGatingFlow73
Reason: "sign-off on the form" → feature acceptance vocabulary → 7.3. No release/deploy/staging vocabulary.

---

## C0B-002-005 — QA sub-phase 7.1 (release pipeline)

A: ["QA"] | "QA the staging deployment before release cut"
B: Sub-phase 7.1 → ReleaseGateFlow71
Reason: "staging deployment", "release cut" → release pipeline vocabulary → 7.1.
