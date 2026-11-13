# Layer 0-D — Flow Factory Pairs (017)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-017-001 — deterministic_generic: ProductSearchAPI GraphQL [API_CONTRACT, DOCUMENT]

```
source_id: xiigen-b12-0d-017-001
flow_type: deterministic_generic
transition_type: documentation
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  domain: ProductSearch GraphQL

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0D-017-002 — deterministic_generic: ARCHITECTURE + API_CONTRACT (design GraphQL API)

```
source_id: xiigen-b12-0d-017-002
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

## B12-0D-017-003 — deterministic_generic: IMPLEMENT + GraphQL → feature_addition

```
source_id: xiigen-b12-0d-017-003
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

---

## B12-0D-017-004 — Factory note: GraphQL vs REST produce same flow selection

```
source_id: xiigen-b12-0d-017-004
pair_kind: factory_reference
```

APIContractFlow33 is API-style agnostic. Factory selection depends on predicates, not API style.

GraphQL request: [API_CONTRACT, DOCUMENT] → APIContractFlow33 + DocumentationFlow21
REST request: [API_CONTRACT, DOCUMENT] → APIContractFlow33 + DocumentationFlow21

Same flows selected. APIContractFlow33 internally handles the GraphQL vs REST distinction
(schema.graphql output vs openapi.yaml output), but the factory selection is identical.
