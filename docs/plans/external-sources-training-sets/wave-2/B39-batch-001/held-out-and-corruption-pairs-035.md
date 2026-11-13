# Held-Out and Corruption Pairs (035)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H035-001 — Held-out: OrderTrackingTest — badge + component

A: "Create E2E test scenarios for the order tracking page — assert the status badge shows correct state, verify the timeline component renders each step."
  Expected: ["TEST", "UI"]
  Held-out: TEST (create, assert, verify). UI (order tracking page → page, status badge → badge, timeline component → component — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## H035-002 — Held-out: OrderTrackingTest — "track order" gap

A: "Track the order and check its delivery status."
  Expected: []
  Held-out: "track" is gap. "check" is gap. "order", "delivery status" — no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## H035-003 — Held-out: OrderTrackingTest — QA before release → 7.1

A: "Write order tracking E2E tests, assert the status badge, and QA sign-off before release."
  Expected: ["TEST", "UI", "QA"]
  Held-out: TEST (write, assert). UI (status badge → badge). QA (QA sign-off). QA → 7.1 (before release). AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.

---

## C035-001 — Corruption v6-02: "track" → TEST

Pattern: v6-02. Corrupt: "Track the order" → predicates = ["TEST"].
Correct: "track" is gap. TEST does not fire. predicates = [].

---

## C035-002 — Corruption v6-02: "check" → QA

Pattern: v6-02. Corrupt: "Check the delivery status" → predicates = ["QA"].
Correct: "check" is gap. QA fires only on literal "QA" abbreviation. predicates = [].
