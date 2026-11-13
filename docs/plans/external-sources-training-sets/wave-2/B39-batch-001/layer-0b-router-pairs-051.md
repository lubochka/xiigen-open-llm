# Layer 0-B — Router Pairs (051)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-051-001 — LCP_FID_CLSTest: routing

A: "Create E2E test assertions for LCP < 2.5s, FID < 100ms, and CLS < 0.1 on the home page." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (home page → page). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-051-002 — LCP_FID_CLSTest: "check FID" [UI] only

A: "Check FID for the home page." AllSubPhaseIds?
B: AllSubPhaseIds: [1.5]. TEST did not fire (check is gap). UI → 1.5.

---

## C0B-051-003 — LCP_FID_CLSTest: all three metrics → still single test predicate

A: Writing tests for LCP, FID, and CLS in one request — how many TEST predicates?
B: One. Deduplication applies. Any number of metric targets in a single request → one TEST predicate → one routing decision to 5.3.

---

## C0B-051-004 — LCP_FID_CLSTest: QA before production → 7.1

A: "Write LCP/FID/CLS tests for checkout page and QA sign-off before production." AllSubPhaseIds?
B: [5.3, 1.5, 7.1]. QA → 7.1.

---

## C0B-051-005 — LCP_FID_CLSTest: routing is identical to non-performance tests

A: Does the performance context (LCP/FID/CLS) produce any routing difference from a standard E2E test?
B: No. Routing is identical: TEST → 5.3, UI → 1.5, QA → 7.3 or 7.1. Performance context only affects Layer 2 Node 07 arbiter selection. The predicate-to-sub-phase mapping is the same for all E2E tests.
