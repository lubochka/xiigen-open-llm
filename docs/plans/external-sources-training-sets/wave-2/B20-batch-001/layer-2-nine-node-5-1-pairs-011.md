# Layer 2 — Nine-Node Flow Pairs (011)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N011 — UnitTestingFlow51: LoyaltyPointsService (accrual + expiry)

Request: "Write unit tests for the LoyaltyPointsService covering point accrual and 12-month expiry."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition
  Evidence: "write unit tests" → TEST+IMPLEMENT

### Node 2 — Context Assembly
  Service: LoyaltyPointsService | Domain: rewards/CRM (Customer Relationship Management)
  Dependencies identified: IPointsRepository, IDateTimeProvider
  Testable units: accruePoints(userId, orderAmount), expirePoints(userId), getBalance(userId)

### Node 3 — Plan Generation
  Plan:
    3.1 accruePoints(£100 order) → expect +100 points (1 point per £1)
    3.2 accruePoints(£0 order) → expect Fail("INVALID_ORDER_AMOUNT")
    3.3 expirePoints where points are 13 months old → expect points zeroed
    3.4 expirePoints where points are 11 months old → expect no change
    3.5 getBalance after accrual → expect updated balance
    3.6 getBalance for unknown userId → expect Fail("USER_NOT_FOUND")

### Node 4 — Dependency Resolution
  Mocks: MockPointsRepository, MockDateTimeProvider (returns controlled "now" date)

### Node 5 — Implementation (Test Code)
  Tests produced: 12

### Node 6 — Verification
  All 12 pass | MockDateTimeProvider enables deterministic expiry tests

### Node 7 — Quality Gate
  Status: PASS | Expiry boundary (11 months vs 13 months) explicitly verified

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: LoyaltyPointsServiceTests.cs

### Node 9 — Result
  Result: PASS | Tests: 12 | Status: green
