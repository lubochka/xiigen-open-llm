# Layer 0-D — Flow Factory Pairs (024) — Flow factory behaviour for zero-predicate requests
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F024-001 — Summary: factory outputs for TEST alone, TEST+IMPLEMENT, and zero-predicate requests

| Input predicates | FlowInstances produced | TransitionType |
|-----------------|----------------------|----------------|
| ["TEST"] | [UnitTestingFlow51] | documentation |
| ["TEST", "IMPLEMENT"] | [UnitTestingFlow51, BackendServiceFlow41] | feature_addition |
| ["TEST", "SECURITY"] | [UnitTestingFlow51, SecurityComplianceFlow52] | feature_addition |
| ["TEST", "IMPLEMENT", "SECURITY"] | [UnitTestingFlow51, BackendServiceFlow41, SecurityComplianceFlow52] | feature_addition |
| [] (run / execute / check) | [] — NoFlowCreated | unknown → knowledge_edge |

---

## F024-002 — Factory invariant: one FlowInstance per unique predicate

Rule: FlowFactory.CreateMulti(predicates) → one FlowInstanceDescriptor per predicate, never collapsed, never merged.
  - ["TEST"] → 1 instance
  - ["TEST", "IMPLEMENT"] → 2 instances (not merged into one)
  - ["TEST", "IMPLEMENT", "SECURITY"] → 3 instances (not merged)
  - [] → 0 instances (knowledge_edge; factory returns empty list)

Anti-pattern (v6-03 error): collapsing multiple instances to one (e.g., selecting only UnitTestingFlow51 when IMPLEMENT is also present).

---

## F024-003 — "run tests" path through the factory: early termination at zero predicates

Request: "Run unit tests for the CartService."
  Compiler output: CompiledPredicateIds: [] (zero predicates)
  Router output: RoutingDecisions: [] (no routing)
  Factory input: predicate list is empty
  Factory output: FlowInstances: [] — factory returns immediately with empty list
  CapabilityGapSignal raised: gap-run-not-test-factory-024
  No nine-node flow is started. No TransitionType is assigned. Request is logged as knowledge_edge.
