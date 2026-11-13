# Layer 1 — Transition Orchestrator Pairs (028)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T028-001 — LoginFlowTest: TransitionType

A: "Write E2E test cases for the login page — assert the login button state, verify error messages." TransitionType?
B: documentation. TEST+UI without IMPLEMENT → documentation. Test-writing for the login page is documentation.

---

## T028-002 — LoginFlowTest: Playwright is gap — TransitionType unchanged

A: "Use Playwright to test the login form." Predicates: [UI]. TransitionType?
B: documentation. UI fires on "login form" → form. TEST does not fire (Playwright is gap, "use" is gap). UI alone → documentation. TransitionType: documentation.

---

## T028-003 — LoginFlowTest: [TEST, UI, QA] execution phases

A: [TEST, UI, QA → 7.3] fires for login test request. Execution phases?
B: Phase 1 (parallel): E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). Phase 2 (last): QAGatingFlow73 (7.3). TransitionType: documentation. QA gate validates the login test suite.

---

## T028-004 — LoginFlowTest: "build login page" IMPLEMENT fires

A: "Build the login page and write E2E test coverage for it." TransitionType and phases?
B: TransitionType: feature_addition ("build" → IMPLEMENT). Phases: BackendServiceFlow41 (4.1) → E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5) parallel.

---

## T028-005 — LoginFlowTest: toast error fires TEST+UI → documentation

A: "Assert the error toast fires on invalid login credentials." TransitionType?
B: documentation. TEST (assert) + UI (toast) → documentation. No IMPLEMENT. The toast assertion is a documentation activity: it documents how the error UI state should behave.
