# Layer 2 — Nine-Node Flow Pairs (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N003 — UnitTestingFlow51: CouponService (idempotency + boundary)

Request: "Assert the CouponService applies each coupon only once and rejects reuse."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1 | TransitionType: feature_addition

### Node 2 — Context Assembly
  Service: CouponService | Domain: promotions
  Dependencies identified: ICouponRepository, IOrderRepository
  Testable units: applyCoupon(orderId, couponCode), hasBeenUsed(couponCode, userId)

### Node 3 — Plan Generation
  Plan:
    3.1 applyCoupon with new coupon → expect Success, discount applied
    3.2 applyCoupon same coupon second time → expect Fail("COUPON_ALREADY_USED")
    3.3 applyCoupon with expired coupon → expect Fail("COUPON_EXPIRED")
    3.4 hasBeenUsed for unused coupon → expect false
    3.5 hasBeenUsed for used coupon → expect true

### Node 4 — Dependency Resolution
  Mocks: MockCouponRepository (returns seeded coupon state), MockOrderRepository
  Test framework: xUnit

### Node 5 — Implementation (Test Code)
  Tests produced: 10 | All using Arrange-Act-Assert pattern

### Node 6 — Verification
  All 10 tests pass | Idempotency assertion confirmed

### Node 7 — Quality Gate
  Status: PASS | 100% branch coverage on applyCoupon decision tree

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: CouponServiceTests.cs

### Node 9 — Result
  Result: PASS | Tests: 10 | Status: green
