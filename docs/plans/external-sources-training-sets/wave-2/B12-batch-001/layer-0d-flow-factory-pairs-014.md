# Layer 0-D — Flow Factory Pairs (014)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-014-001 — deterministic_generic: InventoryAPI [API_CONTRACT, DOCUMENT]

```
source_id: xiigen-b12-0d-014-001
flow_type: deterministic_generic
transition_type: documentation
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"

B:
  FlowType: "deterministic_generic"
  SelectedFlow: "APIContractFlow33"
  SecondaryFlow: "DocumentationFlow21"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "documentation"

---

## B12-0D-014-002 — deterministic_generic: InventoryAPI [DOCUMENT] only → DocumentationFlow

```
source_id: xiigen-b12-0d-014-002
flow_type: deterministic_generic
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  sub_phase_hint: "2.1"
  context: "database schema — no API_CONTRACT"

B:
  FlowType: "deterministic_generic"
  SelectedFlow: "DocumentationFlow21"
  SubPhase: "2.1"
  TransitionType: "documentation"
  SecondaryFlow: null
  FlowOrder: ["DocumentationFlow21"]
  FactoryReason: "No API_CONTRACT → no APIContractFlow33. Single DOCUMENT predicate → DocumentationFlow21 only."

---

## B12-0D-014-003 — deterministic_generic: InventoryAPI IMPLEMENT → feature_addition

```
source_id: xiigen-b12-0d-014-003
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

## B12-0D-014-004 — Factory contrast: [API_CONTRACT, DOCUMENT] vs [DOCUMENT] only

```
source_id: xiigen-b12-0d-014-004
pair_kind: factory_reference
```

InventoryAPI factory selection comparison:

[API_CONTRACT, DOCUMENT] → "Document inventory API endpoints"
  SelectedFlows: [APIContractFlow33, DocumentationFlow21]
  Sub-phases covered: 3.3 + 2.1

[DOCUMENT] only → "Define database schema for inventory table"
  SelectedFlows: [DocumentationFlow21]
  Sub-phases covered: 2.1 only

Factory selection: API_CONTRACT predicate presence determines whether APIContractFlow33 is selected.
No API_CONTRACT → no APIContractFlow33 → no 3.3 execution.
