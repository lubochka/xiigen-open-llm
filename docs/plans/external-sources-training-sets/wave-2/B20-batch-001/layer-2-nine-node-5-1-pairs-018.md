# Layer 2 — Nine-Node Flow Pairs (018)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — LoyaltyPointsTests | TEST alone
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the LoyaltyPointsService covering point accrual, tier multiplier, redemption cap, and expiry logic."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "point_accrual" | scope: "AwardPoints(orderId, amount, tier)"
    - domain: "tier_multiplier" | scope: "GetMultiplier(tier) — gold=3x, silver=2x, standard=1x"
    - domain: "redemption" | scope: "RedeemPoints(accountId, points)"
    - domain: "point_expiry" | scope: "ExpireUnusedPoints(accountId, cutoffDate)"

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "point_accrual" | test_boundary: "£100 standard → 100 pts, £100 gold → 300 pts" | test_type: "unit"
    - source_domain: "tier_multiplier" | test_boundary: "each tier constant, unknown tier → error" | test_type: "unit"
    - source_domain: "redemption" | test_boundary: "redeem within balance, redeem above balance → error" | test_type: "unit"
    - source_domain: "point_expiry" | test_boundary: "12-month-old points expire, recent points do not" | test_type: "unit"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "LoyaltyPointsServiceTests"
        methods:
          - "AwardPoints_StandardTier_Awards1PointPerPound"
          - "AwardPoints_GoldTier_Awards3PointsPerPound"
          - "GetMultiplier_UnknownTier_ReturnsError"
          - "RedeemPoints_WithinBalance_DeductsCorrectly"
          - "RedeemPoints_AboveBalance_ThrowsInsufficientPointsException"
          - "ExpireUnusedPoints_OlderThan12Months_MarksAsExpired"
          - "ExpireUnusedPoints_RecentPoints_DoesNotExpire"

---

## Node 04 — DomainProcessorUnit

B:
  TestDoubles: ["MockLoyaltyAccountRepository", "MockTierConfigRepository"]

---

## Node 05 — ApplierUnit

B: TestsProduced: 7 | Framework: xUnit

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 7 pass" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~94%"

---

## Node 08 — ArbiterUnit

B: Verdict: PASS | Files: ["LoyaltyPointsServiceTests.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS | Tests: 7 | Status: green
