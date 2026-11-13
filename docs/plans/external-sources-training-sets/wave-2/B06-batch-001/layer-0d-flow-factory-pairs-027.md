# Layer 0-D — Flow Factory Selection Pairs (027)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-027-001 — FlashSaleEngine: three-predicate → three flows

```
source_id: xiigen-b06-0d-027-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "3.1", SubPhaseId: "4.1"]
  Context: "FlashSaleEngine DOCUMENT + ARCHITECTURE + IMPLEMENT"

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []
  Flow_3_1:
    SelectedFlowId: "universal-nine-node-development-flow-3.1"
    FlowAlias: "ArchitectureFlow31"
    TrustState: "deterministic_generic"
    FallbackUsed: false
    Warnings: []
  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "deterministic_generic"
    FallbackUsed: false
    Warnings: []

---

## B06-0D-027-002 — FlashSaleEngine: DOCUMENT alone → single flow

```
source_id: xiigen-b06-0d-027-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1"]
  Context: "FlashSaleEngine DOCUMENT alone"

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []
  Teaching: One routing decision → one flow selected. Factory produces exactly as many flows as routing decisions.

---

## B06-0D-027-003 — FlashSaleEngine: factory never produces null

```
source_id: xiigen-b06-0d-027-003
source_license_state: first_party
pair_kind: factory_selection
```

Anti-pattern AP-F3 (reminder): Setting SelectedFlowId to null for any routing decision.
  Correct: ILogicalFlowFactory always produces a valid SelectedFlowId.
    Even for deterministic_generic trust state — the factory uses the universal fallback flow ID.
    Universal fallback: "universal-nine-node-development-flow-{subphase}"
    Example for 3.1: "universal-nine-node-development-flow-3.1" (not null)

  Additional anti-pattern AP-F6: Producing zero flows when routing decisions exist.
    Correct: Number of flows = Number of routing decisions (1:1 mapping).
    Exception: knowledge-edge → DecisionCount: 0 → zero flows (correct behavior).
