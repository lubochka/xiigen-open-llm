# Layer 0-B — Router Pairs (014)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-014-001 — TEST+UI routes to 5.3 + 1.5 for Playwright login flow

A: ["TEST", "UI"] | "Write Playwright test cases for the login form: valid credentials, invalid password, locked-out account"
B: TEST → 5.3 (E2ETestingFlow53); UI → 1.5 (UIInteractionFlow15). Two flows.
Reason: "test cases" in E2E/Playwright context → 5.3; "login form" → UI → 1.5. Playwright is gap but test context confirms 5.3.

---

## C0B-014-002 — TEST+UI+IMPLEMENT → 5.3 + 1.5 + 4.1

A: ["TEST", "UI", "IMPLEMENT"] | "Implement the login page and write assertions for each validation error state"
B: TEST → 5.3; UI → 1.5; IMPLEMENT → 4.1. Three flows.
Reason: "write assertions" → TEST → 5.3 (UI test assertion context); "login page" + "input" → UI → 1.5; "implement" → IMPLEMENT → 4.1.

---

## C0B-014-003 — zero predicates: gap

A: [] | "Configure Playwright to retry flaky tests three times"
B: GapHandler. Zero predicates → knowledge_edge_synthetic.
Reason: "configure" → gap; "Playwright" → gap; "retry" → gap. No test-writing verb on its own without gap context.

---

## C0B-014-004 — TEST+UI: spec coverage for nav menu → 5.3 + 1.5

A: ["TEST", "UI"] | "Add spec coverage for the top navigation menu keyboard accessibility interactions"
B: TEST → 5.3; UI → 1.5. "spec coverage" → TEST → 5.3; "navigation menu" + "accessibility" → UI → 1.5.

---

## C0B-014-005 — TEST+UI: Playwright assertion for toast → 5.3 + 1.5

A: ["TEST", "UI"] | "Write Playwright assertions that validate the API response status shown in the error toast"
B: TEST → 5.3; UI → 1.5. "write assertions" + "validate" → TEST → 5.3; "error toast" → UI → 1.5.
Reason: Two test-writing verbs (write, validate) → both fire TEST but deduplicate to one predicate → one routing decision 5.3.
