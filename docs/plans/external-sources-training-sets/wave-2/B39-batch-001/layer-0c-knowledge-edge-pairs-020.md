# Layer 0-C — Knowledge Edge Pairs (020)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E020-001 — "accessibility" fires UI (confirmed in B39 lexicon)

A: "Write accessibility audit tests for the modal, form inputs, and navigation."
B: ["TEST", "UI"] — "write accessibility audit tests" → TEST; "modal", "form inputs", "navigation" → UI; "accessibility" → UI (accessibility is in UI lexicon).
Edge: "accessibility" is confirmed in the UI lexicon. It fires UI regardless of surrounding test/implementation context.

---

## E020-002 — "aria-label" fires UI (ARIA vocabulary is UI)

A: "Assert that every button has a non-empty aria-label attribute."
B: ["TEST", "UI"] — "assert" → TEST; "button" → UI; "aria-label" → UI (accessibility/ARIA vocabulary is UI).
Edge: ARIA attribute names (aria-label, aria-live, aria-selected, aria-expanded, aria-describedby) all fire UI predicate.

---

## E020-003 — gap: "review accessibility report" — "review" is gap

A: "Review the accessibility report from the last axe-core scan."
B: [] — "review" → gap; "accessibility report" → gap (report alone); "axe-core" → gap (tool name). Zero predicates.
Edge: "review" is consistently gap in B39 for all predicates. Even in accessibility context, "review report" produces zero predicates.

---

## E020-004 — "keyboard navigation" fires UI

A: "Validate that the dropdown component supports full keyboard navigation."
B: ["TEST", "UI"] — "validate" → TEST; "dropdown component" → UI (component, dropdown); "keyboard navigation" → UI (accessibility/keyboard interaction vocabulary).
Edge: "keyboard navigation" is in the UI accessibility vocabulary. It fires UI.

---

## E020-005 — "screen reader" fires UI

A: "Create test cases that verify screen reader announcements for each modal state."
B: ["TEST", "UI"] — "create test cases" + "verify" → TEST; "modal" → UI (modal); "screen reader" → UI (accessibility). "announcements" → gap.
Edge: "screen reader" fires UI as accessibility vocabulary. This is confirmed by its presence in the UI accessibility sub-lexicon.
