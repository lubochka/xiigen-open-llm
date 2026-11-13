# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-016-001 — ShippingRatesAPI: documentation, two flows

```
source_id: xiigen-b12-l1-016-001
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Specify the REST endpoint contract for the shipping rate calculation service."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → shipping rate REST endpoint contracts
      Phase2: DocumentationFlow21 → shipping rate API documentation
    QACheckpoint: after Phase2

---

## B12-L1-016-002 — ShippingRatesAPI IMPLEMENT: feature_addition

```
source_id: xiigen-b12-l1-016-002
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Write the Swagger documentation for the shipping rates API."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → Swagger spec file (writing = code creation)
        - APIContractFlow33 → shipping rates contract definition
      Phase2_Sequential: DocumentationFlow21 → supplementary documentation
    QACheckpoint: after Phase2

---

## B12-L1-016-003 — ShippingRatesAPI ARCHITECTURE+API_CONTRACT: design then specify

```
source_id: xiigen-b12-l1-016-003
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Design the shipping rates service interface and define the API contract."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 3
    ExecutionPlan:
      Phase1: ArchitectureFlow31 → service interface architecture decisions
      Phase2: APIContractFlow33 → shipping rates API contract (informed by architecture)
      Phase3: DocumentationFlow21 → documentation
    QACheckpoint: after Phase3

---

## B12-L1-016-004 — ShippingRatesAPI + SECURITY: three flows with auth review

```
source_id: xiigen-b12-l1-016-004
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  TransitionType: "documentation"
  user_request: "Specify the carrier webhook contract and authentication requirements."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Sequential: APIContractFlow33 → carrier webhook contract
      Phase2_Parallel:
        - DocumentationFlow21 → webhook documentation
        - SecurityAuditFlow34 → authentication requirements review
    QACheckpoint: after Phase2
