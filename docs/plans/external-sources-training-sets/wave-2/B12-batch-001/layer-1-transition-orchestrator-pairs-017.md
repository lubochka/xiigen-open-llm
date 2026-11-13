# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-017-001 — ProductSearchAPI GraphQL: documentation, two flows

```
source_id: xiigen-b12-l1-017-001
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Specify the GraphQL query contract for the product search API."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → GraphQL query contract (schema.graphql)
      Phase2: DocumentationFlow21 → GraphQL API documentation
    QACheckpoint: after Phase2

---

## B12-L1-017-002 — ProductSearchAPI: ARCHITECTURE first for "design GraphQL API"

```
source_id: xiigen-b12-l1-017-002
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Design the GraphQL API for the product search service and define the schema."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 3
    ExecutionPlan:
      Phase1: ArchitectureFlow31 → GraphQL architecture decisions (federation vs monolith, etc.)
      Phase2: APIContractFlow33 → GraphQL schema definition (informed by architecture)
      Phase3: DocumentationFlow21 → GraphQL API documentation
    QACheckpoint: after Phase3

---

## B12-L1-017-003 — ProductSearchAPI IMPLEMENT: feature_addition for resolvers

```
source_id: xiigen-b12-l1-017-003
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Implement the GraphQL resolvers for product search and create the schema."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → GraphQL resolver code (implement)
        - APIContractFlow33 → GraphQL schema definition (create)
      Phase2_Sequential: DocumentationFlow21 → resolver and schema documentation
    QACheckpoint: after Phase2

---

## B12-L1-017-004 — ProductSearch: GraphQL vs REST orchestration (identical)

```
source_id: xiigen-b12-l1-017-004
pair_kind: transition_orchestrator
```

Orchestration is API-style agnostic. The FlowOrder and ExecutionPlan are identical for:

GraphQL: "Specify GraphQL query contract for product search"
  FlowOrder: [APIContractFlow33, DocumentationFlow21]
  Phase1: APIContractFlow33 → schema.graphql output

REST: "Specify REST endpoints for product search"
  FlowOrder: [APIContractFlow33, DocumentationFlow21]
  Phase1: APIContractFlow33 → openapi.yaml output

Same orchestration plan. The output format (schema.graphql vs openapi.yaml) is
determined by APIContractFlow33 internally based on the API style detected in the request.
The orchestrator does not distinguish between GraphQL and REST for scheduling.
