# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-009-001 — UI fires on "empty state" and "placeholder" vocabulary

"empty state" / "empty state illustration" / "no results state" → UI
"placeholder text" / "input placeholder" → UI
"loading placeholder" / "skeleton screen" → UI
"404 page" / "error page" layout → UI (page layout vocabulary)

---

## KE-009-002 — UI fires on "card" and "tile" vocabulary

"product card" / "user card" / "card component" → UI
"tile layout" / "grid tile" → UI
"card hover state" / "card shadow" → UI
Gap: "credit card number" (data field, not UI component) → not UI unless in form context

---

## KE-009-003 — UI fires on "chip" and "badge" vocabulary

"chip" / "tag chip" / "filter chip" → UI
"badge" / "notification badge" / "count badge" → UI
"pill" / "status pill" → UI

---

## KE-009-004 — IMPLEMENT vs TEST: "fix" disambiguation

"fix the button focus ring" → UI (bug fix on UI element; not IMPLEMENT unless "implement a fix")
"fix and write regression tests" → TEST+UI (test-writing verb present)
"write a fix for the focus ring" → IMPLEMENT+UI ("write a fix" has implementation intent)
Rule: "fix" alone without test-writing or implementation verb → UI (if UI vocabulary present); does not fire TEST or IMPLEMENT.

---

## KE-009-005 — Compound predicates: "create and test"

"create the checkbox component and test its checked/unchecked states" → IMPLEMENT+TEST+UI
- "create" → IMPLEMENT; "test" in test-writing context → TEST; "checkbox" → UI.
"create test cases for the checkbox" → TEST+UI
- "create test cases" = test-writing verb → TEST; "checkbox" → UI; no IMPLEMENT (test creation not feature creation).
Rule: "create test cases/tests" → TEST; "create the component" → IMPLEMENT+UI.
