# Layer 0-B — Router Pairs (025) — InterestCalculator routing: TEST alone vs TEST+IMPLEMENT
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R025-001 — TEST alone routes to 5.1 only

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Validate that the InterestCalculator returns correct simple interest for a £1,000 principal at 5% over 12 months."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1 (UnitTestingFlow)
  TransitionType: documentation
  PrimarySubPhaseId: 5.1
  SourceStatus: routable

---

## R025-002 — TEST+IMPLEMENT routes to 5.1 + 4.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the InterestCalculator and implement the simple interest formula."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
    - predicate: IMPLEMENT → sub_phase: 4.1
  TransitionType: feature_addition
  PrimarySubPhaseId: 4.1
  AllSubPhaseIds: [4.1, 5.1]

---

## R025-003 — ZERO predicates: "check" does not route

A:
  compiled_predicate_ids: []
  compiled_text: "Check that the InterestCalculator returns correct interest for edge cases."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
  CapabilityGapSignal:
    gap_id: "gap-check-not-test-025"
    gap_vocabulary: ["check that", "correct interest"]
    SCHEDULE_CAPABILITY_LEARNING: true
