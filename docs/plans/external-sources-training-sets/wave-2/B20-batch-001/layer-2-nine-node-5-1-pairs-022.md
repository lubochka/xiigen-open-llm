# Layer 2 — Nine-Node Flow Pairs (022)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — PriceFormatterTests | TEST alone
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the PriceFormatter covering GBP, EUR (German locale), USD, zero price (free display), negative price (parenthesis wrapping), and rounding."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "locale_formatting" | scope: "Format(amount, locale) — symbol, separator, decimal places"
    - domain: "zero_price_display" | scope: "Format(0, locale, freeDisplay=true)"
    - domain: "negative_price_display" | scope: "Format(negative, locale, accountancyMode=true)"
    - domain: "rounding" | scope: "RoundHalfUp(amount) — always 2 decimal places"

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "locale_formatting" | test_boundary: "GBP → £1,234.56; EUR German → 1.234,56 €; USD → $1,234.56" | test_type: "unit"
    - source_domain: "zero_price_display" | test_boundary: "freeDisplay=true → 'Free'; freeDisplay=false → '£0.00'" | test_type: "unit"
    - source_domain: "negative_price_display" | test_boundary: "accountancyMode=true → '(£10.00)'; false → '-£10.00'" | test_type: "unit"
    - source_domain: "rounding" | test_boundary: "£1.005 → £1.01 (half-up); £1.004 → £1.00" | test_type: "unit"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "PriceFormatterTests"
        methods:
          - "Format_GbpLocale_ReturnsCorrectSymbolAndSeparator"
          - "Format_EurGermanLocale_ReturnsCommaDecimalPoint"
          - "Format_ZeroPrice_FreeDisplayEnabled_ReturnsFreeString"
          - "Format_ZeroPrice_FreeDisplayDisabled_ReturnsZeroCurrency"
          - "Format_NegativePrice_AccountancyMode_WrapsInParentheses"
          - "RoundHalfUp_PointFive_RoundsUp"
          - "RoundHalfUp_BelowPointFive_RoundsDown"

---

## Node 04 — DomainProcessorUnit

B:
  TestDoubles: ["MockLocaleConfigProvider"]
  IsolationStrategy: "PriceFormatter is pure function after locale config is mocked"

---

## Node 05 — ApplierUnit

B: TestsProduced: 7 | Framework: xUnit

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 7 pass" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~93%"

---

## Node 08 — ArbiterUnit

B: Verdict: PASS | Files: ["PriceFormatterTests.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS | Tests: 7 | Status: green
