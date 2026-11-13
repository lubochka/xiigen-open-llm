# Layer 0-D — Flow Factory Pairs (015)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-015-001 — deterministic_generic: UserManagementAPI [API_CONTRACT, DOCUMENT]

```
source_id: xiigen-b12-0d-015-001
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

## B12-0D-015-002 — deterministic_generic: UserManagementAPI SECURITY → three flows

```
source_id: xiigen-b12-0d-015-002
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

## B12-0D-015-003 — deterministic_generic: ARCHITECTURE + API_CONTRACT → three flows

```
source_id: xiigen-b12-0d-015-003
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
  FactoryReason: "ARCHITECTURE fires → ArchitectureFlow31 added at front of FlowOrder before APIContractFlow33."

---

## B12-0D-015-004 — UserManagementAPI: IMPLEMENT + ARCHITECTURE + API_CONTRACT → four flows

```
source_id: xiigen-b12-0d-015-004
flow_type: deterministic_generic
transition_type: feature_addition
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"

B:
  FlowType: "deterministic_generic"
  SelectedFlows: ["ImplementationFlow43", "ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]
  FactoryReason: "IMPLEMENT → feature_addition; four-flow selection for four-predicate request."
