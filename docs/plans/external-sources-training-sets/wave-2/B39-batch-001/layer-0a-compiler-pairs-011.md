# Layer 0-A — Compiler Pairs (011)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-011-001 — UI: "skeleton loader" vocabulary

A: "Add a skeleton loader to the product listing page while data fetches."
B: ["IMPLEMENT", "UI"] — "add" in implementation context → IMPLEMENT; "skeleton loader" → UI.
CompiledPredicateIds: ["IMPLEMENT", "UI"]

---

## C0A-011-002 — TEST+UI: loading state tests

A: "Write tests for the skeleton loader display and transition to content."
B: ["TEST", "UI"] — "write tests" → TEST (5.3); "skeleton loader display and transition" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-011-003 — gap: "Storybook"

A: "Document the button component in Storybook."
B: ["UI", "DOCUMENT"] — "button component" → UI; "document" → DOCUMENT. "Storybook" is a tool name (gap as standalone), but the verb "document" fires DOCUMENT.
CompiledPredicateIds: ["UI", "DOCUMENT"]

---

## C0A-011-004 — UI: "z-index" / "overlay" vocabulary

A: "The cookie consent overlay has a lower z-index than the modal."
B: ["UI"] — "cookie consent overlay", "z-index", "modal" → UI predicate (layout/stacking vocabulary).
CompiledPredicateIds: ["UI"]

---

## C0A-011-005 — TEST+UI+QA: three-predicate summary

A: "Write interaction tests for the cookie overlay, fix z-index stacking, and QA sign-off."
B: ["TEST", "UI", "QA"] — "write interaction tests" → TEST; "z-index stacking", "cookie overlay" → UI; "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
