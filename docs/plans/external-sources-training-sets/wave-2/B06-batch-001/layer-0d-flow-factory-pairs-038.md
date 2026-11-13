# Layer 0-D — Flow Factory Selection Pairs (038)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-038-001 — DynamicPricing: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-038-001
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

## B06-0D-038-002 — DynamicPricing: DOCUMENT + ARCHITECTURE + IMPLEMENT → 3 flows

```
source_id: xiigen-b06-0d-038-002
source_license_state: first_party
pair_kind: factory_selection
```

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "3.1", SubPhaseId: "4.1"]

Output:
  Flow_2_1: FlowAlias: "RequirementsAuthoringFlow21"; TrustState: "trusted_learned"
  Flow_3_1: FlowAlias: "ArchitectureFlow31"; TrustState: "trusted_learned"
  Flow_4_1: FlowAlias: "ImplementationFlow41"; TrustState: "deterministic_generic"

Teaching: 3 routing decisions → 3 flows. ILogicalFlowFactory never produces zero flows when decisions exist.

---

## B06-0D-038-003 — DynamicPricing: DomainId invariant

```
source_id: xiigen-b06-0d-038-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for DynamicPricing.
  Anti-pattern: DomainId: "pricing", "commerce", "revenue" — all incorrect.
  Rule 17 applies unconditionally regardless of how complex or domain-specific the pricing logic is.
