# Layer 3 — Teaching Unit Pairs (019)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU019-001 — "mobile" does not fire UI alone; "screen" does

A: "Write tests for the mobile checkout flow." Does UI fire?
B: TEST fires on "write tests". "mobile" → gap alone (not in UI lexicon). "checkout flow" → gap (no UI component). UI does not fire.
Result: ["TEST"]. Compare: "Write tests for the mobile screen layout" → ["TEST", "UI"] ("screen" and "layout" fire UI).
Teaching: "mobile" is not in the UI lexicon. UI fires on specific component/layout vocabulary, not on platform names.

---

## TU019-002 — "responsive grid layout" fires UI via two triggers

A: "Implement the responsive grid layout." What predicates fire?
B: IMPLEMENT fires on "implement". UI fires on "responsive" (in UI lexicon) and "layout" (in UI lexicon). "grid" → gap alone.
Result: ["UI", "IMPLEMENT"]. Teaching: Two UI triggers → one UI predicate. "grid" is gap (CSS grid as a layout system is not in the UI predicate lexicon).

---

## TU019-003 — "resize" is gap; "layout" fires UI alone

A: "Resize the browser to 375px to see how the layout looks." What predicates fire?
B: UI fires on "layout". "resize" is gap. "see" is gap. Only UI fires.
Result: ["UI"]. Teaching: UI fires on "layout" even with a gap action verb. UI does not require a test or implementation verb.

---

## TU019-004 — "hamburger menu" is gap; "navigation" fires UI

A: "Assert that the hamburger menu navigation is visible at 320px." What fires?
B: TEST fires on "assert". UI fires on "navigation" (nav is in UI lexicon). "hamburger menu" alone → gap; "navigation" in the same phrase confirms UI. "visible" → UI (visual state vocabulary).
Result: ["TEST", "UI"]. Teaching: "hamburger menu" does not fire UI. The word "navigation" (or "nav") fires UI.

---

## TU019-005 — "tap targets" fire UI; "image scaling" fires UI

A: "Verify image scaling and tap targets on mobile product card." What fires?
B: TEST fires on "verify". UI fires on "image scaling" (visual), "tap targets" (button/interaction area → UI), "product card" (component → UI).
Result: ["TEST", "UI"]. Teaching: "tap targets" is treated as button/touch-interaction vocabulary and fires UI.
