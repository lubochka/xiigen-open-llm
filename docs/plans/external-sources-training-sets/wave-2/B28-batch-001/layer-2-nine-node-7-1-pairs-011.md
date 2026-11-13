# Layer 2 — Nine-Node Flow Pairs (011)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: nine_node_flow (QAGatingFlow73)

---

## B28-nine-011 — InventoryService v2 QA gate — PASS_WITH_WARNINGS

Request: "QA sign-off on the InventoryService v2 — validate stock reservation, release on order cancellation, and concurrent reservation handling."

### Flow: QAGatingFlow73

**Node 1**: Target: InventoryService v2 | Features: stock reservation, release, concurrency
**Node 2**: Gates: reservation, release on cancellation, concurrent safety, performance
**Node 3**: Gate 1 — Reservation: reserve 10 units → stock reduced atomically ✓
  Gate 2 — Release: cancel order → reserved units returned to stock ✓
  Gate 3 — Concurrency: 100 concurrent reservation requests for 50 units → exactly 50 succeed ✓ (pessimistic lock)
  Gate 4 — Performance: p99 reservation latency = 120ms → threshold 100ms ✗ WARNING (slightly over)
**Node 4**: No critical defects. Performance slightly over threshold under load (p99 = 120ms vs 100ms).
**Node 5**: Arbiter: ReleaseGateArbiter
**Node 6**: Coverage: reservation: PASS, release: PASS, concurrency: PASS, performance: WARNING
**Node 7**: PASS_WITH_WARNINGS — p99 latency at 120ms (threshold 100ms) under 100 concurrent users; acceptable for current load
**Node 8**: QA report: InventoryService-v2-QA.md | WARNING: performance ticket PERF-234 filed
**Node 9**: Verdict: PASS_WITH_WARNINGS — cleared with performance optimization ticket for next sprint

