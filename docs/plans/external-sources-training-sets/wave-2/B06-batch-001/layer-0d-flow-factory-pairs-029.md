# Layer 0-D — Flow Factory Selection Pairs (029)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-029-001 — WholesaleDiscount: ARCHITECTURE alone → ArchitectureFlow31

```
source_id: xiigen-b06-0d-029-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "3.1"]
  Context: "WholesaleDiscount ARCHITECTURE alone"

B:
  Flow_3_1:
    SelectedFlowId: "universal-nine-node-development-flow-3.1"
    FlowAlias: "ArchitectureFlow31"
    TrustState: "deterministic_generic"
    FallbackUsed: false
    Warnings: []
  Teaching: ARCHITECTURE alone → 3.1 → ArchitectureFlow31. No 2.1 without DOCUMENT.

---

## B06-0D-029-002 — WholesaleDiscount: DOCUMENT + IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-029-002
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

## B06-0D-029-003 — WholesaleDiscount: probationary_rejected fallback

```
source_id: xiigen-b06-0d-029-003
source_license_state: first_party
pair_kind: factory_selection
```

Probationary_rejected scenario for wholesale discount domain:

A:
  RoutingDecisions: [SubPhaseId: "2.1"]
  TrustStateOverride: "probationary_rejected"

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "probationary_rejected"
    FallbackUsed: true
    Warnings:
      - "Primary flow RequirementsAuthoringFlow21 is in probationary_rejected state"
      - "Using universal fallback flow for 2.1"
      - "Pipeline continues — output should be reviewed for quality"
  Teaching: probationary_rejected does NOT halt the pipeline. FallbackUsed: true.
    The same universal flow ID is used. The TrustState label changes but FlowId does not.
