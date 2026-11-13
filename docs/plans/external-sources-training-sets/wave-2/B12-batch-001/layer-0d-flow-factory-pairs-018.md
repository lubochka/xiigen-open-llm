# Layer 0-D — Flow Factory Pairs (018)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-018-001 — deterministic_generic: ReportingAPI [API_CONTRACT, DOCUMENT]

```
source_id: xiigen-b12-0d-018-001
flow_type: deterministic_generic
transition_type: documentation
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0D-018-002 — deterministic_generic: ReportingAPI [DOCUMENT] only (no API terms)

```
source_id: xiigen-b12-0d-018-002
flow_type: deterministic_generic
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  sub_phase_hint: "2.1"
  context: "Document reporting metrics — no API terms"

B:
  FlowType: "deterministic_generic"
  SelectedFlow: "DocumentationFlow21"
  TransitionType: "documentation"
  FlowOrder: ["DocumentationFlow21"]
  FactoryReason: "No API_CONTRACT predicate → no APIContractFlow33 selected."

---

## B12-0D-018-003 — deterministic_generic: ReportingAPI IMPLEMENT (scaffold controller)

```
source_id: xiigen-b12-0d-018-003
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
