# Layer 3 — Teaching Unit Pairs (005)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU005-001 — IMPLEMENT+TEST+UI+QA: full four-predicate with ordering

A: "Build the date picker, write interaction tests, fix calendar overlay z-index, and QA sign-off."
→ ["IMPLEMENT", "TEST", "UI", "QA"] | TransitionType: feature_addition | QA last
Teaching: Four clauses → four predicates. No collapse. QA always last. IMPLEMENT drives TransitionType.

---

## TU005-002 — Deduplication: two UI triggers → one UI predicate

A: "The button hover state and modal focus trap need fixing."
→ ["UI"] — two UI vocabulary triggers; one UI predicate (set, not multiset).
Teaching: Multiple UI triggers within one request → one UI predicate, one UIInteractionFlow15.

---

## TU005-003 — UI + DOCUMENT (Storybook documentation)

A: "Document the design system button component and its variants."
→ ["UI", "DOCUMENT"] — "button component" → UI; "document" → DOCUMENT. Two predicates.
Teaching: Documentation requests about UI components fire both UI and DOCUMENT. No IMPLEMENT (documenting existing component).

---

## TU005-004 — "fix" alone does not fire IMPLEMENT

A: "Fix the checkbox focus ring visibility in dark mode."
→ ["UI"] — "fix" + UI vocabulary → UI (bug fix). Does not fire IMPLEMENT.
Teaching: "fix" is not in the IMPLEMENT trigger lexicon. IMPLEMENT fires on feature-building verbs (build, implement, create, develop). "fix" fires UI when UI vocabulary present.

---

## TU005-005 — TEST+UI+QA: three-predicate for accessibility feature

A: "Write accessibility tests for the form controls, fix missing ARIA labels, QA sign-off."
→ ["TEST", "UI", "QA"] — three predicates, three flows, QA last.
Teaching: Accessibility improvements → TEST (for the tests) + UI (for the fix) + QA (for the gate). Not a single predicate.
