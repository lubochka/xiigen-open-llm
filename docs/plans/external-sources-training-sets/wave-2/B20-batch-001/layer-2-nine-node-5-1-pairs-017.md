# Layer 2 — Nine-Node Flow Pairs (017)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — ShippingCostTests | TEST alone
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the ShippingCostCalculator covering standard, express, overnight weight tiers and free-shipping threshold."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "weight_tier_rates" | scope: "CalculateShipping(weight, service) — standard/express/overnight"
    - domain: "free_shipping_threshold" | scope: "IsEligibleForFreeShipping(orderTotal)"
    - domain: "international_surcharge" | scope: "ApplyInternationalSurcharge(baseRate, destinationZone)"

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "weight_tier_rates" | test_boundary: "1kg standard, 5kg boundary, 6kg heavy" | test_type: "unit"
    - source_domain: "free_shipping_threshold" | test_boundary: "total < threshold, total = threshold, total > threshold" | test_type: "unit — boundary"
    - source_domain: "international_surcharge" | test_boundary: "domestic zone (no surcharge), international zone (surcharge applies)" | test_type: "unit"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "ShippingCostCalculatorTests"
        methods:
          - "CalculateShipping_1kgStandard_ReturnsBaseRate"
          - "CalculateShipping_5kgBoundary_TransitionsToHeavyRate"
          - "CalculateShipping_OvernightService_ReturnsOvernightPremium"
          - "IsEligibleForFreeShipping_OrderBelowThreshold_ReturnsFalse"
          - "IsEligibleForFreeShipping_OrderAtThreshold_ReturnsTrue"
          - "ApplyInternationalSurcharge_DomesticZone_NoSurcharge"
          - "ApplyInternationalSurcharge_InternationalZone_AddsSurcharge"

---

## Node 04 — DomainProcessorUnit

B:
  TestDoubles: ["MockCarrierRateTable", "MockZoneConfigRepository"]

---

## Node 05 — ApplierUnit

B: TestsProduced: 7 | Framework: xUnit

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 7 pass" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~93%"
  Warnings: ["Carrier timeout path not yet covered — recommended for regression suite"]

---

## Node 08 — ArbiterUnit

B: Verdict: PASS_WITH_WARNINGS | Files: ["ShippingCostCalculatorTests.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS_WITH_WARNINGS | Tests: 7 | Status: green
  OutputSummary: "ShippingCostCalculatorTests — weight tiers, free-shipping boundary, international surcharge. Timeout path flagged for follow-up."
