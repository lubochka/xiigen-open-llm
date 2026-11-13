# Layer 3 — Teaching Unit Pairs (028)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU028-001 — LoginFlowTest: Playwright is gap

A: What does LoginFlowTest teach about framework names?
B: "Use Playwright to test the login form" → only UI fires (login form → form). Playwright is gap — framework names never fire TEST or any predicate. "test" used as an imperative verb without a test noun is borderline; conservative rule: gap. Only UI fires in this request. Result: [UI] only.

---

## TU028-002 — LoginFlowTest: error toast fires UI

A: "Assert the error toast fires on invalid login." Why does toast fire UI?
B: "toast" is in the UI lexicon (snackbar/toast category). All toast variants (success toast, error toast, info toast) fire UI. Combined with "assert" (TEST), result is [TEST, UI]. TransitionType: documentation.

---

## TU028-003 — LoginFlowTest: "login page" fires UI

A: Does "login page" fire UI?
B: Yes. "page" is in the UI lexicon. "login page", "dashboard page", "settings page", "profile page" all fire UI via the "page" token. This is one of the most common UI triggers in B39 Round 2 requests.

---

## TU028-004 — LoginFlowTest: locked-account scenario still documentation

A: "Write tests for the locked-account error state on the login page." TransitionType?
B: documentation. TEST (write tests) + UI (login page → page). The locked-account scenario tests an error state — but error states are still test documentation. No feature is being built. TransitionType: documentation.

---

## TU028-005 — LoginFlowTest: QA gate before merge → 7.3

A: "Write login E2E tests and QA gate before merge." QA routing?
B: QA → 7.3 (QAGatingFlow73). "before merge" = feature acceptance vocabulary, not release vocabulary. No "release", "staging", "production", "deployment" present → default 7.3. AllSubPhaseIds: [5.3, 1.5, 7.3].
