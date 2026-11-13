# Layer 0-D — Flow Factory Selection Pairs (050)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-050-001 — WarrantyTracking: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-050-001
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

---

## B06-0D-050-002 — WarrantyTracking: DOCUMENT + IMPLEMENT → 2 flows

```
source_id: xiigen-b06-0d-050-002
source_license_state: first_party
pair_kind: factory_selection
```

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]

Output:
  Flow_2_1: FlowAlias: "RequirementsAuthoringFlow21"; TrustState: "trusted_learned"
  Flow_4_1: FlowAlias: "ImplementationFlow41"; TrustState: "deterministic_generic"

---

## B06-0D-050-003 — WarrantyTracking: DomainId invariant

```
source_id: xiigen-b06-0d-050-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for WarrantyTracking.
  Anti-pattern: DomainId: "warranty", "after-sales", "support" — all incorrect.
