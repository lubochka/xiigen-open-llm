# Layer 0-B — Router Pairs (037)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-037-001 — AdminPanelTest: routing [TEST, UI]

A: "Write E2E test cases for the admin panel page — assert the action button." Predicates [TEST, UI]. Routing?
B: TEST → 5.3 (E2ETestingFlow53): admin panel is a browser UI flow, not a unit test. UI → 1.5 (UIInteractionFlow15): "admin panel page" → page. PrimarySubPhaseId: 5.3. AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-037-002 — AdminPanelTest: sidebar navigation routing

A: "Create test coverage for the admin panel sidebar navigation." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (sidebar → 1.5; navigation → 1.5). Two UI triggers → one UI predicate → one routing decision. AllSubPhaseIds: [5.3, 1.5].

---

## C0B-037-003 — AdminPanelTest: [TEST, UI] with panel vocabulary

A: Does "admin panel" trigger TEST routing to 5.3 or 5.1?
B: 5.3. "admin panel" implies a browser UI context. Panel vocabulary + page vocabulary → confirms E2E/browser testing. 5.1 signals absent (no "unit test", no pure function, no service logic). TEST → 5.3 is confirmed.

---

## C0B-037-004 — AdminPanelTest: IMPLEMENT adds 4.1

A: [IMPLEMENT, TEST, UI] for "Build admin panel component and write tests." AllSubPhaseIds?
B: AllSubPhaseIds: [4.1, 5.3, 1.5]. IMPLEMENT → 4.1. TEST → 5.3. UI → 1.5. TransitionType: feature_addition. Order: 4.1 → 5.3 + 1.5 parallel.

---

## C0B-037-005 — AdminPanelTest: gap routing

A: "Review admin panel access logs." Predicates [UI]. AllSubPhaseIds?
B: AllSubPhaseIds: [1.5]. UI only → UIInteractionFlow15 (1.5). No TEST → no 5.3. No QA → no 7.x.
