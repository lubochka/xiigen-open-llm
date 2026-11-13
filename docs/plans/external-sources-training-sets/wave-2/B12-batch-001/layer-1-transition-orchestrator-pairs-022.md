# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-022-001 — API versioning: documentation, two flows

```
source_id: xiigen-b12-l1-022-001
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Document the versioned API contract changes between v1 and v2."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → versioned API contract diff spec
      Phase2: DocumentationFlow21 → API migration documentation
    QACheckpoint: after Phase2

---

## B12-L1-022-002 — API versioning ARCHITECTURE+API_CONTRACT: strategy first

```
source_id: xiigen-b12-l1-022-002
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Design the API versioning strategy and specify the OpenAPI migration contract."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 3
    ExecutionPlan:
      Phase1: ArchitectureFlow31 → versioning strategy (URL-based vs header-based, etc.)
      Phase2: APIContractFlow33 → OpenAPI versioning contract (deprecation spec)
      Phase3: DocumentationFlow21 → versioning migration guide
    QACheckpoint: after Phase3

---

## B12-L1-022-003 — API versioning IMPLEMENT: feature_addition for negotiation

```
source_id: xiigen-b12-l1-022-003
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Implement API version negotiation and write the response contract."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → version negotiation middleware code
        - APIContractFlow33 → version-specific response contract spec
      Phase2_Sequential: DocumentationFlow21 → version negotiation documentation
    QACheckpoint: after Phase2

---

## B12-L1-022-004 — API versioning: update Swagger (documentation) vs implement negotiation

```
source_id: xiigen-b12-l1-022-004
pair_kind: transition_orchestrator
```

Versioning TransitionType orchestration comparison:

"Update the Swagger spec to mark v1 as deprecated" (TransitionType: documentation)
  FlowOrder: [APIContractFlow33, DocumentationFlow21]
  ExecutionPlan: Phase1 (contract), Phase2 (docs), QA
  Output: updated-swagger.yaml + migration-notes.md

"Implement API version negotiation" (TransitionType: feature_addition)
  FlowOrder: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21]
  ExecutionPlan: Phase1 parallel (code + contract), Phase2 (docs), QA
  Output: version-negotiation.cs + versioned-contract.yaml + versioning-guide.md + tests

The orchestrator executes different patterns based on TransitionType.
"update" → documentation mode (no ImplementationFlow43).
"implement" → feature_addition mode (ImplementationFlow43 added at front).
