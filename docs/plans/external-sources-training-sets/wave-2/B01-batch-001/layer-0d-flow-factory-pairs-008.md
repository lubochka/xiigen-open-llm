# Layer 0-D — Flow Factory Selection Pairs (008)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-008-001 — UI + DOCUMENT + IMPLEMENT factory: three flows

```
source_id: xiigen-b01-0d-008-001
source_title: XIIGen B01 factory-008 — three-flow factory selection for UI+DOCUMENT+IMPLEMENT
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1 + 2.1 + 4.1"
pair_kind: factory_selection
```

A:
  predicate_context: ["UI", "DOCUMENT", "IMPLEMENT"]
  routing_decisions:
    - sub_phase_id: "1.1", evidence_predicates: ["UI"]
    - sub_phase_id: "2.1", evidence_predicates: ["DOCUMENT"]
    - sub_phase_id: "4.1", evidence_predicates: ["IMPLEMENT"]

  Sub-phase 1.1 flow registry: registered_flow_id: null; fallback: "ScreenInventoryFlow11"
  Sub-phase 2.1 flow registry: registered_flow_id: null; fallback: "BusinessRequirementsFlow21"
  Sub-phase 4.1 flow registry: registered_flow_id: null; fallback: "ImplementationFlow41"

B:
  FlowSelections:
    - SubPhaseId: "1.1"
      SelectedFlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    - SubPhaseId: "2.1"
      SelectedFlowId: "BusinessRequirementsFlow21"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    - SubPhaseId: "4.1"
      SelectedFlowId: "ImplementationFlow41"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

  TotalFlowsSelected: 3

Factory logic:
  Three routing decisions → three independent factory selections. Each sub-phase
  has its own flow registry evaluation. All three are deterministic_generic in
  this batch. TotalFlowsSelected: 3. Execution order: 1.1 → 2.1 → 4.1.

---

## B01-0D-008-002 — Mixed trust states across three flows

```
source_id: xiigen-b01-0d-008-002
source_title: XIIGen B01 factory-008 — three flows with mixed trust states
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  predicate_context: ["UI", "DOCUMENT", "IMPLEMENT"]

  Sub-phase 1.1: registered_flow_id: "ScreenInventoryFlow11-v4"; trust_state: "trusted_learned"; gate_b_passed: true
  Sub-phase 2.1: registered_flow_id: null; trust_state: null
  Sub-phase 4.1: registered_flow_id: "ImplementationFlow41-candidate"; trust_state: "probationary"; gate_b_passed: false

B:
  FlowSelections:
    - SubPhaseId: "1.1"
      SelectedFlowId: "ScreenInventoryFlow11-v4"
      TrustState: "trusted_learned"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

    - SubPhaseId: "2.1"
      SelectedFlowId: "BusinessRequirementsFlow21"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    - SubPhaseId: "4.1"
      SelectedFlowId: "ImplementationFlow41"
      TrustState: "deterministic_generic"
      FallbackUsed: true
      RejectedCandidateFlowId: "ImplementationFlow41-candidate"
      Warnings:
        - "probationary_flow_rejected:ImplementationFlow41-candidate"
        - "gate_b_failed:candidate_submitted_not_evaluated"
        - "fallback_used:ImplementationFlow41"
      SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

  TotalFlowsSelected: 3

Factory logic:
  Each sub-phase is evaluated independently; trust states differ. 1.1 uses trusted_learned.
  2.1 has no candidate; uses deterministic_generic. 4.1 has a candidate but it has not
  passed Gate B; falls back to generic. Mixed trust states are normal in production.

---

## B01-0D-008-003 — TotalFlowsSelected and execution completeness check

```
source_id: xiigen-b01-0d-008-003
source_title: XIIGen B01 factory-008 — TotalFlowsSelected gate
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Gate B for factory selection completeness:

  Rule: TotalFlowsSelected must equal the number of distinct sub_phase_id entries
  in the routing decisions array.

  Example:
    RoutingDecisions has entries for: ["1.1", "2.1", "4.1"] (3 entries)
    FlowSelections must have: 3 entries
    TotalFlowsSelected must be: 3

  If TotalFlowsSelected < expected count:
    → Missing flow selection — factory failed to process a sub-phase
    → This is a factory corruption; must be detected and corrected

  If TotalFlowsSelected > expected count:
    → Phantom flow selected — factory added a sub-phase not in routing decisions
    → Also a corruption; should not occur

  The model must always verify: count(FlowSelections) == count(RoutingDecisions)
