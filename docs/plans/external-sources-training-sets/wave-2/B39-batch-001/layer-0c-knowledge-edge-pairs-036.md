# Layer 0-C — Knowledge Edge Pairs (036)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E036-001 — Complete TEST+UI reference: full gap vocabulary list

A: What is the complete gap verb list confirmed across Round 2 (CheckoutFlow through OrderTracking)?
B: Gap verbs confirmed: run, execute, automate, check, scan, configure, track, monitor, describe, profile, review, trigger, schedule, resize. None fire any predicate. These are the common false positives for TEST, IMPLEMENT, and QA in E2E/UI testing requests.

---

## E036-002 — Complete TEST+UI reference: UI lexicon tokens confirmed in Round 2

A: What UI lexicon tokens were confirmed as firing UI in Round 2?
B: Confirmed UI tokens: button, form, modal, page, input, screen, badge, toggle, panel, checkbox, dropdown, component, accordion, toast, sidebar. All confirmed across files 025–035. All fire UI on any occurrence per request.

---

## E036-003 — Complete TEST+UI reference: "grid" is not UI vocabulary

A: Is "results grid" a UI trigger?
B: No. "grid" is not in the UI lexicon. "results grid", "data grid", "product grid" — none fire UI. To fire UI, the request needs: "results screen", "results page", or another explicit UI lexicon word.

---

## E036-004 — Complete TEST+UI reference: "controls" is not UI vocabulary

A: Is "quantity controls" a UI trigger?
B: No. "controls" is not in the UI lexicon. UI requires explicit lexicon words: button, input, form, toggle, checkbox, slider, etc. "controls" is a generic term that does not map to the UI lexicon.

---

## E036-005 — Complete TEST+UI reference: compound noun rule

A: What is the rule for determining if a compound noun fires UI?
B: The head word (usually the final noun) determines the UI trigger. "credit card form" → head = "form" → UI fires. "wishlist badge" → head = "badge" → UI fires. "filter panel" → head = "panel" → UI fires. "search results" → head = "results" — not in UI lexicon → UI does NOT fire. Modifier words before the head are irrelevant to predicate firing.
