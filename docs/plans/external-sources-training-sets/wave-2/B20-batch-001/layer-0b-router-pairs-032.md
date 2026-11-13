# Layer 0-B — Router Pairs (032) — MarginCalculator routing: three TEST verbs deduplicate to one decision
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R032-001 — TEST+IMPLEMENT: "create and add test cases" routes to both

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Create the MarginCalculator service and add test cases for gross margin and net margin calculations."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]

---

## R032-002 — TEST alone: "validate, assert, verify" → one routing decision

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Validate, assert, and verify that the MarginCalculator handles a 0% margin without error."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
  TransitionType: documentation
  Note: Three TEST-firing verbs (validate, assert, verify) deduplicate to exactly one routing decision.
  TransitionType is documentation because no IMPLEMENT construction verb is present.

---

## R032-003 — ZERO predicates: "trigger" does not route

A:
  compiled_predicate_ids: []
  compiled_text: "Trigger the MarginCalculator for all products in the quarterly report."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
