# Layer 0-D — Flow Factory Selection Pairs (037)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-037-001 — CustomerSegmentation: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-037-001
source_license_state: first_party
pair_kind: factory_selection
```

Input:
  RoutingDecisions: [SubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]]

Output:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"
  DomainId: "software-development"
  FallbackUsed: false
  Warnings: []

Teaching: CustomerSegmentation domain maps to RequirementsAuthoringFlow21 (same as all DOCUMENT → 2.1 cases).

---

## B06-0D-037-002 — CustomerSegmentation: DOCUMENT + IMPLEMENT → 2 flows

```
source_id: xiigen-b06-0d-037-002
source_license_state: first_party
pair_kind: factory_selection
```

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]

Output:
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

Teaching: 2 routing decisions → 2 flows. Number of flows = Number of routing decisions (1:1).

---

## B06-0D-037-003 — CustomerSegmentation: DomainId invariant

```
source_id: xiigen-b06-0d-037-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for CustomerSegmentation.
  "CustomerSegmentation" is the business domain.
  "software-development" is the work domain (what we are doing: software development).
  ILogicalFlowFactory always produces DomainId: "software-development" regardless of business domain.
  Anti-pattern: DomainId: "segmentation", "crm", "analytics" — all incorrect.
