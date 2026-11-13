# Layer 2 — Nine-Node Flow Pairs (041)
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N041 — UnitTestingFlow51: CurrencyFormatter

Request: "Write unit tests for the CurrencyFormatter covering GBP (Great British Pound), EUR (Euro), and negative amounts."

### Node 01 — SelectedLogicalTransition
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition

### Node 02 — Context Assembly
  Service: CurrencyFormatter | Domain: utility/formatting
  Dependencies identified: ICurrencyConfig (injectable locale settings)
  Testable units: Format(decimal amount, string currencyCode), FormatNegative(decimal amount, string currencyCode)

### Node 03 — Plan Generation
  3.1 Format 1000 GBP (Great British Pound) → expect "£1,000.00"
  3.2 Format 1000.50 EUR (Euro) → expect "€1,000.50"
  3.3 Format -250.00 USD (US Dollar) → expect "-$250.00"
  3.4 Format 0.00 for any currency → expect "£0.00" / "€0.00" / "$0.00"
  3.5 Unknown currency code → expect UnknownCurrencyException

### Node 04 — Dependency Resolution
  Mock: MockCurrencyConfig. Test framework: xUnit. Assertions: Assert.Equal, Assert.Throws

### Node 05 — Implementation (Test Code)
  Tests produced: 9 | Framework: xUnit
  Coverage: GBP (Great British Pound) format (2 cases), EUR (Euro) format (2 cases), USD (US Dollar) negative (2 cases), zero/exception (3 cases)

### Node 06 — Verification
  All 9 tests pass | No skipped tests

### Node 07 — Quality Gate
  Status: PASS | Coverage: 92% statement coverage

### Node 08 — Output Assembly
  OutputType: TestSuite | Files: CurrencyFormatterTests.cs

### Node 09 — Result
  Result: PASS | Tests: 9 | Status: green
