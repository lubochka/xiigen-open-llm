# Layer 2 — Nine-Node Flow Pairs (019)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — FraudScoreCalculatorTests | TEST+IMPLEMENT
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the FraudScoreCalculator and implement the test doubles for the external risk-scoring API."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
Note: Both TEST and IMPLEMENT fire. "write unit tests" (TEST+IMPLEMENT) + "implement the test doubles" (IMPLEMENT).
Two flow instances: UnitTestingFlow51 (TEST) + BackendServiceFlow41 (IMPLEMENT).

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "score_computation" | scope: "ComputeScore(transaction) → 0-100 risk score"
    - domain: "velocity_check" | scope: "CheckVelocity(accountId, windowMinutes)"
    - domain: "threshold_classification" | scope: "ClassifyRisk(score) → low/medium/high"
    - domain: "test_double_impl" | scope: "Implement IRiskScoringApiDouble stub (IMPLEMENT sub-flow)"

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "score_computation" | test_boundary: "normal transaction, known-bad BIN (Bank Identification Number) → elevated score" | test_type: "unit"
    - source_domain: "velocity_check" | test_boundary: "3 orders in 10 min → pass; 6 orders → velocity breach" | test_type: "unit"
    - source_domain: "threshold_classification" | test_boundary: "score < 40 → low; 40-79 → medium; ≥80 → high" | test_type: "unit"
    - source_domain: "test_double_impl" | test_boundary: "stub returns configurable scores; routes to BackendServiceFlow41"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "FraudScoreCalculatorTests"
        methods:
          - "ComputeScore_NormalTransaction_ReturnsLowScore"
          - "ComputeScore_KnownBadBin_ReturnsElevatedScore"
          - "CheckVelocity_ThreeOrdersInWindow_ReturnsPass"
          - "CheckVelocity_SixOrdersInWindow_ReturnsBreach"
          - "ClassifyRisk_ScoreBelow40_ReturnsLow"
          - "ClassifyRisk_Score80OrAbove_ReturnsHigh"
          - "ClassifyRisk_Score50_ReturnsMedium"

---

## Node 04 — DomainProcessorUnit

B:
  TestDoubles: ["FakeRiskScoringApiDouble (implements IRiskScoringApi — constructed in IMPLEMENT sub-flow)", "MockTransactionRepository"]
  IsolationStrategy: "FakeRiskScoringApiDouble replaces external API call; all scoring logic tested in isolation"

---

## Node 05 — ApplierUnit

B: TestsProduced: 7 | Framework: xUnit | TestDoublesImplemented: ["FakeRiskScoringApiDouble"]

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 7 pass with FakeRiskScoringApiDouble" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~91%"

---

## Node 08 — ArbiterUnit

B: Verdict: PASS | Files: ["FraudScoreCalculatorTests.cs", "FakeRiskScoringApiDouble.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS | Tests: 7 | Status: green
  OutputSummary: "FraudScoreCalculatorTests — score computation, velocity check, threshold classification. Test double implemented in companion IMPLEMENT flow."
