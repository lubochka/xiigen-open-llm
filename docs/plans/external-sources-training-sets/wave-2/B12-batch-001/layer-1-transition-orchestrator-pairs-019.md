# Layer 1 — Transition Orchestrator Pairs (019)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-019-001 — NotificationAPI: documentation, two flows

```
source_id: xiigen-b12-l1-019-001
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Define the notification webhook contract for push delivery events."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → notification webhook contract
      Phase2: DocumentationFlow21 → webhook integration documentation
    QACheckpoint: after Phase2

---

## B12-L1-019-002 — NotificationAPI + SECURITY: three flows with auth

```
source_id: xiigen-b12-l1-019-002
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  TransitionType: "documentation"
  user_request: "Document the notification API authentication scheme and webhook requirements."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Sequential: APIContractFlow33 → notification API contract + webhook spec
      Phase2_Parallel:
        - DocumentationFlow21 → notification API docs
        - SecurityAuditFlow34 → authentication scheme review
    QACheckpoint: after Phase2

---

## B12-L1-019-003 — NotificationAPI IMPLEMENT: feature_addition

```
source_id: xiigen-b12-l1-019-003
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Build the REST API for the notification service and write the contract."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → notification REST API implementation
        - APIContractFlow33 → notification endpoint contract definition
      Phase2_Sequential: DocumentationFlow21 → notification API documentation
    QACheckpoint: after Phase2

---

## B12-L1-019-004 — NotificationAPI: webhook vs gRPC orchestration (same flow, different content)

```
source_id: xiigen-b12-l1-019-004
pair_kind: transition_orchestrator
```

NotificationAPI webhook request:
  "Define notification webhook contract for push events"
  FlowOrder: [APIContractFlow33, DocumentationFlow21]
  APIContractFlow33 output: webhook-payload-contract.json

NotificationAPI gRPC request:
  "Define gRPC service contract for notification delivery"
  FlowOrder: [APIContractFlow33, DocumentationFlow21]
  APIContractFlow33 output: notification-service.proto

Same FlowOrder. Same orchestration plan. Different output format.
APIContractFlow33 detects "webhook" vs "gRPC" from the user request context
and produces the appropriate contract artifact format.
