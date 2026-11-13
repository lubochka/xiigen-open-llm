# Layer 0-B — Router Pairs (027)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-027-001 — UserRegistrationTest: routing decision

A: "Write E2E tests for the user registration form — assert required fields, verify the success screen." Predicates: [TEST, UI]. Route each.
B: TEST → 5.3 (E2ETestingFlow53): registration form E2E is browser/UI interaction, not unit test. UI → 1.5 (UIInteractionFlow15): "registration form", "success screen" → "form", "screen" always route to 1.5. TransitionType: documentation.

---

## C0B-027-002 — UserRegistrationTest: "check" is gap — only UI fires

A: "Check that the registration screen accepts valid emails." Predicates: [UI]. Route.
B: UI → 1.5 (UIInteractionFlow15). TEST does not fire ("check" is gap). Single sub-phase: 1.5. TrustState: deterministic_generic. TransitionType: documentation.

---

## C0B-027-003 — UserRegistrationTest: IMPLEMENT + TEST routing

A: "Build the registration form component, write test coverage for email validation." Predicates: [IMPLEMENT, TEST, UI]. Route.
B: IMPLEMENT → 4.1. TEST → 5.3. UI → 1.5. Execution: IMPLEMENT (4.1) first (depends on architecture); TEST (5.3) + UI (1.5) parallel; QA absent — no gating. TransitionType: feature_addition.

---

## C0B-027-004 — UserRegistrationTest: QA routing

A: "Write registration E2E tests and QA acceptance." Predicates: [TEST, UI, QA]. Route QA.
B: QA → 7.3 (QAGatingFlow73): "QA acceptance" is feature-gate vocabulary, default 7.3. No release/staging vocabulary present. AllSubPhaseIds: [5.3, 1.5, 7.3].

---

## C0B-027-005 — UserRegistrationTest: multi-flow execution order

A: "Build the registration page, write E2E tests for valid and invalid submissions, assert the error toast appears, and QA gate." Predicates: [IMPLEMENT, TEST, UI, QA]. What is the execution order?
B: Phase 1: BackendServiceFlow41 (IMPLEMENT — build the page). Phase 2 parallel: E2ETestingFlow53 (TEST — write tests) + UIInteractionFlow15 (UI — component coverage). Phase 3: QAGatingFlow73 (QA — validate all outputs, always last). TransitionType: feature_addition (IMPLEMENT present).
