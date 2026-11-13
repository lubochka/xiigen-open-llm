# Layer 0-D — Flow Factory Selection Pairs (002)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 002 covers deterministic_generic: 2.1 has no learned flow yet.
RequirementsAuthoringFlow21 is selected as the deterministic generic fallback.

---

## B06-0D-002-001 — deterministic_generic: 2.1 no learned flow

```
source_id: xiigen-b06-0d-002-001
source_title: XIIGen B06 factory — deterministic_generic 2.1
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  routing_decision_source: "B06-0a-002-001 → [DOCUMENT] → 2.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

Factory selection rationale:
  No learned flow registered for sub-phase 2.1. Factory selects the
  universal deterministic generic flow. FallbackUsed is false because
  deterministic_generic selection is the primary path when no learned
  flow exists — it is not a fallback from a rejected candidate.

---

## B06-0D-002-002 — deterministic_generic: payment processing requirements

```
source_id: xiigen-b06-0d-002-002
source_title: XIIGen B06 factory — deterministic_generic payment requirements
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  routing_decision_source: "PaymentProcessingRequirements → [DOCUMENT] → 2.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B06-0D-002-003 — RequirementsAuthoringFlow21 is the Layer 2 flow for 2.1

```
source_id: xiigen-b06-0d-002-003
source_title: XIIGen B06 factory — RequirementsAuthoringFlow21 identity
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — RequirementsAuthoringFlow21:

  The universal-nine-node-development-flow-2.1 is the deterministic generic
  instantiation of RequirementsAuthoringFlow21.

  Flow identity:
    FlowId: "universal-nine-node-development-flow-2.1"
    Also referenced as: "RequirementsAuthoringFlow21"
    Sub-phase: 2.1 (Business Requirements and Use Cases)
    Node count: 9
    Node roles:
      N01: DomainSplitterUnit
      N02: DomainTranslatorUnit
      N03: SchemePlannerUnit
      N04: DomainProcessorUnit
      N05: ApplierUnit
      N06: ResultTranslatorUnit
      N07: ArbiterSelectorUnit
      N08: ArbiterUnit
      N09: FormatterUnit

  When a learned flow is promoted to trusted_learned state and registered
  for sub-phase 2.1, it replaces the deterministic generic flow in the
  factory selection output. Until then, universal-nine-node-development-flow-2.1
  is always selected.
