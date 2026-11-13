# Layer 0-C — Knowledge Edge Pairs (014)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E014-001 — "test cases" fires TEST even alongside Playwright (gap)

A: "Write Playwright test cases for the login form."
B: ["TEST", "UI"] — "write test cases" → TEST; "login form" → UI. "Playwright" → gap.
Edge: "test cases" is a compound TEST noun. Playwright does not prevent TEST from firing.

---

## E014-002 — "assertions" fires TEST; "configure" does not fire IMPLEMENT

A: "Configure retry logic and write assertions for the login page error state."
B: ["TEST", "UI"] — "write assertions" → TEST; "login page" → UI (page). "configure" → gap (not in IMPLEMENT lexicon).
Edge: "configure" is operational vocabulary. IMPLEMENT does not fire.

---

## E014-003 — boundary: "page" alone fires UI without test context

A: "The login page has a two-column layout on desktop."
B: ["UI"] — "page" → UI (page); "layout" → UI (layout). No test-writing verb → no TEST. No IMPLEMENT verb → no IMPLEMENT.
Edge: UI fires on "page" even when no other predicates are present.

---

## E014-004 — boundary: "spec coverage" fires TEST

A: "Add spec coverage for the top navigation menu keyboard accessibility."
B: ["TEST", "UI"] — "add spec coverage" → TEST (compound: "spec" + "coverage" in test context); "navigation menu" → UI (nav); "keyboard accessibility" → UI (accessibility).
Edge: "spec coverage" is a recognized TEST compound. "add" serves as the writing verb.

---

## E014-005 — gap: "run Playwright tests" — "run" blocks TEST

A: "Run Playwright tests for the login page before deploying."
B: [] — "run" is operational gap; "Playwright" is gap; "before deploying" → gap. No test-writing verb.
Edge: "tests" co-occurring only with "run" and a framework name does not fire TEST. Test-writing verb required.
Correction: "tests" here is a noun used with a gap verb. Some analysts may argue TEST fires on "tests" alone — but the decision in B39 is that "run tests" is operational and does not fire TEST. CompiledPredicateIds: ["UI"] — "login page" fires UI (page).
Final: CompiledPredicateIds: ["UI"]
