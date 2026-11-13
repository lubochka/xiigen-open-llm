# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-004-001 — IMPLEMENT + API_CONTRACT: feature_addition (new API being built)

```
source_id: xiigen-b12-l1-004-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
pair_kind: transition_orchestrator
```

A:
  run_id: "B12-L1-004-001"
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33"]
  user_request: "Implement the REST API endpoint for product search."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    PrimarySubPhase: "3.3"
    TotalFlows: 2
    ExecutionPlan:
      Phase1_Parallel:
        - Flow: "ImplementationFlow43"
          SubPhase: "4.3"
          Inputs: ["user_request", "architecture_context", "product_domain_model"]
          Outputs: ["endpoint implementation", "route definition", "request/response types"]
          ParallelWith: "APIContractFlow33"
          Note: "Creates the actual endpoint code — the new artifact"
        - Flow: "APIContractFlow33"
          SubPhase: "3.3"
          Inputs: ["user_request", "endpoint_requirements"]
          Outputs: ["REST endpoint specification", "API contract document"]
          ParallelWith: "ImplementationFlow43"
          Note: "Defines the contract structure parallel with implementation"
    QACheckpoint: "after both flows complete — verify contract matches implementation"
    EstimatedOutputs: 2-4 files

Orchestrator note: ImplementationFlow43 and APIContractFlow33 are parallel-eligible for
feature_addition. In practice, spec and implementation can develop simultaneously.
QA must verify that the generated spec matches the implementation.

---

## B12-L1-004-002 — feature_addition vs documentation: orchestrator behavior difference

```
source_id: xiigen-b12-l1-004-002
pair_kind: transition_orchestrator
```

Comparison of orchestrator behavior:

documentation TransitionType:
  - Purpose: describe/update existing system
  - Creates: specification and documentation files
  - Does NOT create: new service code, new database tables
  - QA check: documentation completeness, accuracy

feature_addition TransitionType:
  - Purpose: create new system artifact
  - Creates: new endpoint code, new service modules, new spec files
  - MAY also create: documentation to accompany the new feature
  - QA check: implementation correctness, spec-to-code alignment

For [IMPLEMENT, API_CONTRACT] feature_addition:
  ImplementationFlow43 creates: new endpoint code files
  APIContractFlow33 creates: accompanying API contract specification
  Combined output: both code AND specification (new artifacts)

For [API_CONTRACT, DOCUMENT] documentation:
  APIContractFlow33 creates: API specification (may be new spec file for existing endpoint)
  DocumentationFlow21 creates: documentation (describes existing or newly specified API)
  Combined output: documentation files (no new service code)

---

## B12-L1-004-003 — IMPLEMENT + API_CONTRACT: what "new API being built" means

```
source_id: xiigen-b12-l1-004-003
pair_kind: transition_orchestrator
```

"New API being built" (feature_addition with IMPLEMENT + API_CONTRACT):

Scenario A: Brand new endpoint
  "Implement the product search REST API endpoint"
  → Endpoint does not exist yet → IMPLEMENT creates it
  → API_CONTRACT specifies the contract for the new endpoint
  → TransitionType: feature_addition (new code artifact created)

Scenario B: New version of existing endpoint
  "Build the v2 customer account endpoint with backward compatibility"
  → v2 endpoint does not exist → IMPLEMENT creates it
  → API_CONTRACT specifies the v2 contract
  → TransitionType: feature_addition (new code artifact even if v1 existed)

Scenario C: New gRPC service
  "Implement the gRPC user authentication service with protobuf contract"
  → gRPC service does not exist → IMPLEMENT creates it
  → API_CONTRACT specifies the .proto contract
  → TransitionType: feature_addition

---

## B12-L1-004-004 — Orchestrator: QA last rule in API context

```
source_id: xiigen-b12-l1-004-004
pair_kind: transition_orchestrator
```

QA position rule for B12 orchestration:

QA checkpoint always runs AFTER all flows in FlowOrder complete.
QA is never inserted between flows.
QA is never the first step.

For [IMPLEMENT, API_CONTRACT]:
  1. ImplementationFlow43 (parallel with APIContractFlow33)
  2. APIContractFlow33 (parallel with ImplementationFlow43)
  3. QA: verify implementation matches contract specification

For [API_CONTRACT, DOCUMENT, SECURITY]:
  1. APIContractFlow33
  2. DocumentationFlow21 (parallel with SecurityAuditFlow34)
  3. SecurityAuditFlow34 (parallel with DocumentationFlow21)
  4. QA: verify documentation completeness, security requirements met

For [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]:
  1. ImplementationFlow43 (parallel with APIContractFlow33)
  2. APIContractFlow33 (parallel with ImplementationFlow43)
  3. DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34 (all parallel)
  4. QA: full review of all outputs

QA is ALWAYS last. This rule has no exceptions in B12 orchestration.
