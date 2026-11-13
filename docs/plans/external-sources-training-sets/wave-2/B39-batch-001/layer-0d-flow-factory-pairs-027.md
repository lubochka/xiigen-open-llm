# Layer 0-D — Flow Factory Pairs (027)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F027-001 — UserRegistrationTest: flow selection

A: "Create E2E test cases for the user registration form — verify required fields, assert email format validation." Predicates [TEST, UI]. Flows?
B: TEST → E2ETestingFlow53 (5.3). UI → UIInteractionFlow15 (1.5) ("registration form" → form). TransitionType: documentation. TrustState: deterministic_generic. Parallel execution.

---

## F027-002 — UserRegistrationTest: gap — "check registration"

A: "Check that user registration works." Predicates? Flows?
B: Zero predicates. "check" is gap. "registration" alone is gap. Result: [] → knowledge_edge_synthetic. No FlowInstances. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## F027-003 — UserRegistrationTest: IMPLEMENT co-fires

A: "Build the registration form component and write E2E test coverage for required field validation." Flows?
B: BackendServiceFlow41 (4.1) — "build the registration form component". E2ETestingFlow53 (5.3) — "write E2E test coverage". UIInteractionFlow15 (1.5) — "registration form" + "component" → UI. Three flows. TransitionType: feature_addition. Order: 4.1 → 5.3 + 1.5.

---

## F027-004 — UserRegistrationTest: "sign-up screen" fires UI

A: "Write test scenarios for the sign-up screen — assert the submit button is disabled until required fields are filled." Flows selected?
B: TEST → E2ETestingFlow53 (5.3) ("write test scenarios", "assert"). UI → UIInteractionFlow15 (1.5) ("sign-up screen" → screen; "submit button" → button; two UI triggers → one UI predicate). Flows: [E2ETestingFlow53, UIInteractionFlow15]. TransitionType: documentation.

---

## F027-005 — UserRegistrationTest: Node 1 SelectedLogicalTransition

A: Node 1 for UserRegistrationTest. What is SelectedLogicalTransition and what domains are identified?
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: registration form fields (email, password, confirm-password, username), submit button state, email format validation, password strength indicator, success redirect, duplicate email error modal.
