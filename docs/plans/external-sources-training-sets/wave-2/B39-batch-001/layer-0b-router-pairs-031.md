# Layer 0-B — Router Pairs (031)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-031-001 — SearchFlowTest: routing decision

A: "Write E2E test scenarios for the search flow — assert the search input, verify the results list." Predicates: [TEST, UI]. Route.
B: TEST → 5.3 (E2ETestingFlow53): search flow involves browser-level interaction. UI → 1.5 (UIInteractionFlow15): "search input" → "input" fires UI → 1.5. "results list" → "list" is not in UI lexicon; only "input" fires UI here. TransitionType: documentation.

---

## C0B-031-002 — SearchFlowTest: autocomplete dropdown routing

A: "Create test cases for the search autocomplete dropdown — assert keyboard arrow navigation." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("search autocomplete dropdown" → "dropdown" fires UI → 1.5). Both UI triggers ("dropdown" and keyboard interaction vocabulary do not fire UI — "keyboard navigation" is not in UI lexicon by itself, but "dropdown" is sufficient). Two sub-phases: [5.3, 1.5].

---

## C0B-031-003 — SearchFlowTest: execute gap — UI only routing

A: "Execute search E2E tests for the keyword input component." Predicates: [UI]. Route.
B: UI → 1.5 ("keyword input component" → "input" and "component" fire UI → 1.5). No TEST (execute is gap). Single sub-phase: 1.5. TrustState: deterministic_generic.

---

## C0B-031-004 — SearchFlowTest: IMPLEMENT routing

A: "Implement the search results component, write snapshot tests for the empty state." Predicates: [IMPLEMENT, TEST, UI]. Route.
B: IMPLEMENT → 4.1. TEST → 5.3 ("write snapshot tests" → snapshot test fires TEST at 5.3). UI → 1.5 ("search results component" → "component" fires UI → 1.5). TransitionType: feature_addition.

---

## C0B-031-005 — SearchFlowTest: QA routing with release vocabulary

A: "Write search flow E2E test coverage, assert the filter panel updates, and QA review before the release." Predicates: [TEST, UI, QA]. Route QA.
B: QA → 7.3 or 7.1? "QA review before the release" — "release" vocabulary present → QA → 7.1 (ReleaseGateFlow71). TEST → 5.3. UI → 1.5 ("filter panel", "results component" → "panel", "component" fire UI). AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.
