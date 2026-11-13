# Layer 0-B — Router Pairs (024) — "run" ≠ TEST boundary: zero-predicate routing
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R024-001 — ZERO predicates: "run unit tests" does NOT route

A:
  compiled_predicate_ids: []
  compiled_text: "Run unit tests for the CartCalculationService and report the results."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
  CapabilityGapSignal:
    gap_id: "gap-run-not-test-001"
    gap_vocabulary: ["run unit tests", "run tests"]
    SCHEDULE_CAPABILITY_LEARNING: true
  Note: "run" is an execution verb. It does not fire TEST. The object "unit tests" does not promote
  "run" into a TEST predicate. Routing produces zero decisions → knowledge_edge.

---

## R024-002 — ZERO predicates: "run the test suite" does NOT route

A:
  compiled_predicate_ids: []
  compiled_text: "Run the test suite against the BillingService before deployment."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
  Note: Same pattern — "run" is execution; "test suite" is the object, not the action. Zero predicates.

---

## R024-003 — Minimal pair routing: "write tests" (routes) vs "run tests" (does not route)

A-write:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write tests for the DiscountEngine."
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
    - predicate: IMPLEMENT → sub_phase: 4.1

A-run:
  compiled_predicate_ids: []
  compiled_text: "Run tests for the DiscountEngine."
  RoutingDecisions: []
  TransitionType: unknown → knowledge_edge

Teaching: The same noun object ("tests for the DiscountEngine") produces opposite routing outcomes
depending solely on the action verb. "write" fires IMPLEMENT; "tests" fires TEST. "run" fires nothing.
