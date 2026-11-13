# Layer 0-D — Flow Factory Pairs
# Batch: B14-batch-001
# Scenario: B14 — DevOps cloud infrastructure, sub-phase 3.5
# Contract: FlowFactoryUnit : ILogicalUnit<SubPhaseRouterResult, FlowInstanceDescriptor>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B14B1-WDA-factory-001 — ARCHITECTURE / 3.5 → deterministic_generic

```
source_id: xiigen-b14-factory-001
transition_type: feature_addition
phase: 3
sub_phase: 3.5
pair_kind: factory_selection
```

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the cloud infrastructure architecture for the microservices deployment layer."
  capability_gap_warnings: ["gap-devops-cloud-001"]

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  FlowLabel: "Infrastructure and DevOps Architecture Flow — sub-phase 3.5"
  SelectionReason: "ARCHITECTURE predicate at 3.5 with infrastructure/deployment context —
    deterministic_generic; gap warnings do not degrade trust state"
  NodeCount: 9

---

## Pair B14B1-WDA-factory-002 — ARCHITECTURE / 3.5 + IMPLEMENT / 4.1 → two flows

```
source_id: xiigen-b14-factory-002
transition_type: feature_addition
phase: 3
sub_phase: 3.5
pair_kind: factory_selection
```

A:
  sub_phase: [3.5, 4.1]
  predicate: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the service mesh layer and implement the network policy components for the Kubernetes cluster."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "Two predicates → two independent flow instances; both deterministic_generic"

---

## Pair B14B1-WDA-factory-003 — ARCHITECTURE / 3.5 + DOCUMENT / 6.2 → two flows

```
source_id: xiigen-b14-factory-003
transition_type: documentation
phase: 3
sub_phase: 3.5
pair_kind: factory_selection
```

A:
  sub_phase: [3.5, 6.2]
  predicate: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Document the cloud architecture blueprint and diagram the infrastructure components."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
  SelectionReason: "Two predicates → two independent flow instances; both deterministic_generic"

---

## Pair B14B1-WDA-factory-004 — zero predicates (scale/cloud platform) → knowledge_edge_synthetic

```
source_id: xiigen-b14-factory-004
transition_type: unknown
phase: null
sub_phase: null
pair_kind: factory_selection
```

A:
  sub_phase: null
  predicate: null
  compiled_text: "Set up AWS ECS for the containerized microservices."
  capability_gap_signal:
    gap_id: "gap-devops-cloud-001"
    DomainId: "software-development"

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; AWS, ECS, set up, containerized are all gap"
  NodeCount: 0
  CapabilityGapSignalForwarded: true
