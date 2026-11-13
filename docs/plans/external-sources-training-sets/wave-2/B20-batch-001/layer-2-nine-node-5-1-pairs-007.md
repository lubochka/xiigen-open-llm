# Layer 2 — Nine-Node Flow Pairs (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N007 — UnitTestingFlow51: RefundService (partial + full refund logic)

Request: "Assert the RefundService processes partial refunds correctly and blocks refunds exceeding the original amount."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1 | TransitionType: feature_addition
  Evidence: "assert" → TEST

### Node 2 — Context Assembly
  Service: RefundService | Domain: payments/orders
  Dependencies identified: IOrderRepository, IPaymentGateway
  Testable units: processRefund(orderId, amount), validateRefundAmount(orderId, amount), getRefundableBalance(orderId)

### Node 3 — Plan Generation
  Plan:
    3.1 processRefund(£50) on order worth £100 → expect Ok, balance £50 remaining
    3.2 processRefund(£100) on order worth £100 → expect Ok, balance £0
    3.3 processRefund(£101) on order worth £100 → expect Fail("REFUND_EXCEEDS_ORDER_VALUE")
    3.4 processRefund on non-existent orderId → expect Fail("ORDER_NOT_FOUND")
    3.5 getRefundableBalance after partial refund → expect reduced balance

### Node 4 — Dependency Resolution
  Mocks: MockOrderRepository (seeded with order £100), MockPaymentGateway (records refund calls)

### Node 5 — Implementation (Test Code)
  Tests produced: 11

### Node 6 — Verification
  All 11 pass | MockPaymentGateway.RefundCalled asserted in full-refund test

### Node 7 — Quality Gate
  Status: PASS | Boundary condition at exact order value covered

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: RefundServiceTests.cs

### Node 9 — Result
  Result: PASS | Tests: 11 | Status: green
