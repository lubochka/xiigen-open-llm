# Layer 0-D — Flow Factory Pairs (021)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-021-001 — deterministic_generic: PartnerAPI [API_CONTRACT, DOCUMENT]

```
source_id: xiigen-b12-0d-021-001
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

## B12-0D-021-002 — deterministic_generic: PartnerAPI ARCHITECTURE+API_CONTRACT

```
source_id: xiigen-b12-0d-021-002
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

## B12-0D-021-003 — deterministic_generic: PartnerAPI SECURITY (partner auth)

```
source_id: xiigen-b12-0d-021-003
flow_type: deterministic_generic
transition_type: documentation
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

## B12-0D-021-004 — deterministic_generic: PartnerAPI IMPLEMENT (build webhook)

```
source_id: xiigen-b12-0d-021-004
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
