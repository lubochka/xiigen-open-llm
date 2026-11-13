# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-004-001 — IMPLEMENT trigger edges in UI context

Trigger vocabulary → IMPLEMENT predicate:
- implement / build / create / develop / write (code context) / add (feature context)
- "implement the carousel" / "build the form" / "create the modal component"
- "add the toggle switch to the settings page" (feature addition context)

Gap vocabulary (do NOT fire IMPLEMENT):
- configure / set up / install / run / deploy → operational gap
- "fix a bug" alone → gap (unless "implement a fix" or "write a fix")
- framework names alone → gap

---

## KE-004-002 — TEST trigger edge: "add" disambiguation

Context: "add assertions" → TEST (test-writing verb)
Context: "add the button component" → IMPLEMENT+UI (feature addition)
Context: "add tests for the modal" → TEST (test-writing with target)
Rule: "add" fires TEST when followed by test vocabulary (assertions, tests, test cases, coverage); fires IMPLEMENT when followed by feature vocabulary (component, feature, functionality).

---

## KE-004-003 — "validate" disambiguation

Context: "validate that the button renders" → TEST (assertion in test context)
Context: "validate user input in the form" → IMPLEMENT (business logic validation, implementation task)
Context: "validate the API contract" → API_CONTRACT
Rule: "validate" fires TEST when in assertion/test sentence structure; fires other predicates in their domain contexts.

---

## KE-004-004 — UI predicate: "screen" and "page" vocabulary

"page" alone → context-dependent. "page" without UI vocabulary co-present → may not fire UI.
"screen" alone → context-dependent.
"login page layout" → UI (layout vocabulary present)
"checkout page API" → no UI (no layout/interaction/styling vocabulary)
Rule: "page" and "screen" fire UI only when co-present with UI-specific vocabulary (layout, component, style, interaction state).
