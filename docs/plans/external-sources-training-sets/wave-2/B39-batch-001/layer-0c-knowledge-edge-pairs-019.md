# Layer 0-C — Knowledge Edge Pairs (019)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E019-001 — "mobile" alone does not fire UI; "screen" fires UI

A: "Write E2E tests for the mobile checkout flow at 320px and 375px."
B: ["TEST", "UI"] — "write E2E tests" → TEST; "mobile" → gap alone (not in UI lexicon); "320px" → gap; "375px" → gap. Is there UI vocabulary? "checkout flow" — "checkout" is not UI. "footer button" (from the full prompt) → UI (button). "screen" (if mentioned) → UI.
Edge: "mobile" is not in the UI lexicon. UI fires when "screen" or "button" or "layout" is also present. Mobile context alone is insufficient.

---

## E019-002 — "responsive grid layout" fires UI (three terms)

A: "Implement the responsive grid layout for the product listing."
B: ["UI", "IMPLEMENT"] — "implement" → IMPLEMENT; "responsive grid layout" → UI (responsive fires, layout fires — grid is gap). "product listing" → gap.
Edge: "responsive" and "layout" each independently fire UI. "grid" is not in the UI lexicon.
CompiledPredicateIds: ["UI", "IMPLEMENT"]

---

## E019-003 — "layout" fires UI without test verb

A: "Resize the browser to 375px to see how the layout looks."
B: ["UI"] — "layout" → UI; "resize" → gap; "see" → gap; "375px" → gap. Only UI fires.
Edge: UI fires on "layout" even when no test or implementation verb is present.

---

## E019-004 — "hamburger menu navigation" fires UI via "navigation"

A: "Assert that the hamburger menu navigation is visible at 320px."
B: ["TEST", "UI"] — "assert" → TEST; "hamburger menu navigation" → UI (nav); "visible" → UI (visual). "hamburger" → gap alone, but "navigation" in the same phrase fires UI.
Edge: "hamburger menu" does not independently fire UI. The word "navigation" (or "nav") fires UI.

---

## E019-005 — "tap targets" fires UI via button/interaction vocabulary

A: "Verify image scaling and tap targets on mobile product card."
B: ["TEST", "UI"] — "verify" → TEST; "image scaling" → UI (visual); "tap targets" → UI (button/interaction area); "product card" → UI (component).
Edge: "tap targets" is treated as button/interaction vocabulary → fires UI.
