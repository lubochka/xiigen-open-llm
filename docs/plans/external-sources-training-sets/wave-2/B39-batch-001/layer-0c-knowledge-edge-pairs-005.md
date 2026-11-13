# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-005-001 — UI fires in negative sentences

A: "The dropdown does not render correctly on Safari."
Fires: ["UI"] — negative sentence does not suppress UI predicate. "dropdown" → UI.

A: "The button focus ring is not visible in high-contrast mode."
Fires: ["UI"] — negation does not suppress. "button focus ring" → UI.

Rule: UI predicate fires on vocabulary presence, not on positive sentiment.

---

## KE-005-002 — TEST fires in interrogative

A: "Have we written tests for the modal close behavior?"
Fires: ["TEST", "UI"] — "written tests" → TEST; "modal close behavior" → UI. Interrogative does not suppress.

A: "Are there E2E tests for the checkout flow?"
Fires: ["TEST"] — "E2E tests" → TEST (5.3). Interrogative fires TEST normally.

---

## KE-005-003 — "dashboard" is confirmed gap for UI predicate

A: "Fix the analytics dashboard layout."
Fires: [] — "dashboard" is confirmed gap vocabulary. Does not fire UI predicate.
Contrast: "Fix the sidebar layout" → ["UI"] — "sidebar" is in UI lexicon; "dashboard" is not.

Rule: dashboard → gap. sidebar, header, footer, nav, breadcrumb, pagination → UI.

---

## KE-005-004 — TEST deduplication: multiple "write tests" clauses

A: "Write unit tests for the utility and write E2E tests for the checkout form."
Fires: ["TEST"] — two TEST triggers collapse to one TEST predicate.
However: Sub-phase 5.1 (unit) and 5.3 (E2E) conflict. Resolve: 5.3 takes precedence when both unit and E2E present (system-level wins).
