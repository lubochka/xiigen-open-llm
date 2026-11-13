# Layer 0-D — Flow Factory Pairs (009)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-009-001 — Full five-predicate factory: IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY+AUTHORIZATION

```
source_id: xiigen-b12-0d-009-001
flow_type: deterministic_generic
transition_type: feature_addition
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-009-001"
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  sub_phase_hint: "3.3"
  request: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0 token endpoint, and document the security requirements."

B:
  FlowFactoryResult:
    FlowType: "deterministic_generic"
    TransitionType: "feature_addition"
    TotalFlows: 5
    PrimarySubPhase: "3.3"
    DecisionCount: 5
    SelectedFlows:
      - { FlowId: "ImplementationFlow43", SubPhase: "4.3", Predicate: "IMPLEMENT", Order: 1 }
      - { FlowId: "APIContractFlow33", SubPhase: "3.3", Predicate: "API_CONTRACT", Order: 2 }
      - { FlowId: "DocumentationFlow21", SubPhase: "2.1", Predicate: "DOCUMENT", Order: 3 }
      - { FlowId: "SecurityAuditFlow34", SubPhase: "3.4", Predicate: "SECURITY", Order: 4 }
      - { FlowId: "AuthorizationFlow34", SubPhase: "3.4", Predicate: "AUTHORIZATION", Order: 5 }
    FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34", "AuthorizationFlow34"]
    ParallelGroups:
      Sequential: ["ImplementationFlow43", "APIContractFlow33"]
      Parallel: ["DocumentationFlow21", "SecurityAuditFlow34", "AuthorizationFlow34"]
    TransitionType: "feature_addition"
    FactoryReason: "Five predicates → five flows. IMPLEMENT → feature_addition. OAuth fires both
    SECURITY and AUTHORIZATION → two separate 3.4 flows. ImplementationFlow43 and APIContractFlow33
    run sequentially. Final three flows can run in parallel after spec is complete."

---

## B12-0D-009-002 — Five-flow parallel execution analysis

```
source_id: xiigen-b12-0d-009-002
pair_kind: factory_reference
```

Parallel execution analysis for five-flow factory:

Sequential phase:
  1. ImplementationFlow43 — creates the OAuth token endpoint implementation
  2. APIContractFlow33 — defines the OpenAPI specification structure
  (These two can potentially run in parallel if implementation and spec are independent tasks)

Parallel phase (after spec is complete):
  3. DocumentationFlow21 — documents the spec and implementation
  4. SecurityAuditFlow34 — reviews token/authentication security
  5. AuthorizationFlow34 — reviews OAuth scopes and authorization flows

Why 3-4-5 can run in parallel:
  - DocumentationFlow21: reads the spec → writes documentation (reads only)
  - SecurityAuditFlow34: reads authentication portions of spec → produces security notes
  - AuthorizationFlow34: reads authorization portions of spec → produces auth review
  All three are read-only consumers of the APIContractFlow33 output → no conflict.

---

## B12-0D-009-003 — Five-predicate factory: DecisionCount vs FlowCount

```
source_id: xiigen-b12-0d-009-003
pair_kind: factory_reference
```

For [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]:

  DecisionCount = 5 (one per predicate, Core Rule 23)
  FlowCount = 5 (one flow per predicate)
  UniqueSubPhaseCount = 4 (4.3, 3.3, 2.1, 3.4)

Note: SECURITY and AUTHORIZATION both route to 3.4 but use DIFFERENT flows at 3.4:
  SECURITY → SecurityAuditFlow34
  AUTHORIZATION → AuthorizationFlow34

So UniqueSubPhaseCount = 4 but FlowCount = 5 because 3.4 has two distinct flows.

---

## B12-0D-009-004 — Factory map reference: all predicate combinations to flow compositions

```
source_id: xiigen-b12-0d-009-004
pair_kind: factory_reference
```

Complete B12 factory map:

| CompiledPredicateIds | FlowOrder | TransitionType | DecisionCount |
|---------------------|-----------|----------------|---------------|
| [API_CONTRACT] | [APIContractFlow33] | documentation | 1 |
| [DOCUMENT] | [DocumentationFlow21] | documentation | 1 |
| [API_CONTRACT, DOCUMENT] | [APIContractFlow33, DocumentationFlow21] | documentation | 2 |
| [ARCHITECTURE] | [ArchitectureFlow31] | documentation | 1 |
| [ARCHITECTURE, DOCUMENT] | [ArchitectureFlow31, DocumentationFlow21] | documentation | 2 |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT] | [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21] | documentation | 3 |
| [API_CONTRACT, DOCUMENT, SECURITY] | [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] | documentation | 3 |
| [API_CONTRACT, DOCUMENT, AUTHORIZATION] | [APIContractFlow33, DocumentationFlow21, AuthorizationFlow34] | documentation | 3 |
| [IMPLEMENT, API_CONTRACT] | [ImplementationFlow43, APIContractFlow33] | feature_addition | 2 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21] | feature_addition | 3 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] | feature_addition | 4 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION] | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34] | feature_addition | 5 |
