# Layer 0-B — Router Pairs (023)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-023-001 — TEST+UI+QA → 5.3 + 1.5 + 7.3: three-predicate dense signal (modal + ARIA + QA)

A: ["TEST", "UI", "QA"] | "Write e2e test cases for the accessibility-compliant modal, validate every ARIA role, and QA the test suite"
B: TEST → 5.3; UI → 1.5; QA → 7.3. Three independent routing decisions.
Reason: "write e2e test cases" + "validate" → TEST → 5.3; "modal" + "ARIA role" → UI → 1.5; "QA the test suite" → QA → 7.3.

---

## C0B-023-002 — TEST+UI+QA → 5.3 + 1.5 + 7.3: three separate clauses

A: ["TEST", "UI", "QA"] | "Create test coverage for checkout button states, update the responsive layout, and QA sign-off"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
Note: "update" is gap (not IMPLEMENT). Three predicates; three routing decisions; no IMPLEMENT; TransitionType: documentation.

---

## C0B-023-003 — TEST+UI+QA → 5.3 + 1.5 + 7.3: accessibility audit three-predicate

A: ["TEST", "UI", "QA"] | "Write accessibility test scenarios for form input and button components, assert WCAG 2.1 AA, QA validate"
B: TEST → 5.3; UI → 1.5; QA → 7.3.

---

## C0B-023-004 — TEST+UI+QA → 5.3 + 1.5 + 7.3: verify + screen + QA

A: ["TEST", "UI", "QA"] | "Verify the mobile screen layout for the checkout flow and QA the results for release acceptance"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
Note: "release acceptance" → is this QA → 7.1? No — "release acceptance" without staging/deployment vocabulary → 7.3 (feature acceptance default). Staging/deploy vocabulary would shift to 7.1.

---

## C0B-023-005 — TEST+UI+QA → 5.3 + 1.5 + 7.3: assert + navigation + QA gate

A: ["TEST", "UI", "QA"] | "Assert that every navigation component passes interaction tests and QA sign-off before shipping"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
AllSubPhaseIds: [5.3, 1.5, 7.3]
