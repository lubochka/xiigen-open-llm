# Held-Out and Corruption Pairs (019)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H019-001 — Held-out: TEST+UI — mobile E2E tests for sticky footer button

A: "Write end-to-end tests for the mobile checkout flow at 320px covering the sticky footer button."
  Expected: ["TEST", "UI"]
  Held-out: "write end-to-end tests" → TEST; "footer button" → UI (footer, button). "mobile" → gap alone; "320px" → gap.

---

## H019-002 — Held-out: UI only — "resize to see layout"

A: "Resize the browser to 375px to see how the layout looks."
  Expected: ["UI"]
  Held-out: "layout" → UI (layout); "resize" → gap; "see" → gap. Only UI fires.

---

## H019-003 — Held-out: TEST+UI — assert hamburger menu visible on mobile

A: "Assert that the hamburger menu navigation is visible and functional at 320px mobile viewport."
  Expected: ["TEST", "UI"]
  Held-out: "assert" → TEST; "navigation" → UI (nav); "visible" → UI (visual). "hamburger menu" → gap; "mobile viewport" → gap.

---

## C019-001 — Corruption v6-01: MOBILE predicate invented

A: "Write end-to-end tests for the mobile checkout flow at 320px."
  Correct: ["TEST"] (no UI component vocabulary in "checkout flow" at 320px without "button/screen/layout")
  Corrupt: ["TEST", "MOBILE"] ← v6-01 (MOBILE invented as predicate from "mobile" context)
  Teaching: "mobile" is not a predicate and does not fire UI. MOBILE is not a valid predicate in B39.

---

## C019-002 — Corruption v6-02: "resize" → IMPLEMENT (wrong)

A: "Resize the browser to 375px to see how the layout looks."
  Correct: ["UI"]
  Corrupt: ["UI", "IMPLEMENT"] ← v6-02 ("resize" incorrectly treated as IMPLEMENT — model sees "resize" as building/changing a UI)
  Teaching: "resize" is an operational/browser verb → gap. IMPLEMENT does not fire. Only "layout" fires UI.
