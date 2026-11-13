# Layer 2 — Nine-Node Flow Pairs (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N005 — UnitTestingFlow51: InventoryService (concurrency + stock deduction)

Request: "Verify the InventoryService handles concurrent stock deductions correctly."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1 | TransitionType: feature_addition
  Evidence: "verify" → TEST; "concurrent" is domain gap (not a predicate trigger)

### Node 2 — Context Assembly
  Service: InventoryService | Domain: inventory management
  Dependencies identified: IStockRepository
  Testable units: deductStock(productId, quantity), getAvailableStock(productId), reserveStock(productId, quantity)

### Node 3 — Plan Generation
  Plan:
    3.1 deductStock(100) from stock of 200 → expect 100 remaining, Ok result
    3.2 deductStock(200) from stock of 100 → expect Fail("INSUFFICIENT_STOCK")
    3.3 deductStock(0) → expect Fail("INVALID_QUANTITY")
    3.4 reserveStock then deductStock → expect atomic reduction, no double-deduct
    3.5 getAvailableStock after reservation → expect reserved quantity excluded

### Node 4 — Dependency Resolution
  Mocks: MockStockRepository (thread-safe, seeded with initial stock values)
  Test framework: xUnit | No actual threading needed at unit level (behavior is mocked)

### Node 5 — Implementation (Test Code)
  Tests produced: 11 | Sequential mocks simulate concurrent scenario without actual thread contention

### Node 6 — Verification
  All 11 pass | Reservation-then-deduct atomicity confirmed via mock state assertions

### Node 7 — Quality Gate
  Status: PASS_WITH_WARNINGS
  Warning: actual thread-safety tested at integration level (5.2), not unit level

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: InventoryServiceTests.cs

### Node 9 — Result
  Result: PASS_WITH_WARNINGS | Tests: 11 | Warning: 1 (thread-safety scope)
