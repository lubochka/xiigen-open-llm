# Layer 0-D — Flow Factory Pairs (023)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-023-001 — deterministic_generic: IMPLEMENT+API_CONTRACT (build product catalog REST)

```
source_id: xiigen-b12-0d-023-001
flow_type: deterministic_generic
transition_type: feature_addition
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  context: "Build REST API — IMPLEMENT fires"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]
  FactoryReason: "IMPLEMENT fires via 'build' verb → ImplementationFlow43 added to FlowOrder. TransitionType: feature_addition."

---

## B12-0D-023-002 — deterministic_generic: IMPLEMENT+API_CONTRACT (create GraphQL schema)

```
source_id: xiigen-b12-0d-023-002
flow_type: deterministic_generic
transition_type: feature_addition
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  context: "Create GraphQL schema — IMPLEMENT fires via 'create'"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0D-023-003 — deterministic_generic: documentation (update API contract — no IMPLEMENT)

```
source_id: xiigen-b12-0d-023-003
flow_type: deterministic_generic
transition_type: documentation
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  context: "Update API contract documentation — 'update' not IMPLEMENT"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]
  FactoryReason: "No IMPLEMENT predicate → ImplementationFlow43 not selected. documentation TransitionType."
