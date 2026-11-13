# Layer 0-D — Flow Factory Pairs (031)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F031-001 — SearchFlowTest: flow selection

A: "Write E2E test cases for the search page — assert results appear after typing in the search input, verify the no-results screen." Predicates [TEST, UI]. Flows?
B: E2ETestingFlow53 (5.3): TEST. UIInteractionFlow15 (1.5): UI ("search page" → page; "search input" → input; "no-results screen" → screen). Flows: [E2ETestingFlow53, UIInteractionFlow15]. TransitionType: documentation.

---

## F031-002 — SearchFlowTest: "scan" is gap

A: "Scan the search results for duplicate entries." Does TEST fire?
B: No. "scan" is gap — explicitly listed as a gap verb for TEST. "search results" does not contain UI vocabulary. Result: [] → knowledge_edge_synthetic. No flows. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## F031-003 — SearchFlowTest: autocomplete dropdown fires UI

A: "Assert the search autocomplete dropdown shows top 5 results." Full factory output?
B: TEST (assert). UI ("autocomplete dropdown" → dropdown). E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: documentation. TrustState: deterministic_generic.

---

## F031-004 — SearchFlowTest: [TEST, UI, QA] with gate before merge

A: "Write search E2E tests, verify the results grid, and QA gate before sprint merge." AllSubPhaseIds?
B: AllSubPhaseIds: [5.3, 1.5, 7.3]. QA → 7.3 ("gate before sprint merge" = feature acceptance). Order: 5.3 + 1.5 parallel → 7.3. TransitionType: documentation.

---

## F031-005 — SearchFlowTest: Node 1 domains

A: Node 1 domain output for SearchFlowTest.
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: search input (typing, debounce), autocomplete dropdown (results list, keyboard navigation), results grid (count, layout), no-results screen, search clear button, URL query parameter sync.
