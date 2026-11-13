# Layer 0-D — Flow Factory Selection Pairs (026)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-026-001 — ReferralProgram: DOCUMENT + stakeholder → two flows (2.1 + 2.4)

```
source_id: xiigen-b06-0d-026-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "2.4"]
  Context: "ReferralProgram DOCUMENT + stakeholder"

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []
  Flow_2_4:
    SelectedFlowId: "universal-nine-node-development-flow-2.4"
    FlowAlias: "StakeholderRequirementsFlow24"
    TrustState: "deterministic_generic"
    FallbackUsed: false
    Warnings: []

---

## B06-0D-026-002 — ReferralProgram: DOCUMENT + IMPLEMENT → RequirementsFlow21 + ImplementationFlow41

```
source_id: xiigen-b06-0d-026-002
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
    FallbackUsed: false
  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "deterministic_generic"
    FallbackUsed: false

---

## B06-0D-026-003 — ReferralProgram: trust state reference for Round 2 flows

```
source_id: xiigen-b06-0d-026-003
source_license_state: first_party
pair_kind: factory_selection
```

Trust state assignment confirmed for Round 2 domains:
  RequirementsAuthoringFlow21: trusted_learned (B06 batch-001 trained)
  StakeholderRequirementsFlow24: deterministic_generic (no dedicated B06 training)
  ImplementationFlow41: deterministic_generic (no dedicated B06 training)
  ArchitectureFlow31: deterministic_generic (no dedicated B06 training)

  All Round 2 domains (GiftCardRedemption through DigitalProductDelivery) use
  RequirementsAuthoringFlow21 for 2.1 routing. Trust state: trusted_learned for all.
  The flow is domain-agnostic — domain context is carried in the SelectedLogicalTransition,
  not in the FlowId.
