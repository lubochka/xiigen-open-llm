# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-015-001 — UserManagementAPI: documentation two flows

```
source_id: xiigen-b12-l1-015-001
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Specify the user management API contract including CRUD operations."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → user management API contract (CRUD endpoint specs)
      Phase2: DocumentationFlow21 → user management API reference documentation
    QACheckpoint: after Phase2

---

## B12-L1-015-002 — UserManagementAPI + SECURITY: three flows with authentication

```
source_id: xiigen-b12-l1-015-002
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  TransitionType: "documentation"
  user_request: "Document the user authentication API endpoints and token response schema."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Sequential: APIContractFlow33 → user auth API endpoint contracts
      Phase2_Parallel:
        - DocumentationFlow21 → user auth API reference docs
        - SecurityAuditFlow34 → authentication security requirements review
    QACheckpoint: after Phase2

---

## B12-L1-015-003 — UserManagementAPI ARCHITECTURE+API_CONTRACT: three flows, ARCH first

```
source_id: xiigen-b12-l1-015-003
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Design the user account management API and specify the endpoint contract."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Sequential: ArchitectureFlow31 → user management system architecture decisions
      Phase2_Sequential: APIContractFlow33 → user management API contract (informed by architecture)
      Phase3_Sequential: DocumentationFlow21 → documentation (informed by contract)
    QACheckpoint: after Phase3

Note: ARCHITECTURE (3.1) must execute FIRST (FlowOrder rule). API_CONTRACT (3.3) follows.
Then DOCUMENT (2.1) last. Full sequential — no parallel for three-flow architecture pattern.

---

## B12-L1-015-004 — UserManagementAPI IMPLEMENT: feature_addition

```
source_id: xiigen-b12-l1-015-004
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Implement the user registration API endpoint and write the OpenAPI spec."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → user registration endpoint code
        - APIContractFlow33 → OpenAPI spec for user registration
      Phase2_Sequential:
        - DocumentationFlow21 → user registration API documentation
    QACheckpoint: after Phase2
