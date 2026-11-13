# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-014-001 — InventoryAPI: documentation, two flows sequential

```
source_id: xiigen-b12-l1-014-001
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Document the inventory API endpoints for stock level queries."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → inventory REST endpoint contracts
      Phase2: DocumentationFlow21 → inventory API reference docs
    QACheckpoint: after Phase2

---

## B12-L1-014-002 — InventoryAPI: DOCUMENT only, single flow (database schema)

```
source_id: xiigen-b12-l1-014-002
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Define the database schema for the inventory stock levels table."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 1
    ExecutionPlan:
      Phase1: DocumentationFlow21 → database schema documentation
    QACheckpoint: after Phase1

Note: Single DOCUMENT predicate → single DocumentationFlow21 execution.
No APIContractFlow33 → no 3.3 execution. No sub-phase 3.3 output.

---

## B12-L1-014-003 — InventoryAPI IMPLEMENT: feature_addition, three flows parallel+sequential

```
source_id: xiigen-b12-l1-014-003
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Write the OpenAPI specification for the inventory management API."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → OpenAPI spec file creation (code/artifact creation)
        - APIContractFlow33 → inventory contract definition
      Phase2_Sequential:
        - DocumentationFlow21 → inventory API documentation (consumes Phase1 outputs)
    QACheckpoint: after Phase2

---

## B12-L1-014-004 — InventoryAPI routing contrast: 3.3 present vs absent

```
source_id: xiigen-b12-l1-014-004
pair_kind: transition_orchestrator
```

The presence or absence of APIContractFlow33 in the orchestration plan
depends entirely on whether API_CONTRACT is in CompiledPredicateIds:

Case A: "Document inventory API endpoints"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Flows: [APIContractFlow33, DocumentationFlow21]
  3.3 execution: YES → inventory endpoint contracts produced

Case B: "Define database schema for inventory"
  CompiledPredicateIds: ["DOCUMENT"]
  Flows: [DocumentationFlow21]
  3.3 execution: NO → no inventory API contract produced

The orchestrator respects the routing decision exactly.
It never adds flows for sub-phases not covered by the predicate list.
