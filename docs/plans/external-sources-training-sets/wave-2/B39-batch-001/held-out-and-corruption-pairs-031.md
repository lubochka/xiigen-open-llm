# Held-Out and Corruption Pairs (031)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H031-001 — Held-out: SearchFlowTest — autocomplete + results

A: "Write E2E test cases for the search page — assert the autocomplete dropdown shows top 5, verify the results screen renders after submit."
  Expected: ["TEST", "UI"]
  Held-out: TEST (write, assert, verify). UI (search page → page, autocomplete dropdown → dropdown, results screen → screen — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## H031-002 — Held-out: SearchFlowTest — "scan" gap

A: "Scan the search results for performance bottlenecks."
  Expected: []
  Held-out: "scan" is gap. "search results" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## H031-003 — Held-out: SearchFlowTest — [TEST, UI, QA] before sprint merge

A: "Write search E2E tests, assert the search input debounce behavior, and QA gate before sprint merge."
  Expected: ["TEST", "UI", "QA"]
  Held-out: TEST (write, assert). UI (search input → input). QA (QA gate). QA → 7.3 ("before sprint merge" = acceptance). AllSubPhaseIds: [5.3, 1.5, 7.3]. TransitionType: documentation.

---

## C031-001 — Corruption v6-01: SEARCH predicate invented

Pattern: v6-01. Corrupt: predicates = ["TEST", "UI", "SEARCH"].
Correct: predicates = ["TEST", "UI"]. "SEARCH" is a domain term, not a predicate.

---

## C031-002 — Corruption v6-02: "scan" → TEST

Pattern: v6-02. Corrupt: "Scan search results" → predicates = ["TEST"].
Correct: "scan" is gap. Zero predicates fire unless UI vocabulary is also present. predicates = [].
