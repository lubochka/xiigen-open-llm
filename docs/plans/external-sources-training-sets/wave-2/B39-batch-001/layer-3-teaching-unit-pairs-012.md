# Layer 3 — Teaching Unit Pairs (012)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU012-001 — "popover" fires UI

A: "The user profile popover does not close when clicking outside."
→ ["UI"] — "user profile popover" → UI (component vocabulary).

---

## TU012-002 — "scroll" fires UI in UI context

A: "The infinite scroll does not load more products at the bottom of the page."
→ ["UI"] — "infinite scroll" + "load more products" → UI (scroll interaction vocabulary).

---

## TU012-003 — "sticky" fires UI

A: "The sticky header overlaps the page content on mobile."
→ ["UI"] — "sticky header" + "overlaps content" → UI (sticky positioning vocabulary).

---

## TU012-004 — "overflow" fires UI

A: "The dropdown list overflows below the viewport on small screens."
→ ["UI"] — "dropdown list overflows" + "viewport" → UI (overflow/layout vocabulary).

---

## TU012-005 — Full gap summary contrast

Gap requests (confirmed in B39):
- "Automate the test suite" → []
- "Set up Cypress CI" → []
- "Configure Playwright" → []
- "Run smoke tests" → []
- "Check the UI visually" → []
- "Fix the analytics dashboard" → []
- "Compare screenshots" → []
- "Quality assurance sign-off" → [] (spelled out)

Teaching: Each of these is gap because: operational verbs, framework names, or observational verbs without test-writing or QA abbreviation.
