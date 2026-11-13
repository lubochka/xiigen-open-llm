# Layer 0-C — Knowledge Edge Pairs (018)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E018-001 — "cross-browser" fires TEST when paired with test verb

A: "Write cross-browser tests for the product listing page layout."
B: ["TEST", "UI"] — "write cross-browser tests" → TEST (cross-browser tests compound); "product listing page" + "layout" → UI.
Edge: "cross-browser tests" is a TEST compound. "cross-browser" alone without a test noun/verb is gap.

---

## E018-002 — browser names (Chrome, Firefox, Safari, IE11) are gap

A: "Test the modal on Chrome, Firefox, and Safari."
B: ["TEST", "UI"] — "test" alone with the right verb context → TEST fires? "Test the modal" → TEST fires on "test" when used as a test-writing verb; "modal" → UI.
Edge: Browser names (Chrome, Firefox, Safari, IE11) are gap vocabulary. The predicate fires on "test" (as a verb) and "modal".
CompiledPredicateIds: ["TEST", "UI"]

---

## E018-003 — gap: "check compatibility" without test verb

A: "Check the browser compatibility table for CSS grid support."
B: [] — "check" → gap; "browser compatibility table" → gap; "CSS grid" → gap. Zero predicates.
Edge: "check" is operational gap. "compatibility" is gap. No predicate fires.

---

## E018-004 — "responsive" fires UI even in multi-browser context

A: "Validate the responsive layout of the navigation bar across Chrome and Firefox."
B: ["TEST", "UI"] — "validate" → TEST; "responsive layout" + "navigation bar" → UI (responsive, layout, nav). Chrome/Firefox → gap.
Edge: "responsive" reliably fires UI even when browser names dominate the surrounding context.

---

## E018-005 — "form input component" fires UI (three UI triggers, one predicate)

A: "Write cross-browser regression test cases for each form input component."
B: ["TEST", "UI"] — "write regression test cases" → TEST; "form input component" → UI (form fires, input fires, component fires — all deduplicated to one UI predicate).
Edge: Multiple UI vocabulary words in one phrase → one UI predicate (deduplication rule).
