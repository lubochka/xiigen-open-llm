# Layer 0-A — Compiler Pairs (028)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-028-001 — LoginFlowTest: predicate compilation

A: "Write E2E test cases for the login page — assert the username input and password field accept valid credentials, verify the dashboard screen loads after successful login."
B: Predicates: TEST ("write E2E test cases", "assert", "verify" → deduplicated); UI ("login page", "username input", "password field", "screen" → deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-028-002 — LoginFlowTest: locked-out state

A: "Create test scenarios for the locked-out login screen — assert the error banner displays and the retry button is disabled after five failed attempts."
B: Predicates: TEST ("create test scenarios", "assert" → deduplicated); UI ("login screen", "error banner", "retry button" → deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-028-003 — LoginFlowTest: gap-only request

A: "Run the login E2E suite against the staging environment."
B: Predicates: none. "Run" is gap. "Login E2E suite" with only gap verbs does not fire TEST. No UI vocabulary. Result: [] → knowledge_edge routing. CapabilityGapSignal emitted. TrustState: knowledge_edge_synthetic.

---

## C0A-028-004 — LoginFlowTest: IMPLEMENT+TEST+UI

A: "Build the login form with remember-me toggle, add test coverage for the toggle state persistence, and assert the checkbox retains state on reload."
B: Predicates: IMPLEMENT ("build", "add" [feature noun: login form, toggle]); TEST ("add test coverage", "assert" → deduplicated); UI ("login form", "remember-me toggle", "checkbox" → deduplicated). Result: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5].

---

## C0A-028-005 — LoginFlowTest: QA with default 7.3

A: "Write login flow specs, assert the redirect after login goes to the correct screen, and QA gate before the sprint review."
B: Predicates: TEST ("write", "assert" → deduplicated); UI ("screen" → UI); QA ("QA gate" → QA → 7.3 default, no release/staging vocabulary). Result: [TEST, UI, QA]. Routing: [5.3, 1.5, 7.3]. TransitionType: documentation.
