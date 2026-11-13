# Layer 3 — Teaching Unit Pairs (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU003-001 — TEST 5.1 vs 5.3 disambiguation

A: "Write unit tests for the price calculation service." → TEST → 5.1 (UnitTestingFlow51)
A: "Write E2E tests for the checkout form." → TEST → 5.3 (E2ETestingFlow53)
Teaching: "unit tests" + pure logic → 5.1. "E2E"/"smoke"/"integration" + UI/system context → 5.3.

---

## TU003-002 — UI always routes to 1.5

A: "The modal focus trap is broken." → ["UI"] → UIInteractionFlow15 (1.5)
A: "Write tests for the modal focus trap." → ["TEST", "UI"] → E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5)
Teaching: UI predicate always routes to 1.5 regardless of testing context. TEST routes to 5.3 for UI/E2E tests.

---

## TU003-003 — "dashboard" is confirmed UI gap

A: "Fix the analytics dashboard." → [] (gap — "dashboard" not in UI lexicon)
A: "Fix the sidebar in the analytics section." → ["UI"] (gap resolved — "sidebar" is UI vocabulary)
Teaching: "dashboard" does not fire UI. "sidebar", "header", "nav", "breadcrumb" are UI vocabulary.

---

## TU003-004 — Framework names are gap

A: "Migrate from Cypress to Playwright." → [] (gap — framework names are gap)
A: "Write the test migration plan." → ["DOCUMENT"] (gap for testing part; DOCUMENT fires on "plan" in documentation context? Actually "write the migration plan" → DOCUMENT. Let me reconsider.)
Actually: "Write the test migration plan." → ["DOCUMENT"] — "write" + "plan" in documentation context → DOCUMENT.
Teaching: Framework names (Cypress, Playwright, Selenium) alone → gap. Test-writing verbs + test targets → TEST.

---

## TU003-005 — IMPLEMENT+UI: building a UI component

A: "Build the accordion component." → ["IMPLEMENT", "UI"] — "build" → IMPLEMENT; "accordion component" → UI.
A: "Fix the accordion animation." → ["UI"] — "fix" alone + UI vocabulary → UI only (no IMPLEMENT).
Teaching: "build", "implement", "create" + UI component → IMPLEMENT+UI. "fix" alone → UI only (bug fix, not feature addition).
