# Layer 0-D — Flow Factory Pairs (013)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-013-001 — deterministic_generic: OrderWebhooks [API_CONTRACT, DOCUMENT]

```
source_id: xiigen-b12-0d-013-001
flow_type: deterministic_generic
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  domain: OrderWebhooks

B:
  FlowType: "deterministic_generic"
  SelectedFlow: "APIContractFlow33"
  SubPhase: "3.3"
  TransitionType: "documentation"
  SecondaryFlow: "DocumentationFlow21"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0D-013-002 — deterministic_generic: OrderWebhooks + SECURITY → three flows

```
source_id: xiigen-b12-0d-013-002
flow_type: deterministic_generic
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  sub_phase_hint: "3.3"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

---

## B12-0D-013-003 — deterministic_generic: IMPLEMENT+OrderWebhooks → feature_addition

```
source_id: xiigen-b12-0d-013-003
flow_type: deterministic_generic
transition_type: feature_addition
phase: 3
sub_phase: 3.3
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
  FactoryReason: "IMPLEMENT fires → feature_addition; ImplementationFlow43 added at front of FlowOrder."
