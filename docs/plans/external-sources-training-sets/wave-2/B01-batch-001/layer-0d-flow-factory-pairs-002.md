# Layer 0-D — Flow Factory Selection Pairs (002)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-002-001 — deterministic_generic: 1.1 (UI predicate → ScreenInventoryFlow11)

```
source_id: xiigen-b01-0d-002-001
source_title: XIIGen B01 factory-002 — deterministic_generic for sub-phase 1.1 (admin panel)
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "ScreenInventoryFlow11"
  request_context: "admin panel page inventory"
  predicate_context: ["UI"]

B:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

Factory logic:
  No learned flow is registered for sub-phase 1.1 in the admin panel context.
  The universal ScreenInventoryFlow11 is the deterministic fallback for all UI-predicate
  inputs to sub-phase 1.1. FallbackUsed: false because this is the primary selection
  — no candidate flow existed to fall back from.

---

## B01-0D-002-002 — deterministic_generic: 1.1 for checkout UI flow

```
source_id: xiigen-b01-0d-002-002
source_title: XIIGen B01 factory-002 — deterministic_generic for sub-phase 1.1 (checkout)
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "ScreenInventoryFlow11"
  request_context: "checkout interface UI flow"
  predicate_context: ["UI"]

B:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

Factory logic:
  ScreenInventoryFlow11 is used for all sub-phase 1.1 UI-predicate routing in B01,
  regardless of whether the request is about checkout, admin panel, onboarding, or
  other UX scenarios. The flow handles the nine-node chain for all screen mapping work.

---

## B01-0D-002-003 — ScreenInventoryFlow11 identity and scope

```
source_id: xiigen-b01-0d-002-003
source_title: XIIGen B01 factory-002 — ScreenInventoryFlow11 identity
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

ScreenInventoryFlow11 identity:

  FlowId: "ScreenInventoryFlow11"
  FlowName: "Screen Inventory Flow for Sub-Phase 1.1"
  Phase: 1
  SubPhase: 1.1
  NodeCount: 9
  TrustState: deterministic_generic (in B01 current state)

  Node chain:
    Node 01 — DomainSplitterUnit: receives SelectedLogicalTransition, produces DomainPacket[]
    Node 02 — DomainTranslatorUnit: processes one DomainPacket, produces TranslatedDomainStatement
    Node 03 — SchemePlannerUnit: all statements → WorkBranch[]
    Node 04 — DomainProcessorUnit: one WorkBranch → ProcessedBranchResult
    Node 05 — ApplierUnit: one ProcessedBranchResult → AppliedBranchResult
    Node 06 — ValidationUnit: all AppliedBranchResults → ValidationReport
    Node 07 — IntegrationUnit: ValidationReport → IntegratedFlowResult
    Node 08 — QualityUnit: IntegratedFlowResult → QualityMetrics
    Node 09 — FormatterUnit: QualityMetrics → FinalHumanResult (the screen inventory document)

  Gate B assertion for this flow:
    All 9 nodes must produce output with Confidence >= 0.70.
    Screen count >= expected_screens for the scenario.
    No dead-end screens (every screen must have ≥ 1 exit transition).
