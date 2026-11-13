# Layer 2 — Nine-Node Flow Pairs (023)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — PROBATIONARY_REJECTED (TEST+QA quality gate flow rejected)
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "QA the coupon applicator flow and test the single-use coupon assertion before release."
Sub-phase: 5.1 (TEST portion) + 7.3 (QA portion)
Predicates: ["QA", "TEST"] | TransitionType: feature_addition
Note: This flow is PROBATIONARY — it contains both QA and TEST predicates.
The quality gate (ArbiterUnit) flags the QA+TEST mixture as ambiguous routing.
The flow is REJECTED at Node 07 — the predicate compiler should have kept QA and TEST
as separate requests, not combined.

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "test_assertion" | scope: "'test the single-use coupon assertion' → TEST → 5.1"
    - domain: "qa_process" | scope: "'QA the coupon applicator flow' → QA → 7.3"
  SplitWarning: "Two predicates (QA + TEST) map to different sub-phases; flow is ambiguous."

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "test_assertion" | test_boundary: "Assert CouponApplicator marks coupon consumed after first use" | test_type: "unit"
    - source_domain: "qa_process" | test_boundary: "Cannot translate to unit test — QA is a process, not a test method" | test_type: "UNTRANSLATABLE"
  TranslationWarning: "QA domain cannot be translated to a unit test. QA routes to 7.3, not 5.1."

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit"
    PlanningWarning: "Only TEST portion can be planned. QA portion deferred to 7.3 QA flow."
    test_classes:
      - class: "CouponApplicatorTests (partial)"
        methods:
          - "MarkConsumed_AfterFirstApplication_MarksAsConsumed"

---

## Node 04 — DomainProcessorUnit

B: TestDoubles: ["MockCouponRepository"]

---

## Node 05 — ApplierUnit

B: TestsProduced: 1 | Framework: xUnit | QAPortionDeferred: true

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "1 test passes; QA portion not executed in this flow" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit (REJECTION GATE)

B:
  QualityGateStatus: REJECTED
  RejectionReason: "Mixed QA+TEST request should be split into two separate requests before processing.
    QA → separate QA review flow at 7.3. TEST → UnitTestingFlow51 at 5.1.
    Processing both in a single nine-node flow produces an incomplete output for both sub-phases."
  Recommendation: "Resubmit as two separate requests: (1) 'Test the single-use coupon assertion' (TEST → 5.1), (2) 'QA the coupon applicator flow' (QA → 7.3)."

---

## Node 08 — ArbiterUnit

B: Verdict: REJECTED
  RejectionClassification: "mixed_predicate_routing_conflict"

---

## Node 09 — FormatterUnit

B: Result: REJECTED | Tests: 1 (partial) | Status: rejected
  OutputSummary: "Mixed QA+TEST request rejected at quality gate. Split required. See Node 07 for recommended resubmission."
