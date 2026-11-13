# Layer 2 — Nine-Node Flow Pairs (009)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N009 — UnitTestingFlow51: ShippingCostCalculator (weight brackets)

Request: "Generate test cases for the ShippingCostCalculator covering all weight bracket transitions."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition
  Evidence: "generate" → IMPLEMENT; "test cases" → TEST

### Node 2 — Context Assembly
  Service: ShippingCostCalculator | Domain: logistics/shipping
  Dependencies identified: ICarrierRateRepository
  Testable units: calculateCost(weightKg, destination), getBracket(weightKg), applyFuelSurcharge(baseCost)

### Node 3 — Plan Generation
  Plan:
    3.1 calculateCost(0.5kg, "UK") → £3.99 (under 1kg bracket)
    3.2 calculateCost(1.0kg, "UK") → £5.49 (1kg boundary, exact)
    3.3 calculateCost(1.1kg, "UK") → £7.99 (1-5kg bracket)
    3.4 calculateCost(5.0kg, "UK") → £7.99 (5kg boundary, same bracket)
    3.5 calculateCost(5.1kg, "UK") → £14.99 (5kg+ bracket)
    3.6 calculateCost(0, "UK") → Fail("INVALID_WEIGHT")
    3.7 calculateCost(1.0kg, "INVALID") → Fail("UNKNOWN_DESTINATION")

### Node 4 — Dependency Resolution
  Mocks: MockCarrierRateRepository (seeded with UK rate table)

### Node 5 — Implementation (Test Code)
  Tests produced: 14 | Theory for weight/bracket combinations; individual for error cases

### Node 6 — Verification
  All 14 pass | Boundary values at 1.0 and 5.0 kg explicitly covered

### Node 7 — Quality Gate
  Status: PASS | All bracket transitions covered

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: ShippingCostCalculatorTests.cs

### Node 9 — Result
  Result: PASS | Tests: 14 | Status: green
