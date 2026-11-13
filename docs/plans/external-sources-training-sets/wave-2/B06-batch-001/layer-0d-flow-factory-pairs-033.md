# Layer 0-D — Flow Factory Selection Pairs (033)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-033-001 — CrossSellUpsell: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-033-001
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

## B06-0D-033-002 — CrossSellUpsell: three-predicate → three flows

```
source_id: xiigen-b06-0d-033-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "3.1", SubPhaseId: "4.1"]

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
  Flow_3_1:
    SelectedFlowId: "universal-nine-node-development-flow-3.1"
    FlowAlias: "ArchitectureFlow31"
    TrustState: "deterministic_generic"
  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "deterministic_generic"

---

## B06-0D-033-003 — CrossSellUpsell: anti-pattern — no domain-specific flows

```
source_id: xiigen-b06-0d-033-003
source_license_state: first_party
pair_kind: factory_selection
```

Anti-pattern AP-F7: Creating CrossSellFlow21 or UpsellImplementationFlow41.
  Correct: All domains use the same domain-agnostic flows.
  Anti-pattern AP-F8: Using IMPLEMENT TrustState for RequirementsAuthoringFlow21.
    RequirementsAuthoringFlow21 always has TrustState: "trusted_learned" in B06.
    ImplementationFlow41 always has TrustState: "deterministic_generic" in B06.
  Anti-pattern AP-F9: Omitting ArchitectureFlow31 when ARCHITECTURE predicate is present.
    Correct: Every routing decision maps to exactly one flow. No routing decision may be skipped.
