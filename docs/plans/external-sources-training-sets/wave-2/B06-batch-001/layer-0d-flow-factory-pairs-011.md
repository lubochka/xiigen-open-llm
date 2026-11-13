# Layer 0-D — Flow Factory Selection Pairs (011)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 011 covers the documentation TransitionType in B06: updating existing requirements.

---

## B06-0D-011-001 — documentation TransitionType: updating existing requirements

```
source_id: xiigen-b06-0d-011-001
source_title: XIIGen B06 factory — documentation TransitionType update existing
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  routing_decision_source:
    user_request: "Update the acceptance criteria for the payment use case — the retry limit has changed from 3 to 5."
    CompiledPredicateIds: ["DOCUMENT"]
    PredictedSubPhaseId: "2.1"
  transition_type: "documentation"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
  TransitionType: "documentation"
  Warnings: []

Factory selection note:
  TransitionType is "documentation" because the user is updating existing
  acceptance criteria (existing artifact), not creating new requirements.
  The flow selection is identical to feature_addition — RequirementsAuthoringFlow21
  is used regardless of TransitionType. TransitionType affects orchestration
  context (e.g., the flow knows to update an existing artifact rather than
  create a new one), not the flow identity.

---

## B06-0D-011-002 — TransitionType does NOT change flow selection

```
source_id: xiigen-b06-0d-011-002
source_title: XIIGen B06 factory — TransitionType does not change flow selection
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — TransitionType and flow selection:

  TransitionType (feature_addition vs documentation) does NOT change
  which flow is selected by the factory.

  Sub-phase 2.1 always selects RequirementsAuthoringFlow21 (or its learned
  equivalent), regardless of TransitionType.

  TransitionType is passed to the selected flow as context. The flow's
  N01 DomainSplitterUnit and N05 ApplierUnit use TransitionType to determine
  whether to create a new requirements document or update an existing one.

  Factory selection is purely sub-phase-based + trust-state-based.
  TransitionType is a downstream input to flow execution, not a factory input.

---

## B06-0D-011-003 — B06 TransitionType B12 cross-reference

```
source_id: xiigen-b06-0d-011-003
source_title: XIIGen B06 factory — TransitionType B06 vs B12 comparison
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — B06 TransitionType in B06 context:

  B06 Core Rule 12: TransitionType: feature_addition (new requirements artifact);
    documentation (updating existing).

  In B06 training pairs, the dominant TransitionType is:
    - feature_addition: when new requirements artifacts are being created
      and/or IMPLEMENT/UI predicates fire
    - documentation: when existing requirements are being described or updated
      and only DOCUMENT/ARCHITECTURE predicates fire

  B06 TransitionType distribution:
    feature_addition: pairs where "write", "create", or UI terms fire
    documentation: pairs where "describe", "define", "specify", "document" fire
      without IMPLEMENT/UI co-firing

  Both TransitionTypes use RequirementsAuthoringFlow21. The factory output
  always includes the correct TransitionType from the router/transition-orchestrator.
