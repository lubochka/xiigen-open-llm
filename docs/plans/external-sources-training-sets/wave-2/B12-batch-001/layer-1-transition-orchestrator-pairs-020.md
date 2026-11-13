# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-020-001 — FileUploadAPI: documentation, two flows

```
source_id: xiigen-b12-l1-020-001
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Specify the file upload API endpoint contract with multipart request format."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → file upload endpoint contract (multipart form spec)
      Phase2: DocumentationFlow21 → file upload API documentation
    QACheckpoint: after Phase2

---

## B12-L1-020-002 — FileUploadAPI + SECURITY: authentication requirements

```
source_id: xiigen-b12-l1-020-002
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  TransitionType: "documentation"
  user_request: "Document the file upload REST API authentication requirements and signed URL contract."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Sequential: APIContractFlow33 → upload contract + signed URL specification
      Phase2_Parallel:
        - DocumentationFlow21 → upload API documentation
        - SecurityAuditFlow34 → authentication requirements review (signed URL security)
    QACheckpoint: after Phase2

---

## B12-L1-020-003 — FileUploadAPI IMPLEMENT (create REST contract)

```
source_id: xiigen-b12-l1-020-003
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  user_request: "Create the REST API contract and response schema for the document upload service."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → document upload REST endpoint code
        - APIContractFlow33 → upload contract + response schema
      Phase2_Sequential: DocumentationFlow21 → upload API documentation
    QACheckpoint: after Phase2

---

## B12-L1-020-004 — FileUploadAPI: idempotency in orchestration

```
source_id: xiigen-b12-l1-020-004
pair_kind: transition_orchestrator
```

When "idempotent" fires API_CONTRACT in a file upload contract request:
  "Define the idempotent upload contract for the file storage API"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  FlowOrder: [APIContractFlow33, DocumentationFlow21]

The idempotency concern is handled INSIDE APIContractFlow33:
  - N04 (DomainProcessorUnit) specifies idempotency behavior per endpoint
  - N05 (ApplierUnit) adds idempotency key header to contract spec
  - N06 (ResultTranslatorUnit) includes idempotency guarantees in contract summary

Idempotency is an API contract content concern, not an orchestration routing concern.
The routing is the same with or without "idempotent" in the request.
