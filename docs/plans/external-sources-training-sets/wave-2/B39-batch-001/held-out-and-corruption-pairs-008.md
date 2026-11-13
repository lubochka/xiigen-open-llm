# Held-Out and Corruption Pairs (008)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H008-001 — Held-out: UI — "sticky header"

A: "The sticky header overlaps the anchor link target when navigating in-page."
  Expected: ["UI"]
  Held-out: "sticky header" + "anchor link" → UI (sticky positioning + navigation vocabulary).

---

## H008-002 — Held-out: IMPLEMENT+TEST+UI — "build + write tests"

A: "Build the breadcrumb component and write tests for the back-navigation path."
  Expected: ["IMPLEMENT", "TEST", "UI"]
  Held-out: "build" → IMPLEMENT; "write tests" → TEST (5.3); "breadcrumb component" → UI. Three predicates.

---

## H008-003 — Held-out: gap — "Storybook configuration"

A: "Configure Storybook for the design system components."
  Expected: []
  Held-out: "configure Storybook" → operational gap (framework configuration verb + tool name). Gap.

---

## C008-001 — Corruption: "add" → IMPLEMENT (wrong when followed by test vocabulary)

A: "Add E2E tests for the filter component."
  Correct: ["TEST", "UI"]
  Corrupt: ["IMPLEMENT", "UI"] ← wrong ("add" incorrectly triggers IMPLEMENT instead of TEST)
  Teaching: "add" fires TEST when followed by test vocabulary (tests, test cases, assertions). "add the filter component" fires IMPLEMENT+UI. Context determines which predicate "add" fires.

---

## C008-002 — Corruption v6-01: ACCESSIBILITY invented

A: "Write accessibility tests for the form controls."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST", "UI", "ACCESSIBILITY"] ← v6-01 (ACCESSIBILITY is not a valid predicate)
  Teaching: ACCESSIBILITY is not a predicate. Accessibility vocabulary fires UI (component-level) and TEST (when test-writing verb present).
