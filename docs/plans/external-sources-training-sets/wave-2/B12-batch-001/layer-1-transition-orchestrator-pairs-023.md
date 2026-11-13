# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-023-001 — IMPLEMENT+API_CONTRACT: feature_addition orchestration (build REST)

```
source_id: xiigen-b12-l1-023-001
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Build the REST API for the product catalog service."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → REST API implementation code
        - APIContractFlow33 → product catalog endpoint contract
      Phase2_Sequential: DocumentationFlow21 → API documentation
    QACheckpoint: after Phase2

---

## B12-L1-023-002 — IMPLEMENT+API_CONTRACT: feature_addition orchestration (create GraphQL)

```
source_id: xiigen-b12-l1-023-002
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Create the GraphQL schema and resolver contract for customer profiles."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → GraphQL resolver implementation
        - APIContractFlow33 → GraphQL schema definition
      Phase2_Sequential: DocumentationFlow21 → GraphQL API documentation
    QACheckpoint: after Phase2

---

## B12-L1-023-003 — Documentation vs feature_addition: orchestration pattern summary

```
source_id: xiigen-b12-l1-023-003
pair_kind: transition_orchestrator
```

Two-flow documentation pattern [API_CONTRACT, DOCUMENT]:
  ExecutionPlan:
    Phase1: APIContractFlow33 (sequential, primary)
    Phase2: DocumentationFlow21 (sequential, depends on Phase1)
    QA: after Phase2
  Output count: 2-3 files

Three-flow feature_addition pattern [IMPLEMENT, API_CONTRACT, DOCUMENT]:
  ExecutionPlan:
    Phase1_Parallel: [ImplementationFlow43, APIContractFlow33]
    Phase2_Sequential: DocumentationFlow21
    QA: after Phase2
  Output count: 4-6 files (code + spec + tests + docs)

Three-flow documentation pattern [ARCHITECTURE, API_CONTRACT, DOCUMENT]:
  ExecutionPlan:
    Phase1_Sequential: ArchitectureFlow31
    Phase2_Sequential: APIContractFlow33
    Phase3_Sequential: DocumentationFlow21
    QA: after Phase3
  Output count: 3-4 files (architecture diagrams + contract spec + docs)

---

## B12-L1-023-004 — "update" TransitionType: documentation orchestration

```
source_id: xiigen-b12-l1-023-004
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Update the API contract documentation to reflect new response fields."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → updated API contract diff
      Phase2: DocumentationFlow21 → updated API documentation
    QACheckpoint: after Phase2
    Note: "update" → documentation TransitionType → no ImplementationFlow43.
    Orchestrator does NOT scaffold new code for documentation TransitionType.
