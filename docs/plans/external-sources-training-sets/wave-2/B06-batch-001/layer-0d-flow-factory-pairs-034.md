# Layer 0-D — Flow Factory Selection Pairs (034)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-034-001 — SeasonalPromotion: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-034-001
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

## B06-0D-034-002 — SeasonalPromotion: DOCUMENT + IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-034-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "deterministic_generic"

---

## B06-0D-034-003 — SeasonalPromotion: trust state summary for Round 2

```
source_id: xiigen-b06-0d-034-003
source_license_state: first_party
pair_kind: factory_selection
```

Trust state summary for all Round 2 flows:

  Flow                         | TrustState           | Rationale
  -----------------------------|----------------------|----------------------------------
  RequirementsAuthoringFlow21  | trusted_learned      | B06 batch-001 training covers 2.1
  StakeholderRequirementsFlow24| deterministic_generic| No dedicated Round 2 training
  ArchitectureFlow31           | deterministic_generic| No dedicated Round 2 training
  ImplementationFlow41         | deterministic_generic| No dedicated Round 2 training
  ScreenInventoryFlow11        | deterministic_generic| Not covered in B06

  Note: Only RequirementsAuthoringFlow21 is promoted to trusted_learned by B06.
  All other flows remain at deterministic_generic until their own batches are produced.
  This is unchanged from the original 013-024 batch extension.
