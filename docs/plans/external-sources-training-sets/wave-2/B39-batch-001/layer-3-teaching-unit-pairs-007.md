# Layer 3 — Teaching Unit Pairs (007)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU007-001 — UI fires on negative sentences

A: "The toggle switch does not maintain state after page navigation."
→ ["UI"] — "toggle switch" → UI. Negation does not suppress.
Teaching: Negation ("does not", "is not", "fails to") does not suppress UI predicate. Vocabulary presence fires it.

---

## TU007-002 — TEST fires in interrogative

A: "Have we written tests for the sidebar collapse behavior?"
→ ["TEST", "UI"] — "written tests" → TEST; "sidebar collapse behavior" → UI.
Teaching: Past tense + interrogative still fires TEST (test-writing vocabulary present). "sidebar collapse" → UI.

---

## TU007-003 — "render" fires UI in component context

A: "The skeleton loader does not render correctly during data fetch."
→ ["UI"] — "skeleton loader" + "render" → UI. "render" in component context → UI vocabulary.
Teaching: "render" + UI component vocabulary → UI predicate. "render PDF" or "render report" is not UI context.

---

## TU007-004 — Responsive vocabulary fires UI

A: "The navigation menu overflows at 320px viewport width."
→ ["UI"] — "navigation menu" + "320px viewport" (responsive context) → UI.
Teaching: "viewport width" + UI component vocabulary → UI predicate fires on responsive layout context.

---

## TU007-005 — Five-predicate: IMPLEMENT+TEST+UI+DOCUMENT+QA

A: "Build the notification badge component, write tests, fix badge positioning, document usage, QA sign-off."
→ ["IMPLEMENT", "TEST", "UI", "DOCUMENT", "QA"] — five predicates, five flows, QA last.
Teaching: Five distinct clauses → five predicates. No collapse. IMPLEMENT drives TransitionType (feature_addition).
