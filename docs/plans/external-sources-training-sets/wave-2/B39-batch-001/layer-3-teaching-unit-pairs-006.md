# Layer 3 — Teaching Unit Pairs (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU006-001 — "set up" is gap (not IMPLEMENT)

A: "Set up Playwright for automated UI testing."
→ [] — "set up" is operational gap; "Playwright" is framework name → gap.
Teaching: IMPLEMENT does not fire on "set up". Framework configuration is operational gap.

---

## TU006-002 — "check" is not a test verb

A: "Check the UI for visual regressions before release."
→ [] — "check" is observational gap. No test-writing verb and no QA abbreviation.
Teaching: "check", "review", "inspect", "look at" are observational gap verbs. Not TEST triggers.

---

## TU006-003 — "create test cases" fires TEST (not IMPLEMENT)

A: "Create test cases for the accordion expand/collapse behavior."
→ ["TEST", "UI"] — "create test cases" → TEST (test-writing intent); "accordion" → UI.
Teaching: "create test cases" is a test-writing verb form. It fires TEST, not IMPLEMENT.

---

## TU006-004 — "add assertions" fires TEST

A: "Add assertions for the tooltip positioning on overflow."
→ ["TEST", "UI"] — "add assertions" → TEST; "tooltip positioning" → UI.
Teaching: "add assertions" is equivalent to "write tests" as a test-writing verb. Fires TEST.

---

## TU006-005 — "verify" in test context fires TEST

A: "Verify that the progress bar animates from 0% to 100%."
→ ["TEST", "UI"] — "verify" (assertion verb in test context) → TEST; "progress bar animates" → UI.
Teaching: "verify" fires TEST when used as a test assertion. Contrast with "verify" in operational context (e.g., "verify the deployment") which is gap.
