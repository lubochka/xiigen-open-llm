# Layer 2 — Nine-Node Flow Pairs (005)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: nine_node_flow (QAGatingFlow73)

---

## B28-nine-005 — CheckoutService smoke test gate

Request: "Run QA smoke tests on the CheckoutService before the Black Friday deployment."

### Flow: QAGatingFlow73 (with E2EFlow53 for smoke tests)

**Node 1**: Target: CheckoutService | Context: pre-Black Friday deployment | Test type: smoke
**Node 2**: Smoke gates: cart, payment, order confirmation, email notification
**Node 3**: Gate 1 — Cart: add item → cart total correct ✓
  Gate 2 — Payment: stripe test card → payment processed ✓
  Gate 3 — Order confirmation: order record created, status = PENDING ✓
  Gate 4 — Email: confirmation email delivered within 30 seconds ✓
**Node 4**: All smoke gates pass. Performance: p99 checkout latency = 1.2s (within 2s threshold)
**Node 5**: Arbiter: ReleaseGateArbiter
**Node 6**: Smoke test coverage: 4 critical paths, 100% pass rate
**Node 7**: PASS — all smoke gates green; performance within threshold
**Node 8**: QA report: CheckoutService-BF-smoke-QA.md
**Node 9**: Verdict: PASS — cleared for Black Friday deployment

