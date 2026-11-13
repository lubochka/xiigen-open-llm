# Layer 2 — Nine-Node Flow Pairs: UnitTestingFlow51
# Batch: B20-batch-001
# Scenario: B20 — Unit testing, sub-phase 5.1
# Flow: UnitTestingFlow51 — PaymentCalculationService unit test suite
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the PaymentCalculationService covering all edge cases."
Sub-phase: 5.1 — Unit and Integration Testing
TrustState: deterministic_generic
FlowId: UnitTestingFlow51
Target service: PaymentCalculationService — calculates order totals, applies discount
tiers, computes tax by region, validates payment instrument limits.

---

## Node 1 — DomainSplitterUnit

```
source_id: xiigen-b20-nine-001
node: DomainSplitterUnit
pair_kind: nine_node_flow
```

A:
  flow_input: "Write unit tests for the PaymentCalculationService covering all edge cases."
  node: DomainSplitterUnit

B:
  DomainBoundaries:
    - domain: "payment_calculation"
      scope: "PaymentCalculationService — order total, discount tiers, tax computation"
    - domain: "payment_validation"
      scope: "Payment instrument validation — card limits, currency checks"
  SplitStrategy: "Split by service responsibility boundary: calculation logic vs. validation logic"
  NodeOutput: "Two test domain buckets: calculation and validation"

---

## Node 2 — DomainTranslatorUnit

```
source_id: xiigen-b20-nine-002
node: DomainTranslatorUnit
pair_kind: nine_node_flow
```

A:
  domain_buckets: ["payment_calculation", "payment_validation"]
  node: DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "payment_calculation"
      test_boundary: "CalculateOrderTotal(), ApplyDiscountTier(), ComputeTaxByRegion()"
      test_type: "unit — pure function logic, no I/O"
    - source_domain: "payment_validation"
      test_boundary: "ValidateCardLimit(), ValidateCurrency(), ValidateInstrument()"
      test_type: "unit — validation logic with mock payment gateway"
  TranslationNote: "Each public method becomes a test class entry point;
    no integration test boundary identified (no external I/O in scope)"

---

## Node 3 — SchemePlannerUnit

```
source_id: xiigen-b20-nine-003
node: SchemePlannerUnit
pair_kind: nine_node_flow
```

A:
  translated_boundaries: ["CalculateOrderTotal", "ApplyDiscountTier", "ComputeTaxByRegion",
                          "ValidateCardLimit", "ValidateCurrency"]
  node: SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit"
    pattern: "Arrange-Act-Assert (AAA)"
    test_classes:
      - class: "PaymentCalculationServiceTests"
        methods:
          - "CalculateOrderTotal_StandardOrder_ReturnsCorrectTotal"
          - "CalculateOrderTotal_ZeroItems_ReturnsZero"
          - "CalculateOrderTotal_NegativeQuantity_ReturnsError"
          - "ApplyDiscountTier_GoldMember_AppliesTenPercent"
          - "ApplyDiscountTier_NoMembership_AppliesNoDiscount"
          - "ComputeTaxByRegion_UKRegion_AppliesVAT"
          - "ComputeTaxByRegion_USRegion_AppliesStateTax"
          - "ComputeTaxByRegion_UnknownRegion_ReturnsError"
      - class: "PaymentValidationTests"
        methods:
          - "ValidateCardLimit_WithinLimit_ReturnsOk"
          - "ValidateCardLimit_ExceedsLimit_ReturnsFailure"
          - "ValidateCurrency_SupportedCurrency_ReturnsOk"
          - "ValidateCurrency_UnsupportedCurrency_ReturnsFailure"
  MockDependencies:
    - "IPaymentGateway — mock for validation tests"
    - "IRegionTaxProvider — mock for tax computation tests"

---

## Node 4 — DomainProcessorUnit

```
source_id: xiigen-b20-nine-004
node: DomainProcessorUnit
pair_kind: nine_node_flow
```

A:
  test_suite_schema: "PaymentCalculationServiceTests + PaymentValidationTests (12 methods)"
  node: DomainProcessorUnit

B:
  ProcessedTestCases:
    - test: "CalculateOrderTotal_StandardOrder_ReturnsCorrectTotal"
      arrange: "var service = new PaymentCalculationService(); var order = new Order { Items = [{Price: 100, Qty: 2}] };"
      act: "var result = service.CalculateOrderTotal(order);"
      assert: "Assert.True(result.IsSuccess); Assert.Equal(200m, result.Value);"

    - test: "CalculateOrderTotal_ZeroItems_ReturnsZero"
      arrange: "var order = new Order { Items = [] };"
      act: "var result = service.CalculateOrderTotal(order);"
      assert: "Assert.True(result.IsSuccess); Assert.Equal(0m, result.Value);"

    - test: "CalculateOrderTotal_NegativeQuantity_ReturnsError"
      arrange: "var order = new Order { Items = [{Price: 100, Qty: -1}] };"
      act: "var result = service.CalculateOrderTotal(order);"
      assert: "Assert.False(result.IsSuccess); Assert.Equal(\"INVALID_QUANTITY\", result.ErrorCode);"

    - test: "ApplyDiscountTier_GoldMember_AppliesTenPercent"
      arrange: "var total = 200m; var member = MemberTier.Gold;"
      act: "var result = service.ApplyDiscountTier(total, member);"
      assert: "Assert.Equal(180m, result.Value);"

    - test: "ValidateCardLimit_ExceedsLimit_ReturnsFailure"
      arrange: "var mockGateway = new Mock<IPaymentGateway>(); mockGateway.Setup(g => g.GetLimit(It.IsAny<string>())).Returns(100m);"
      act: "var result = service.ValidateCardLimit(\"card-123\", 200m, mockGateway.Object);"
      assert: "Assert.False(result.IsSuccess); Assert.Equal(\"LIMIT_EXCEEDED\", result.ErrorCode);"

  EdgeCaseCoverage:
    - "Zero items order"
    - "Negative quantity"
    - "Unknown/unsupported region"
    - "Card limit exceeded"
    - "Unsupported currency"
    - "Gold/Silver/None member tiers"

---

## Node 5 — ApplierUnit

```
source_id: xiigen-b20-nine-005
node: ApplierUnit
pair_kind: nine_node_flow
```

A:
  processed_test_cases: "12 test cases across PaymentCalculationServiceTests + PaymentValidationTests"
  node: ApplierUnit

B:
  AppliedConventions:
    naming: "MethodName_Scenario_ExpectedResult (xUnit naming standard)"
    structure: "Arrange-Act-Assert blocks with blank line separators"
    return_type: "All service methods return OperationResult<T> — assert IsSuccess/IsFailure + Value/ErrorCode"
    mocking: "Moq framework for IPaymentGateway and IRegionTaxProvider"
    test_data: "Inline primitives for unit tests; no shared test fixtures"
  GeneratedTestFile:
    namespace: "LlmFromScratch.Tests.PaymentCalculation"
    using_directives: ["Xunit", "Moq", "LlmFromScratch.Services.Payment"]
    class_count: 2
    method_count: 12

---

## Node 6 — ResultTranslatorUnit

```
source_id: xiigen-b20-nine-006
node: ResultTranslatorUnit
pair_kind: nine_node_flow
```

A:
  applied_conventions: "xUnit + Moq, 12 tests, OperationResult<T> assertions"
  node: ResultTranslatorUnit

B:
  TranslatedOutput:
    format: "C# xUnit test file"
    file_path: "tests/LlmFromScratch.Tests/PaymentCalculation/PaymentCalculationServiceTests.cs"
    structure:
      - "using declarations"
      - "namespace LlmFromScratch.Tests.PaymentCalculation"
      - "public class PaymentCalculationServiceTests"
      - "  [Fact] methods for calculation logic"
      - "public class PaymentValidationTests"
      - "  [Fact] methods with Mock<IPaymentGateway>"
    total_test_count: 12
    coverage_note: "Happy path + boundary + error path for each public method"

---

## Node 7 — ArbiterSelectorUnit

```
source_id: xiigen-b20-nine-007
node: ArbiterSelectorUnit
pair_kind: nine_node_flow
```

A:
  translated_output: "12 xUnit tests in PaymentCalculationServiceTests + PaymentValidationTests"
  trust_state: deterministic_generic
  node: ArbiterSelectorUnit

B:
  SelectedArbiter: "CoverageCompletenessArbiter"
  SelectionReason: "deterministic_generic trust state at 5.1 — arbiter validates test
    coverage completeness (happy path + edge case + error path per method)"
  ArbiterConfig:
    required_paths_per_method: ["happy_path", "edge_case", "error_path"]
    min_test_count: 3

---

## Node 8 — ArbiterUnit

```
source_id: xiigen-b20-nine-008
node: ArbiterUnit
pair_kind: nine_node_flow
```

A:
  arbiter: "CoverageCompletenessArbiter"
  test_suite: "12 tests across 5 methods"
  node: ArbiterUnit

B:
  ArbiterVerdict:
    CalculateOrderTotal:
      happy_path: "CalculateOrderTotal_StandardOrder_ReturnsCorrectTotal ✓"
      edge_case: "CalculateOrderTotal_ZeroItems_ReturnsZero ✓"
      error_path: "CalculateOrderTotal_NegativeQuantity_ReturnsError ✓"
      verdict: PASS

    ApplyDiscountTier:
      happy_path: "ApplyDiscountTier_GoldMember_AppliesTenPercent ✓"
      edge_case: "ApplyDiscountTier_NoMembership_AppliesNoDiscount ✓"
      error_path: "missing — no error path for invalid tier"
      verdict: WARN — missing error path for ApplyDiscountTier invalid input

    ComputeTaxByRegion:
      happy_path: "ComputeTaxByRegion_UKRegion_AppliesVAT ✓"
      edge_case: "ComputeTaxByRegion_USRegion_AppliesStateTax ✓"
      error_path: "ComputeTaxByRegion_UnknownRegion_ReturnsError ✓"
      verdict: PASS

    ValidateCardLimit:
      happy_path: "ValidateCardLimit_WithinLimit_ReturnsOk ✓"
      error_path: "ValidateCardLimit_ExceedsLimit_ReturnsFailure ✓"
      edge_case: "missing — boundary value (exactly at limit) not covered"
      verdict: WARN — missing boundary test

    ValidateCurrency:
      happy_path: "ValidateCurrency_SupportedCurrency_ReturnsOk ✓"
      error_path: "ValidateCurrency_UnsupportedCurrency_ReturnsFailure ✓"
      verdict: PASS (2-path sufficient for validation function)

  OverallVerdict: PASS_WITH_WARNINGS
  Warnings:
    - "ApplyDiscountTier missing error path for invalid tier input"
    - "ValidateCardLimit missing boundary test (amount == limit)"

---

## Node 9 — FormatterUnit

```
source_id: xiigen-b20-nine-009
node: FormatterUnit
pair_kind: nine_node_flow
```

A:
  arbiter_verdict: "PASS_WITH_WARNINGS — 12 tests, 2 gaps identified"
  node: FormatterUnit

B:
  FormattedOutput:
    delivery_format: "C# xUnit test file with inline gap annotations"
    file: "PaymentCalculationServiceTests.cs"
    summary_block: |
      // Test suite: PaymentCalculationService
      // Coverage: 12 tests across 5 public methods
      // Arbiter verdict: PASS_WITH_WARNINGS
      // Gaps: ApplyDiscountTier error path; ValidateCardLimit boundary value
    gap_annotations:
      - location: "ApplyDiscountTier test class"
        comment: "// TODO: Add test for invalid MemberTier enum value → should return INVALID_TIER"
      - location: "ValidateCardLimit test class"
        comment: "// TODO: Add test for amount == limit boundary (exactly at limit should pass)"
    output_status: "ready_with_gaps"
