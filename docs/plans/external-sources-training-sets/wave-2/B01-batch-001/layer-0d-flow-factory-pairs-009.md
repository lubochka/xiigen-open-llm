# Layer 0-D — Flow Factory Selection Pairs (009)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-009-001 — DOCUMENT alone factory: BusinessRequirementsFlow21 (routes to 2.1, not 1.1)

```
source_id: xiigen-b01-0d-009-001
source_title: XIIGen B01 factory-009 — DOCUMENT alone: BusinessRequirementsFlow21 only
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
pair_kind: factory_selection
```

A:
  predicate_context: ["DOCUMENT"]
  routing_decisions:
    - sub_phase_id: "2.1", evidence_predicates: ["DOCUMENT"]

  Sub-phase 2.1 flow registry:
    registered_flow_id: null
    trust_state: null
    fallback_flow_id: "BusinessRequirementsFlow21"

  NOTE: sub-phase 1.1 is NOT in routing_decisions because no UI predicate fired.

B:
  FlowSelections:
    - SubPhaseId: "2.1"
      SelectedFlowId: "BusinessRequirementsFlow21"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

  TotalFlowsSelected: 1

Factory logic:
  DOCUMENT alone → single routing decision (2.1) → single flow selection
  (BusinessRequirementsFlow21). ScreenInventoryFlow11 is NOT selected because
  no UI predicate fired and no 1.1 routing decision exists. TotalFlowsSelected: 1.
  The factory must not add a ScreenInventoryFlow11 selection speculatively.

---

## B01-0D-009-002 — No factory selection for knowledge-edge inputs

```
source_id: xiigen-b01-0d-009-002
source_title: XIIGen B01 factory-009 — knowledge-edge input: no flow selected
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  predicate_context: []
  routing_decisions:
    - sub_phase_id: null
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

B:
  FlowSelections: []
  TotalFlowsSelected: 0
  KnowledgeEdgeHandled: true
  KnowledgeEdgeFlowId: "knowledge-edge-missing-flow"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW"

Factory logic:
  When all routing decisions have KnowledgeEdge: true, the factory produces
  NO real flow selections. TotalFlowsSelected: 0. The knowledge-edge synthetic
  flow is invoked instead. This synthetic flow does not execute the nine-node
  chain — it produces the KNOWLEDGE_EDGE result envelope directly.

---

## B01-0D-009-003 — Single vs multi-flow selection summary

```
source_id: xiigen-b01-0d-009-003
source_title: XIIGen B01 factory-009 — single vs multi-flow selection patterns
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Factory selection count patterns in B01:

  TotalFlowsSelected == 0:
    Only when knowledge-edge (all predicates empty or all routing decisions KnowledgeEdge: true)

  TotalFlowsSelected == 1:
    Single predicate fired: [UI], [DOCUMENT], [IMPLEMENT], or [ARCHITECTURE] alone

  TotalFlowsSelected == 2:
    Two predicates fired: [UI+DOCUMENT], [UI+IMPLEMENT], [UI+ARCHITECTURE],
      [DOCUMENT+IMPLEMENT], [ARCHITECTURE+IMPLEMENT], [ARCHITECTURE+DOCUMENT]

  TotalFlowsSelected == 3:
    Three predicates fired: [UI+DOCUMENT+IMPLEMENT], [UI+ARCHITECTURE+IMPLEMENT],
      [UI+ARCHITECTURE+DOCUMENT], [ARCHITECTURE+DOCUMENT+IMPLEMENT]

  TotalFlowsSelected == 4:
    All four predicates fired: [UI+ARCHITECTURE+IMPLEMENT+DOCUMENT]

  The maximum is 4 (one flow per predicate type). There is no scenario in B01
  where more than 4 flows are selected for a single request.
