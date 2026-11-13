# Layer 0-C — Knowledge Edge Pairs (015)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E015-001 — "visual regression tests" fires TEST; "baseline" is gap alone

A: "Write visual regression tests for the component library button baselines."
B: ["TEST", "UI"] — "write visual regression tests" → TEST (visual regression test compound); "component library button" → UI (component, button). "baselines" → gap alone.
Edge: "visual regression tests" is a recognized TEST compound spanning multiple words.

---

## E015-002 — "snapshot" fires TEST when paired with test-writing verb

A: "Create snapshot tests that capture the visual baseline for the product card."
B: ["TEST", "UI"] — "create snapshot tests" → TEST; "product card" → UI (component).
Edge: "snapshot" alone is gap but "snapshot tests" or "snapshot assertions" fires TEST.

---

## E015-003 — gap: "update snapshots" — "update" is gap, not test-writing

A: "Update the visual snapshots after the theme change."
B: ["UI"] — "update" → gap (not in IMPLEMENT or TEST lexicon for this use); "visual snapshots" alone with gap verb → no TEST. No UI component vocabulary explicitly either — "visual" is in UI lexicon → UI fires.
Edge: "update" is a consistent gap word for both IMPLEMENT and TEST in B39.

---

## E015-004 — "assert" fires TEST for snapshot comparison

A: "Assert that the modal component snapshot matches the approved baseline after the padding update."
B: ["TEST", "UI"] — "assert" → TEST; "modal component" → UI (modal, component).
Edge: "assert" is a valid TEST trigger even in snapshot/visual-regression context.

---

## E015-005 — boundary: QA fires ONLY on "QA" abbreviation — not on "quality check"

A: "Run a quality check on the visual regression baseline approvals."
B: [] — "quality check" → gap (full-form quality vocabulary does not fire QA predicate). "run" → gap. "visual regression" → gap without test-writing verb.
Edge: QA fires on "QA" abbreviation only. "quality check", "quality review", "quality assurance" are all gap.
CompiledPredicateIds: []
