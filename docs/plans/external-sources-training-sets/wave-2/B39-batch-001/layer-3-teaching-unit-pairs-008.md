# Layer 3 — Teaching Unit Pairs (008)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU008-001 — "cover" fires TEST in writing context

A: "Add test coverage for the file upload error states."
→ ["TEST", "UI"] — "add test coverage" → TEST (writing intent); "file upload error states" → UI.
Teaching: "add/increase test coverage" with feature target → TEST. "view test coverage report" → gap.

---

## TU008-002 — "icon" fires UI

A: "The loading spinner icon jitters on Safari 17."
→ ["UI"] — "loading spinner icon" → UI predicate. Browser context doesn't change predicate.
Teaching: "icon" in visual/rendering context → UI predicate. Confirmed UI vocabulary.

---

## TU008-003 — "z-index" fires UI

A: "Fix the cookie consent banner z-index — it's hidden behind the product modal."
→ ["UI"] — "cookie consent banner z-index", "product modal" → UI (layout/stacking vocabulary).
Teaching: CSS stacking context vocabulary (z-index, overlay, position, stacking order) → UI predicate.

---

## TU008-004 — "animation" fires UI

A: "The page transition animation causes layout shift on Chrome."
→ ["UI"] — "page transition animation" + "layout shift" → UI (animation + layout vocabulary).
Teaching: "animation", "transition", "keyframe" in browser/CSS context → UI predicate.

---

## TU008-005 — SECURITY+TEST+UI: XSS in form context

A: "Write tests for XSS prevention in the comment input and fix input sanitization display."
→ ["SECURITY", "TEST", "UI"] — "XSS prevention" → SECURITY; "write tests" → TEST; "comment input" + "sanitization display" → UI.
Teaching: Security testing in UI context → three predicates. SECURITY does not subsume TEST or UI.
