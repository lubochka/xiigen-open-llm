# Layer 0-D — Flow Factory Selection Pairs
# Batch: O1-batch-001
# Scenario: O1 — multi-tenant JWT authentication
# Contract: ILogicalFlowFactory<TInput> — flow factory selection surface
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: adaptive_routing_flow_factory

---

## Method note

Layer 0-D pairs teach the model the four factory selection outcomes defined
in EXAMINATION-METHOD-ORCHESTRATION-v7.md:

  Pair 001: Trusted learned flow — a specific domain-trained flow is registered
    for the sub-phase (e.g. SecurityVerification34Flow for 3.4). Selection is
    authoritative; factory produces this flow without fallback.

  Pair 002: Deterministic generic flow — no specific flow registered; the seeder's
    UniversalNineNodeDevelopmentFlow is the registered factory output. Used as
    a placeholder until domain learning replaces it.

  Pair 003: Rejected fallback — a candidate flow is found but its trust state
    is "probationary" and it has not passed Gate B. Factory rejects it and falls
    back to the generic nine-node flow. A Warnings entry records the rejection.

  Pair 004: Probationary accepted — a candidate flow passes Gate B metrics but
    carries a PROBATIONARY trust label. Factory accepts it with a warning that
    production promotion has not occurred.

The ILogicalFlowFactory<TInput> contract produces a flow instance or a
structured rejection result. It does not throw and does not return null.

---

## Coverage map

| Pair ID | Sub-phase | Trust state | Factory decision |
|---|---|---|---|
| O1B1-WDA-factory-001 | 3.4 | trusted_learned | Accept specific flow |
| O1B1-WDA-factory-002 | 3.3 | deterministic_generic | Accept generic nine-node |
| O1B1-WDA-factory-003 | 5.1 | probationary_rejected | Reject; fall back to generic |
| O1B1-WDA-factory-004 | 3.3 | probationary_accepted | Accept with PROBATIONARY warning |

---

## Pair O1B1-WDA-factory-001 — trusted learned flow selected for 3.4

```
source_id: xiigen-o1-factory-001
source_title: XIIGen O1 factory — trusted learned flow for security-verification 3.4
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — trusted_learned selection
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: adaptive_routing_flow_factory
node_role: flow-factory-selector
coverage_status: covered
pair_kind: adaptive_routing_flow_factory
```

A:
  sub_phase_id: "3.4"
  flow_registry_state:
    registered_flow_id: "security-verification-flow-3.4"
    flow_type: "SecurityVerification34FlowAdapter"
    trust_state: "trusted_learned"
    gate_b_passed: true
    version: "1.0.0"
  routing_confidence: 0.92
  evidence_predicates: ["SECURITY", "AUTHORIZATION"]

B:
  SelectedFlowId: "security-verification-flow-3.4"
  FlowType: "SecurityVerification34FlowAdapter"
  SelectionBasis: "trusted_learned"
  TrustState: "trusted_learned"
  GateBPassed: true
  FallbackUsed: false
  Warnings: []
  [unit result envelope]
    UnitId: "unit-flow-factory-selector-v1"
    UnitRole: "flow-factory-selector"
    SubPhaseId: "3.4"
    Confidence: 0.92
    SelectionOutcome: "ACCEPTED_TRUSTED_LEARNED"

Factory evidence:
  CreateDefault() registers SecurityVerification34FlowAdapter before calling
  DevelopmentLifecycleTopologySeeder.Seed(). The seeder finds hasSpecificFlow=true
  for 3.4 and skips registration of a generic nine-node flow.
  The factory resolves "security-verification-flow-3.4" as a trusted learned flow
  with full Gate B evidence from the CB4 external-source training batch.

---

## Pair O1B1-WDA-factory-002 — deterministic generic nine-node flow for 3.3

```
source_id: xiigen-o1-factory-002
source_title: XIIGen O1 factory — generic nine-node flow for api-contract 3.3
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — deterministic_generic selection
transition_type: feature_addition
phase: 3
sub_phase: 3.3
task_type: adaptive_routing_flow_factory
node_role: flow-factory-selector
coverage_status: covered
pair_kind: adaptive_routing_flow_factory
```

A:
  sub_phase_id: "3.3"
  flow_registry_state:
    registered_flow_id: "universal-nine-node-development-flow-3.3"
    flow_type: "UniversalNineNodeDevelopmentFlow"
    trust_state: "deterministic_generic"
    gate_b_passed: false
    version: "1.0.0"
  routing_confidence: 0.92
  evidence_predicates: ["API_CONTRACT"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.3"
  FlowType: "UniversalNineNodeDevelopmentFlow"
  SelectionBasis: "deterministic_generic"
  TrustState: "deterministic_generic"
  GateBPassed: false
  FallbackUsed: false
  Warnings: ["generic_flow_pending_domain_learning:3.3"]
  [unit result envelope]
    UnitId: "unit-flow-factory-selector-v1"
    UnitRole: "flow-factory-selector"
    SubPhaseId: "3.3"
    Confidence: 0.92
    SelectionOutcome: "ACCEPTED_DETERMINISTIC_GENERIC"

Factory evidence:
  DevelopmentLifecycleTopologySeeder.Seed() registers one UniversalNineNodeDevelopmentFlow
  per sub-phase without a prior specific flow. For 3.3 there is no specific api-contract
  flow yet. The factory accepts the generic flow as the deterministic placeholder and
  records the "generic_flow_pending_domain_learning" warning so the learning pipeline
  can schedule 3.3 domain learning in a future wave.

---

## Pair O1B1-WDA-factory-003 — probationary flow rejected, falls back to generic

```
source_id: xiigen-o1-factory-003
source_title: XIIGen O1 factory — probationary flow rejected for unit-tests 5.1
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — probationary_rejected outcome
transition_type: feature_addition
phase: 5
sub_phase: 5.1
task_type: adaptive_routing_flow_factory
node_role: flow-factory-selector
coverage_status: covered
pair_kind: adaptive_routing_flow_factory
```

A:
  sub_phase_id: "5.1"
  flow_registry_state:
    registered_flow_id: "unit-test-design-flow-5.1-candidate"
    flow_type: "UnitTestDesign51FlowAdapter"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 2 of 9 nodes covered"
    version: "0.3.0"
  fallback_flow_id: "universal-nine-node-development-flow-5.1"
  routing_confidence: 0.92
  evidence_predicates: ["TEST"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  FlowType: "UniversalNineNodeDevelopmentFlow"
  SelectionBasis: "fallback_after_probationary_rejection"
  TrustState: "deterministic_generic"
  GateBPassed: false
  FallbackUsed: true
  RejectedCandidateFlowId: "unit-test-design-flow-5.1-candidate"
  RejectedCandidateReason: "insufficient_evidence_pairs: 2 of 9 nodes covered"
  Warnings: ["probationary_flow_rejected:unit-test-design-flow-5.1-candidate", "fallback_used:universal-nine-node-development-flow-5.1"]
  [unit result envelope]
    UnitId: "unit-flow-factory-selector-v1"
    UnitRole: "flow-factory-selector"
    SubPhaseId: "5.1"
    Confidence: 0.92
    SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

Factory evidence:
  A candidate flow "unit-test-design-flow-5.1-candidate" was found in the registry
  but has not passed Gate B (only 2 of 9 nodes have sufficient evidence pairs).
  The factory rejects the probationary candidate and selects the registered generic
  nine-node flow as the safe fallback. Both the rejection and the fallback are
  recorded in Warnings so the learning pipeline can track candidate flow maturation.

---

## Pair O1B1-WDA-factory-004 — probationary flow accepted (Gate B passed)

```
source_id: xiigen-o1-factory-004
source_title: XIIGen O1 factory — probationary flow accepted after Gate B passed for 3.3
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — probationary_accepted outcome
transition_type: feature_addition
phase: 3
sub_phase: 3.3
task_type: adaptive_routing_flow_factory
node_role: flow-factory-selector
coverage_status: covered
pair_kind: adaptive_routing_flow_factory
```

A:
  sub_phase_id: "3.3"
  flow_registry_state:
    registered_flow_id: "api-contract-flow-3.3-candidate"
    flow_type: "ApiContract33FlowAdapter"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence_0.82; zero_failures_on_held_out"
    version: "0.9.0"
  routing_confidence: 0.92
  evidence_predicates: ["API_CONTRACT"]

B:
  SelectedFlowId: "api-contract-flow-3.3-candidate"
  FlowType: "ApiContract33FlowAdapter"
  SelectionBasis: "probationary_accepted"
  TrustState: "probationary"
  GateBPassed: true
  FallbackUsed: false
  Warnings: ["flow_trust_state_is_probationary:api-contract-flow-3.3-candidate", "promote_to_trusted_learned_when_production_evidence_sufficient"]
  [unit result envelope]
    UnitId: "unit-flow-factory-selector-v1"
    UnitRole: "flow-factory-selector"
    SubPhaseId: "3.3"
    Confidence: 0.92
    SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Factory evidence:
  "api-contract-flow-3.3-candidate" is probationary but has passed Gate B:
  all 9 nodes are covered, minimum node confidence is 0.82, and the held-out
  validation set shows zero failures. The factory accepts the probationary flow
  and records two warnings: one noting the trust state is still probationary,
  and one recommending promotion to trusted_learned once production evidence
  meets the promotion threshold. The factory does not promote automatically —
  promotion is a pipeline operator decision.
