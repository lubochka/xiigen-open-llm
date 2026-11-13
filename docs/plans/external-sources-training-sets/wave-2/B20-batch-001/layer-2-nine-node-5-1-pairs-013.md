# Layer 2 — Nine-Node Flow Pairs (013)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — CartCalculationTests | TEST alone
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the CartCalculationService covering item totals, quantity adjustments, and empty-cart edge cases."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Target: CartCalculationService — computes cart subtotals, applies per-item pricing, handles quantity changes.
Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

A: flow_input: "Write unit tests for CartCalculationService." | node: DomainSplitterUnit
B:
  DomainBoundaries:
    - domain: "cart_item_pricing" | scope: "Per-item total: price × quantity"
    - domain: "cart_aggregate" | scope: "Cart subtotal, tax-base calculation"
    - domain: "cart_edge_states" | scope: "Empty cart, zero-quantity items, negative quantity guard"
  SplitStrategy: "Split by calculation responsibility: item-level vs cart-level vs edge states"

---

## Node 02 — DomainTranslatorUnit

A: domain_buckets: ["cart_item_pricing", "cart_aggregate", "cart_edge_states"] | node: DomainTranslatorUnit
B:
  TranslatedTestBoundaries:
    - source_domain: "cart_item_pricing" | test_boundary: "CalculateItemTotal(price, qty)" | test_type: "unit — pure function"
    - source_domain: "cart_aggregate" | test_boundary: "CalculateSubtotal(items)" | test_type: "unit — aggregate"
    - source_domain: "cart_edge_states" | test_boundary: "CalculateSubtotal([]), CalculateSubtotal([qty=0])" | test_type: "unit — boundary"

---

## Node 03 — SchemePlannerUnit

A: translated_boundaries: ["CalculateItemTotal", "CalculateSubtotal", "edge states"] | node: SchemePlannerUnit
B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA (Arrange-Act-Assert)"
    test_classes:
      - class: "CartCalculationServiceTests"
        methods:
          - "CalculateItemTotal_SingleItem_ReturnsCorrectTotal"
          - "CalculateItemTotal_MultipleQuantity_ReturnsMultipliedTotal"
          - "CalculateSubtotal_ThreeItems_ReturnsSum"
          - "CalculateSubtotal_EmptyCart_ReturnsZero"
          - "CalculateSubtotal_ZeroQuantityItem_ExcludesItem"

---

## Node 04 — DomainProcessorUnit

A: test_schema: CartCalculationServiceTests | node: DomainProcessorUnit
B:
  DependenciesIdentified: ["ICartItemRepository (mock)", "IPricingCatalog (mock)"]
  TestDoubles: ["MockCartItemRepository", "MockPricingCatalog"]
  IsolationStrategy: "Replace I/O dependencies with mocks; CartCalculationService logic is pure after mocking"

---

## Node 05 — ApplierUnit

A: test_plan: CartCalculationServiceTests — 5 methods | node: ApplierUnit
B:
  TestsProduced: 5
  Framework: xUnit
  Patterns: AAA
  MocksUsed: ["MockCartItemRepository", "MockPricingCatalog"]

---

## Node 06 — ResultTranslatorUnit

A: tests_produced: 5 | node: ResultTranslatorUnit
B:
  VerificationResult: "All 5 tests pass in isolation"
  FailedTests: 0 | SkippedTests: 0

---

## Node 07 — ArbiterSelectorUnit

A: verification_result: PASS | node: ArbiterSelectorUnit
B:
  QualityGateStatus: PASS
  CoverageEstimate: "~90% statement coverage on tested methods"
  Warnings: []

---

## Node 08 — ArbiterUnit

A: quality_gate: PASS | node: ArbiterUnit
B:
  Verdict: PASS
  OutputType: TestSuite
  Files: ["CartCalculationServiceTests.cs"]

---

## Node 09 — FormatterUnit

A: verdict: PASS | node: FormatterUnit
B:
  Result: PASS | Tests: 5 | Status: green
  OutputSummary: "CartCalculationServiceTests — 5 unit tests; item totals, subtotal, empty-cart edge cases covered."
