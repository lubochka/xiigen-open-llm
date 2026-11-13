# Layer 1 — Transition Orchestrator Pairs (027)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T027-001 — UserRegistrationTest: TransitionType

A: "Create E2E test cases for the registration form — verify required fields, assert email format." TransitionType?
B: documentation. TEST+UI without IMPLEMENT → documentation. Creating test cases = documentation artifact.

---

## T027-002 — UserRegistrationTest: "add tests" does not fire IMPLEMENT

A: "Add E2E tests for the registration form." TransitionType?
B: documentation. "add tests" → TEST (adding test coverage — test-writing verb with test artifact noun). IMPLEMENT does not fire. TransitionType: documentation. This is a key disambiguation: "add [test noun]" → TEST; "add [feature noun]" → IMPLEMENT.

---

## T027-003 — UserRegistrationTest: IMPLEMENT via "build form" — phases

A: "Build the registration form component and write E2E tests." Execution phases?
B: Phase 1: BackendServiceFlow41 (4.1) — "build the registration form component" (IMPLEMENT). Phase 2 (parallel): E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: feature_addition.

---

## T027-004 — UserRegistrationTest: duplicate email error modal — TransitionType

A: "Assert the duplicate email error modal appears on registration." TransitionType?
B: documentation. TEST (assert) + UI (modal) fires. No IMPLEMENT. TransitionType: documentation. The assertion is a test documentation activity even though it tests an error state.

---

## T027-005 — UserRegistrationTest: gap → knowledge_edge

A: "Check that user registration works." TransitionType?
B: No TransitionType. Zero predicates → knowledge_edge_synthetic. "check" is gap. "user registration" is a domain term, not a predicate trigger. TransitionType is not assigned for knowledge_edge requests.
