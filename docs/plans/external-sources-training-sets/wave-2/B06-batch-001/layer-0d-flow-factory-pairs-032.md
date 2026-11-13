# Layer 0-D — Flow Factory Selection Pairs (032)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-032-001 — ProductRecommendation: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-032-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1"]

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

---

## B06-0D-032-002 — ProductRecommendation: DOCUMENT + ARCHITECTURE → two flows

```
source_id: xiigen-b06-0d-032-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "3.1"]

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
  Flow_3_1:
    SelectedFlowId: "universal-nine-node-development-flow-3.1"
    FlowAlias: "ArchitectureFlow31"
    TrustState: "deterministic_generic"

---

## B06-0D-032-003 — ProductRecommendation: DOCUMENT + stakeholder → 2.1 + 2.4

```
source_id: xiigen-b06-0d-032-003
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "2.4"]
  Context: "ProductRecommendation — stakeholder term present"

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
  Flow_2_4:
    SelectedFlowId: "universal-nine-node-development-flow-2.4"
    FlowAlias: "StakeholderRequirementsFlow24"
    TrustState: "deterministic_generic"
  Teaching: "stakeholder" signal adds 2.4 routing decision → factory produces StakeholderRequirementsFlow24 in addition to RequirementsAuthoringFlow21.
