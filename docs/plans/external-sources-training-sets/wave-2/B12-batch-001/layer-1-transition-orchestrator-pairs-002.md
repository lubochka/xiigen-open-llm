# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-002-001 — API_CONTRACT + DOCUMENT: documentation TransitionType, parallel eligible

```
source_id: xiigen-b12-l1-002-001
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: transition_orchestrator
```

A:
  run_id: "B12-L1-002-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]
  user_request: "Define the gRPC service contract and protobuf schema for the user service."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    PrimarySubPhase: "3.3"
    TotalFlows: 2
    ExecutionPlan:
      Phase1_Sequential:
        - Flow: "APIContractFlow33"
          SubPhase: "3.3"
          Inputs: ["user_request", "domain_context"]
          Outputs: ["gRPC contract spec", "protobuf schema definition"]
          ParallelEligible: false  (must run first — primary flow)
      Phase2_Sequential:
        - Flow: "DocumentationFlow21"
          SubPhase: "2.1"
          Inputs: ["APIContractFlow33.outputs"]
          Outputs: ["gRPC contract documentation"]
          ParallelEligible: false  (depends on APIContractFlow33 output)
    QACheckpoint: "after all flows complete"
    EstimatedOutputs: 2-3 files

Orchestrator note: two-flow sequence for [API_CONTRACT, DOCUMENT].
DocumentationFlow21 depends on APIContractFlow33 output → must run sequentially after.
No parallel execution for this two-flow pattern (second depends on first).

---

## B12-L1-002-002 — API_CONTRACT + DOCUMENT: three scenarios with different content types

```
source_id: xiigen-b12-l1-002-002
pair_kind: transition_orchestrator
```

The [API_CONTRACT, DOCUMENT] two-flow pattern handles three B12 content types:

Content type 1: gRPC contract
  APIContractFlow33 produces: .proto file or contract specification
  DocumentationFlow21 produces: gRPC contract documentation with message field descriptions

Content type 2: webhook contract
  APIContractFlow33 produces: webhook payload schema and event contract
  DocumentationFlow21 produces: webhook integration guide and event documentation

Content type 3: data contracts
  APIContractFlow33 produces: inter-service data contract specification
  DocumentationFlow21 produces: data contract documentation for service teams

All three use the same FlowOrder: [APIContractFlow33, DocumentationFlow21]
The flow IDs are the same; the content produced differs based on request context.

---

## B12-L1-002-003 — Documentation TransitionType: what "documentation" means for orchestrator

```
source_id: xiigen-b12-l1-002-003
pair_kind: transition_orchestrator
```

TransitionType: documentation orchestrator behavior:

Purpose: update, annotate, or specify an EXISTING artifact or pattern
  vs. feature_addition: CREATE a new artifact

Orchestrator behavior for documentation TransitionType:
  - Checks existing documentation artifacts in repository
  - Updates or creates documentation files that describe existing system
  - Does NOT trigger "new feature implementation" scaffolding
  - Does NOT create new service endpoints or code modules
  - MAY create new specification files (OpenAPI .yaml, .proto, .graphql) as documentation

For [API_CONTRACT, DOCUMENT] with documentation TransitionType:
  - Creates or updates: API specification files
  - Creates or updates: Contract documentation files
  - Does NOT create: new service implementations
  - Does NOT create: new database tables or migrations

---

## B12-L1-002-004 — Orchestrator output structure for [API_CONTRACT, DOCUMENT]

```
source_id: xiigen-b12-l1-002-004
pair_kind: transition_orchestrator
```

Expected orchestrator output for [API_CONTRACT, DOCUMENT] documentation:

Primary output from APIContractFlow33 (3.3):
  - Contract specification file (format depends on context):
    - REST: openapi.yaml or swagger.json
    - gRPC: service.proto
    - GraphQL: schema.graphql
    - Webhook: webhook-contract.json or webhook-spec.md
    - Data contracts: data-contracts.md or data-contracts.yaml

Secondary output from DocumentationFlow21 (2.1):
  - Developer-facing documentation file:
    - API reference documentation
    - Integration guide
    - Contract change log

Minimum output: 2 files (one per flow)
Typical output: 3-4 files (spec + documentation + supporting files)

QA checkpoint: both flows complete before QA review
QA checks: contract completeness, documentation accuracy, no conflicting specifications
