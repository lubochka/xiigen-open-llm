# Layer 0-D — Flow Factory Pairs (022)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-022-001 — deterministic_generic: API versioning [API_CONTRACT, DOCUMENT]

```
source_id: xiigen-b12-0d-022-001
flow_type: deterministic_generic
transition_type: documentation
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  context: "API versioning contract documentation"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0D-022-002 — deterministic_generic: API versioning ARCHITECTURE+API_CONTRACT

```
source_id: xiigen-b12-0d-022-002
flow_type: deterministic_generic
transition_type: documentation
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0D-022-003 — deterministic_generic: API versioning IMPLEMENT (version negotiation)

```
source_id: xiigen-b12-0d-022-003
flow_type: deterministic_generic
transition_type: feature_addition
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]
