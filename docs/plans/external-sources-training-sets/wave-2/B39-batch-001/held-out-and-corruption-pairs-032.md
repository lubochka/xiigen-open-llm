# Held-Out and Corruption Pairs (032)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H032-001 — Held-out: FilterFlowTest — panel + badge + checkbox

A: "Write E2E test scenarios for the filter panel — assert filter checkboxes narrow results, verify the active filter badge count."
  Expected: ["TEST", "UI"]
  Held-out: TEST (write, assert, verify). UI (filter panel → panel, filter checkboxes → checkbox, active filter badge → badge — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## H032-002 — Held-out: FilterFlowTest — "configure" gap

A: "Configure the filter options for the product listing."
  Expected: []
  Held-out: "configure" is gap. "filter options", "product listing" — no UI lexicon words. Zero predicates → knowledge_edge_synthetic.

---

## H032-003 — Held-out: FilterFlowTest — clear filters button + checkbox

A: "Add test coverage to verify the clear filters button resets all active filter checkboxes."
  Expected: ["TEST", "UI"]
  Held-out: TEST ("add test coverage" = test-writing, "verify"). UI (clear filters button → button, active filter checkboxes → checkbox — two triggers → one UI). Result: [TEST, UI]. TransitionType: documentation (no IMPLEMENT).

---

## C032-001 — Corruption v6-02: "configure" → IMPLEMENT

Pattern: v6-02. Corrupt: "Configure filter options" → predicates = ["IMPLEMENT"].
Correct: "configure" is gap. predicates = []. IMPLEMENT requires build/implement/create/add[feature noun].

---

## C032-002 — Corruption v6-03: UI routing collapsed into 5.3

Pattern: v6-03. Corrupt: "Assert filter panel — UI sub-phase routed to 5.3."
Correct: UI always routes to 1.5. TEST routes to 5.3. UI routing to 5.3 is always incorrect for B39.
