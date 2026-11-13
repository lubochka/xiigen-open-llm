# Layer 0-D — Flow Factory Pairs (007)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-007-001 — IMPLEMENT + API_CONTRACT + DOCUMENT factory: feature_addition flows

```
source_id: xiigen-b12-0d-007-001
flow_type: deterministic_generic
transition_type: feature_addition
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-007-001"
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  request: "Write the OpenAPI specification for the cart service."

B:
  FlowFactoryResult:
    FlowType: "deterministic_generic"
    TransitionType: "feature_addition"
    TotalFlows: 3
    PrimarySubPhase: "3.3"
    SelectedFlows:
      - { FlowId: "ImplementationFlow43", SubPhase: "4.3", Predicate: "IMPLEMENT", Order: 1 }
      - { FlowId: "APIContractFlow33", SubPhase: "3.3", Predicate: "API_CONTRACT", Order: 2 }
      - { FlowId: "DocumentationFlow21", SubPhase: "2.1", Predicate: "DOCUMENT", Order: 3 }
    FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]
    TransitionType: "feature_addition"
    FactoryReason: "IMPLEMENT fires → feature_addition. Three flows for three predicates.
    ImplementationFlow43 first (creating the new artifact), APIContractFlow33 second (defining
    the contract structure), DocumentationFlow21 last (capturing the result)."

---

## B12-0D-007-002 — IMPLEMENT + API_CONTRACT: two-flow factory (no DOCUMENT)

```
source_id: xiigen-b12-0d-007-002
flow_type: deterministic_generic
transition_type: feature_addition
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  (no DOCUMENT predicate — no documentation verb in request)

B:
  FlowFactoryResult:
    FlowType: "deterministic_generic"
    TransitionType: "feature_addition"
    SelectedFlows:
      - { FlowId: "ImplementationFlow43", SubPhase: "4.3", Predicate: "IMPLEMENT", Order: 1 }
      - { FlowId: "APIContractFlow33", SubPhase: "3.3", Predicate: "API_CONTRACT", Order: 2 }
    FlowOrder: ["ImplementationFlow43", "APIContractFlow33"]
    TransitionType: "feature_addition"

---

## B12-0D-007-003 — Factory: ImplementationFlow43 description for B12 context

```
source_id: xiigen-b12-0d-007-003
flow_type: deterministic_generic
pair_kind: factory_selection
```

ImplementationFlow43 in B12 API contract context:

ImplementationFlow43 handles code/artifact creation for API-related items:
  - Writing new OpenAPI specification files (.yaml/.json)
  - Writing new .proto files for gRPC contracts
  - Creating new GraphQL schema files (.graphql)
  - Writing webhook handler code
  - Creating new API endpoint implementations

In the B12 scenario, ImplementationFlow43 + APIContractFlow33 work together:
  - ImplementationFlow43: creates the actual specification file/code artifact
  - APIContractFlow33: validates the API contract structure and documentation standards

When "write the OpenAPI spec" is requested:
  IMPLEMENT fires on "write" → ImplementationFlow43 creates the spec file
  API_CONTRACT fires on "openapi" → APIContractFlow33 structures the contract
  Combined: both flows run to produce a correctly structured, validated spec file

---

## B12-0D-007-004 — Complete factory map for IMPLEMENT-containing patterns

```
source_id: xiigen-b12-0d-007-004
pair_kind: factory_reference
```

Factory output for all IMPLEMENT-containing predicate patterns in B12:

[IMPLEMENT, API_CONTRACT]:
  Flows: [ImplementationFlow43, APIContractFlow33]
  TransitionType: feature_addition
  Notes: two-flow pattern, no documentation sub-phase

[IMPLEMENT, API_CONTRACT, DOCUMENT]:
  Flows: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21]
  TransitionType: feature_addition
  Notes: standard three-flow implementation pattern

[IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY]:
  Flows: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]
  TransitionType: feature_addition
  Notes: four-flow pattern with security review

[IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]:
  Flows: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34]
  TransitionType: feature_addition
  Notes: five-flow pattern; SecurityAuditFlow34 and AuthorizationFlow34 can run in parallel

[ARCHITECTURE, IMPLEMENT, API_CONTRACT, DOCUMENT]:
  Flows: [ArchitectureFlow31, ImplementationFlow43, APIContractFlow33, DocumentationFlow21]
  TransitionType: feature_addition
  Notes: when "design" appears with "implement/write/build"; architecture planned first
  Sub-phases: 3.1 + 4.3 + 3.3 + 2.1
