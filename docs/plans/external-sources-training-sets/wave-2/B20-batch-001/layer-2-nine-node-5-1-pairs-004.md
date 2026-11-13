# Layer 2 — Nine-Node Flow Pairs (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N004 — UnitTestingFlow51: TaxCalculationService (EU VAT rates)

Request: "Validate the TaxCalculationService output for all EU VAT rate bands."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1 | TransitionType: feature_addition
  Evidence: "validate" always fires TEST regardless of context

### Node 2 — Context Assembly
  Service: TaxCalculationService | Domain: taxation/finance
  Dependencies identified: ICountryTaxRateRepository
  Testable units: calculateVat(amount, countryCode), getEffectiveRate(countryCode), applyExemption(orderId)

### Node 3 — Plan Generation
  Plan:
    3.1 calculateVat(100, "DE") → expect 19.00 (Germany standard 19%)
    3.2 calculateVat(100, "FR") → expect 20.00 (France standard 20%)
    3.3 calculateVat(100, "IE") → expect 23.00 (Ireland standard 23%)
    3.4 calculateVat(100, "XX") → expect Fail("UNKNOWN_COUNTRY_CODE")
    3.5 applyExemption for B2B order → expect zero VAT
    3.6 calculateVat with zero amount → expect 0.00

### Node 4 — Dependency Resolution
  Mocks: MockCountryTaxRateRepository (seeded: DE=19, FR=20, IE=23)
  Test framework: xUnit | Theory attribute for parameterized rates

### Node 5 — Implementation (Test Code)
  Tests produced: 12 | Theory tests cover 8 country codes; individual tests for exemption and zero-amount

### Node 6 — Verification
  All 12 pass | Rounding: Assert.Equal(expectedDecimal, result, 2) for 2-decimal precision

### Node 7 — Quality Gate
  Status: PASS | Precision gate: all results within 0.01 tolerance

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: TaxCalculationServiceTests.cs

### Node 9 — Result
  Result: PASS | Tests: 12 | Status: green
