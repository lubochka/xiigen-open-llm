# Held-Out and Corruption Pairs (024)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H024-001 — Held-out: complete predicate reference — dense signal case

A: "Write accessibility e2e test cases for the modal ARIA roles and navigation keyboard interactions, assert WCAG 2.1 AA compliance, and QA sign-off before merge."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "write e2e test cases" + "assert" → TEST (deduplicated); "modal" + "navigation" + "ARIA roles" + "keyboard interactions" + "WCAG 2.1 AA" → UI (all accessibility/component vocabulary, deduplicated); "QA sign-off" → QA.
  Routing: [5.3, 1.5, 7.3]. TransitionType: documentation.

---

## H024-002 — Held-out: gap — only framework name present with run verb

A: "Run Selenium tests for the checkout form."
  Expected: ["UI"] (form fires UI; "run" and "Selenium" are gap; but "checkout form" → form → UI)
  Held-out: UI fires on "checkout form" (form). TEST does not fire — "run" is gap. "Selenium" is gap. Only UI fires.
  Result: ["UI"].

---

## H024-003 — Held-out: IMPLEMENT+TEST+UI → feature_addition (IMPLEMENT overrides)

A: "Build the accessibility test harness, write ARIA assertion tests for every modal component, and QA sign-off."
  Expected: ["TEST", "UI", "IMPLEMENT", "QA"]
  Held-out: "build" → IMPLEMENT; "write ARIA assertion tests" → TEST; "modal component" + "ARIA" → UI; "QA sign-off" → QA.
  TransitionType: feature_addition (IMPLEMENT present). Routing: [4.1, 5.3, 1.5, 7.3].

---

## C024-001 — Complete corruption summary: v6-01 pattern (invented predicates)

Pattern: v6-01 corruptions invent new predicates from modifier words.
Examples seen in B39-013 to 024: PLAYWRIGHT, BDD, VISUAL_REGRESSION, MOBILE, PERFORMANCE, BROWSER, ACCESSIBILITY, TEST_AUTOMATION.
Rule: No predicate may be invented from: framework names, methodology names, test type modifiers, platform names, tool names. Only the seven valid predicates exist: TEST, UI, QA, IMPLEMENT, DOCUMENT, ARCHITECTURE, REFACTOR.

---

## C024-002 — Complete corruption summary: v6-02 pattern (gap verb → predicate)

Pattern: v6-02 corruptions fire predicates from gap verbs.
Examples: "run" → TEST, "check" → TEST or QA, "update" → IMPLEMENT, "configure" → IMPLEMENT, "profile" → TEST, "review" → QA, "resize" → IMPLEMENT, "describe" → TEST.
Rule: Gap verbs (run, check, update, configure, profile, review, resize, describe, automate, monitor, scan) do not fire any predicate. Predicates fire only on the designated lexicon.
