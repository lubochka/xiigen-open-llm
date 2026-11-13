# Layer 0-B — Router Pairs (033) — MarkupCalculator routing: "test coverage" compound routes TEST
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R033-001 — TEST alone: "test coverage" routes to 5.1 even when "ensure" is the main verb

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Ensure test coverage for the MarkupCalculator includes 100% branch coverage on the rounding logic."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
  TransitionType: documentation
  Note: "ensure" is zero-predicate; "test coverage" is the TEST signal. Routes to 5.1 despite "ensure".

---

## R033-002 — TEST+IMPLEMENT: "build and create test suite" routes to 4.1 + 5.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Build the MarkupCalculator and create a test suite covering retail, wholesale, and bulk pricing tiers."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]

---

## R033-003 — ZERO predicates: "launch" does not route

A:
  compiled_predicate_ids: []
  compiled_text: "Launch the MarkupCalculator to price all SKUs in the catalogue."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
