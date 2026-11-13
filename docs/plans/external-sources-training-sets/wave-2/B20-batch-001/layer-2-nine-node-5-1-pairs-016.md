# Layer 2 — Nine-Node Flow Pairs (016)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — TaxCalculationTests | TEST alone
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the TaxCalculationService covering EU VAT bands, US state tax, and zero-rated goods."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "eu_vat" | scope: "ComputeVat(amount, countryCode) — UK 20%, DE 19%, FR 20%"
    - domain: "us_state_tax" | scope: "ComputeStateTax(amount, stateCode)"
    - domain: "zero_rated" | scope: "IsZeroRated(productCategory) — children's clothing, food"

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "eu_vat" | test_boundary: "UK standard rate, DE reduced rate, unknown country → error" | test_type: "unit"
    - source_domain: "us_state_tax" | test_boundary: "CA rate, TX rate, null state → error" | test_type: "unit"
    - source_domain: "zero_rated" | test_boundary: "children's clothing → zero, electronics → standard" | test_type: "unit"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "TaxCalculationServiceTests"
        methods:
          - "ComputeVat_UkStandardRate_Returns20Percent"
          - "ComputeVat_GermanReducedRate_Returns19Percent"
          - "ComputeVat_UnknownCountry_ReturnsError"
          - "ComputeStateTax_California_ReturnsCorrectRate"
          - "ComputeStateTax_NullState_ReturnsError"
          - "IsZeroRated_ChildrensClothing_ReturnsTrue"
          - "IsZeroRated_Electronics_ReturnsFalse"

---

## Node 04 — DomainProcessorUnit

B:
  TestDoubles: ["MockTaxRateRepository", "MockCountryConfigRepository"]

---

## Node 05 — ApplierUnit

B: TestsProduced: 7 | Framework: xUnit

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 7 pass" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~91%"

---

## Node 08 — ArbiterUnit

B: Verdict: PASS | Files: ["TaxCalculationServiceTests.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS | Tests: 7 | Status: green
  OutputSummary: "TaxCalculationServiceTests — EU VAT, US state tax, zero-rated goods."
