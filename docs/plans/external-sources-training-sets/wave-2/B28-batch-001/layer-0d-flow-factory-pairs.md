# Layer 0-D — Flow Factory Pairs
# Batch: B28-batch-001
# Scenario: B28 — QA automation DevOps, sub-phase 7.1
# Contract: FlowFactoryUnit : ILogicalUnit<SubPhaseRouterResult, FlowInstanceDescriptor>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B28B1-WDA-factory-001 — QA/7.3 + IMPLEMENT/4.1 + TEST/5.3 → three flows

```
source_id: xiigen-b28-factory-001
transition_type: feature_addition
phase: 7
sub_phase: 7.3
pair_kind: factory_selection
```

A:
  sub_phase: [7.3, 4.1, 5.3]
  predicate: ["QA", "IMPLEMENT", "TEST"]
  compiled_text: "Implement the automated QA pipeline for the release process and build the smoke test harness."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "QATestingFlow73"
      predicate: QA
      sub_phase: 7.3
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
  SelectionReason: "Three predicates → three independent flow instances; all deterministic_generic"

---

## Pair B28B1-WDA-factory-002 — QA/7.3 + TEST/5.3 → two flows

```
source_id: xiigen-b28-factory-002
transition_type: feature_addition
phase: 7
sub_phase: 7.3
pair_kind: factory_selection
```

A:
  sub_phase: [7.3, 5.3]
  predicate: ["QA", "TEST"]
  compiled_text: "QA the deployment artifacts and verify the staging environment health checks."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "QATestingFlow73"
      predicate: QA
      sub_phase: 7.3
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
  SelectionReason: "Two predicates → two independent flow instances; both deterministic_generic"

---

## Pair B28B1-WDA-factory-003 — QA/7.3 + ARCHITECTURE/3.1 + TEST/5.1 → three flows

```
source_id: xiigen-b28-factory-003
transition_type: feature_addition
phase: 7
sub_phase: 7.3
pair_kind: factory_selection
```

A:
  sub_phase: [7.3, 3.1, 5.1]
  predicate: ["QA", "ARCHITECTURE", "TEST"]
  compiled_text: "Design the QA automation architecture and structure the test infrastructure layer."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "QATestingFlow73"
      predicate: QA
      sub_phase: 7.3
    - FlowId: "SystemArchitectureFlow31"
      predicate: ARCHITECTURE
      sub_phase: 3.1
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
  SelectionReason: "Three predicates → three independent flow instances; all deterministic_generic"

---

## Pair B28B1-WDA-factory-004 — zero predicates (CI/CD) → knowledge_edge_synthetic

```
source_id: xiigen-b28-factory-004
transition_type: unknown
phase: null
sub_phase: null
pair_kind: factory_selection
```

A:
  sub_phase: null
  predicate: null
  compiled_text: "Set up the CI/CD pipeline for continuous deployment to the staging environment."
  capability_gap_signal:
    gap_id: "gap-devops-cicd-001"
    DomainId: "software-development"

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; CI/CD, deploy, staging are all gap"
  NodeCount: 0
  CapabilityGapSignalForwarded: true
