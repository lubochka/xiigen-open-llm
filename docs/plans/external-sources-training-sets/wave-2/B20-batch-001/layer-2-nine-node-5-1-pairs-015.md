# Layer 2 — Nine-Node Flow Pairs (015)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — DiscountEngineTests | TEST alone
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the DiscountEngine covering percentage discounts, fixed-amount discounts, BOGO (Buy One Get One) rules, and discount exclusion logic."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "percentage_discount" | scope: "ApplyPercentageDiscount(cart, pct)"
    - domain: "fixed_discount" | scope: "ApplyFixedDiscount(cart, amount)"
    - domain: "bogo_discount" | scope: "ApplyBogoDiscount(cart, eligibleSku)"
    - domain: "discount_exclusion" | scope: "CanCombine(discountA, discountB)"

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "percentage_discount" | test_boundary: "10%, 20%, 0% edge" | test_type: "unit"
    - source_domain: "fixed_discount" | test_boundary: "£10 off, cap enforcement" | test_type: "unit"
    - source_domain: "bogo_discount" | test_boundary: "eligible SKU (Stock Keeping Unit) present, no eligible SKU" | test_type: "unit"
    - source_domain: "discount_exclusion" | test_boundary: "non-combinable pair, combinable pair" | test_type: "unit"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "DiscountEngineTests"
        methods:
          - "ApplyPercentageDiscount_TenPercent_ReducesTotalCorrectly"
          - "ApplyPercentageDiscount_ZeroPercent_LeavesTotalUnchanged"
          - "ApplyFixedDiscount_BelowCap_ReducesTotalByFixedAmount"
          - "ApplyBogoDiscount_EligibleSkuPresent_AddsSecondItemAtZeroCost"
          - "ApplyBogoDiscount_NoEligibleSku_LeavesTotalUnchanged"
          - "CanCombine_NonCombinablePair_ReturnsFalse"
          - "CanCombine_CombinablePair_ReturnsTrue"

---

## Node 04 — DomainProcessorUnit

B:
  TestDoubles: ["MockProductCatalog", "MockDiscountRuleRepository"]

---

## Node 05 — ApplierUnit

B: TestsProduced: 7 | Framework: xUnit

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 7 pass" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~92%"

---

## Node 08 — ArbiterUnit

B: Verdict: PASS | Files: ["DiscountEngineTests.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS | Tests: 7 | Status: green
