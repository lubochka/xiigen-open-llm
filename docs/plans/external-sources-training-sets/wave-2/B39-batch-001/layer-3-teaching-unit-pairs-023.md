# Layer 3 — Teaching Unit Pairs (023)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU023-001 — multiple test-writing verbs → one TEST predicate (deduplication)

A: "Write e2e test cases for the modal, validate every ARIA role, and QA the test suite." How many TEST predicates fire?
B: One. "write e2e test cases" → TEST trigger 1. "validate" → TEST trigger 2. Both deduplicate to one TEST predicate. QA fires once. UI fires on "modal" + "ARIA role". Total predicates: ["TEST", "UI", "QA"].
Teaching: Deduplication applies to multiple triggers of the same predicate. Two TEST triggers → one TEST predicate.

---

## TU023-002 — "update" is gap (neither IMPLEMENT nor TEST)

A: "Create test coverage for checkout button states, update the responsive layout, and QA sign-off." Does IMPLEMENT fire for "update"?
B: No. "update" is gap for IMPLEMENT. IMPLEMENT does not fire. "create test coverage" → TEST. "checkout button" + "responsive layout" → UI. "QA sign-off" → QA.
Result: ["TEST", "UI", "QA"]. TransitionType: documentation (no IMPLEMENT). Teaching: "update" is the most common IMPLEMENT false-positive. It is consistently gap in B39.

---

## TU023-003 — "release acceptance" without staging → QA → 7.3 (not 7.1)

A: "Verify the screen layout and QA the results for release acceptance." Does QA route to 7.1?
B: No. "release acceptance" without staging/deployment/production vocabulary → QA → 7.3 (feature acceptance gate). "release" alone does not shift QA to 7.1. Only explicit staging/deployment vocabulary shifts QA to 7.1.
Result: TEST → 5.3, UI → 1.5, QA → 7.3. Teaching: "release" in "release acceptance" is feature-release vocabulary (7.3), not pipeline/deployment vocabulary (7.1).

---

## TU023-004 — "navigation component" fires UI via both "navigation" and "component"

A: "Assert that every navigation component passes interaction tests." What fires?
B: TEST fires on "assert" + "interaction tests" (deduplicated → one TEST). UI fires on "navigation component" ("navigation" → nav → UI; "component" → UI; two UI triggers → one UI predicate).
Result: ["TEST", "UI"]. Teaching: "navigation component" fires UI via two vocabulary words simultaneously — deduplicated to one UI predicate.

---

## TU023-005 — three-predicate combination always routes to three sub-phases independently

A: "Write accessibility tests for the navigation component and QA sign-off." What routes where?
B: TEST → 5.3 (E2E testing context). UI → 1.5 (navigation + component vocabulary). QA → 7.3 (sign-off = feature acceptance).
AllSubPhaseIds: [5.3, 1.5, 7.3]. Three separate routing decisions, three flows: E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73.
Teaching: Three predicates always route to three independent sub-phases. They are never merged.
