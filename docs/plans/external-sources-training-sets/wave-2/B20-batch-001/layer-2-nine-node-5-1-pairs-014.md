# Layer 2 — Nine-Node Flow Pairs (014)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — InventoryAllocationTests | TEST alone
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the InventoryAllocationService covering stock reservation, over-allocation guard, and rollback on payment failure."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Target: InventoryAllocationService — reserves stock for orders, validates availability, rolls back on failure.
Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

A: flow_input: "Write unit tests for InventoryAllocationService." | node: DomainSplitterUnit
B:
  DomainBoundaries:
    - domain: "stock_reservation" | scope: "ReserveStock(orderId, productId, qty)"
    - domain: "availability_validation" | scope: "ValidateAvailability(productId, qty)"
    - domain: "rollback_on_failure" | scope: "ReleaseReservation(orderId)"

---

## Node 02 — DomainTranslatorUnit

A: domain_buckets: ["stock_reservation", "availability_validation", "rollback_on_failure"] | node: DomainTranslatorUnit
B:
  TranslatedTestBoundaries:
    - source_domain: "stock_reservation" | test_boundary: "ReserveStock — success path, partial stock" | test_type: "unit"
    - source_domain: "availability_validation" | test_boundary: "ValidateAvailability — zero stock, exact match, over-request" | test_type: "unit"
    - source_domain: "rollback_on_failure" | test_boundary: "ReleaseReservation — partial release, full release" | test_type: "unit"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "InventoryAllocationServiceTests"
        methods:
          - "ReserveStock_SufficientStock_ReservesCorrectQuantity"
          - "ReserveStock_InsufficientStock_ThrowsInsufficientStockException"
          - "ValidateAvailability_ExactMatch_ReturnsTrue"
          - "ValidateAvailability_ZeroStock_ReturnsFalse"
          - "ReleaseReservation_PaymentFailed_ReleasesFullReservedQuantity"

---

## Node 04 — DomainProcessorUnit

B:
  DependenciesIdentified: ["IInventoryRepository (mock)", "IOrderRepository (mock)"]
  TestDoubles: ["MockInventoryRepository", "MockOrderRepository"]

---

## Node 05 — ApplierUnit

B: TestsProduced: 5 | Framework: xUnit

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 5 tests pass" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~88% on tested methods"

---

## Node 08 — ArbiterUnit

B: Verdict: PASS | Files: ["InventoryAllocationServiceTests.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS | Tests: 5 | Status: green
  OutputSummary: "InventoryAllocationServiceTests — stock reservation, availability guard, rollback path."
