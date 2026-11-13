# Layer 2 — Nine-Node Flow Pairs (008)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N008 — UnitTestingFlow51: OrderValidationService (required fields + business rules)

Request: "Write tests for the OrderValidationService — all required fields and business rule violations."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition
  Evidence: "write tests" → TEST+IMPLEMENT

### Node 2 — Context Assembly
  Service: OrderValidationService | Domain: orders
  Dependencies identified: IProductRepository, ICustomerRepository
  Testable units: validate(order), checkRequiredFields(order), checkBusinessRules(order)

### Node 3 — Plan Generation
  Plan:
    3.1 validate complete order → expect Ok
    3.2 validate order with null customerId → expect Fail("CUSTOMER_ID_REQUIRED")
    3.3 validate order with empty items list → expect Fail("ORDER_MUST_HAVE_ITEMS")
    3.4 validate order with negative quantity → expect Fail("INVALID_ITEM_QUANTITY")
    3.5 validate order with out-of-stock product → expect Fail("PRODUCT_OUT_OF_STOCK")
    3.6 validate order with inactive customer → expect Fail("CUSTOMER_INACTIVE")

### Node 4 — Dependency Resolution
  Mocks: MockProductRepository (seeded with stock states), MockCustomerRepository

### Node 5 — Implementation (Test Code)
  Tests produced: 16 | Includes Theory for quantity boundary values (-1, 0, 1)

### Node 6 — Verification
  All 16 pass

### Node 7 — Quality Gate
  Status: PASS | All error codes verified exact string match

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: OrderValidationServiceTests.cs

### Node 9 — Result
  Result: PASS | Tests: 16 | Status: green
