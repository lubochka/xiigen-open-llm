# Layer 0-D — Flow Factory Pairs (028)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F028-001 — LoginFlowTest: flow selection

A: "Write E2E test cases for the login page — assert the login button is disabled when fields are empty, verify error message on invalid credentials." Predicates [TEST, UI]. Flows?
B: E2ETestingFlow53 (5.3) for TEST. UIInteractionFlow15 (1.5) for UI ("login page" → page; "login button" → button). TransitionType: documentation. TrustState: deterministic_generic.

---

## F028-002 — LoginFlowTest: framework name is gap

A: "Use Playwright to automate the login flow and test the login form." Predicates? Flows?
B: UI fires on "login form" → form. TEST fires on "test the login form" → "test" as a verb with test noun "form" — actually "test" as imperative verb without explicit test-writing vocabulary = borderline. Safe rule: "test" alone as a verb is ambiguous. "automate" is gap. "Playwright" is gap. But "login form" → UI fires. Conservative result: [UI]. UIInteractionFlow15 (1.5) only.

---

## F028-003 — LoginFlowTest: [TEST, UI, QA] full factory

A: "Write login E2E specs, assert the error toast appears on invalid login, and QA gate before merge." Full flow factory output?
B: E2ETestingFlow53 (5.3): TEST. UIInteractionFlow15 (1.5): UI ("toast" fires UI). QAGatingFlow73 (7.3): QA ("QA gate before merge" = feature acceptance, not release → 7.3). TransitionType: documentation. Order: 5.3 + 1.5 parallel → 7.3. TrustState: deterministic_generic.

---

## F028-004 — LoginFlowTest: TransitionType when IMPLEMENT added

A: "Build the login page and write E2E test coverage for it." TransitionType and AllSubPhaseIds?
B: TransitionType: feature_addition (IMPLEMENT present via "build"). AllSubPhaseIds: [4.1, 5.3, 1.5]. BackendServiceFlow41 executes first; E2ETestingFlow53 + UIInteractionFlow15 execute in parallel after.

---

## F028-005 — LoginFlowTest: Node 1 for login scenarios

A: Node 1 domain identification for LoginFlowTest.
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: login form (email/password inputs), login button state (enabled/disabled), invalid credentials error display, locked-account state, successful redirect, remember-me toggle, forgot-password link.
