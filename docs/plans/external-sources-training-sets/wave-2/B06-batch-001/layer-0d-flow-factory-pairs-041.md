# Layer 0-D — Flow Factory Selection Pairs (041)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-041-001 — MembershipTiers: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-041-001
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

## B06-0D-041-002 — MembershipTiers: DOCUMENT + IMPLEMENT → 2 flows

```
source_id: xiigen-b06-0d-041-002
source_license_state: first_party
pair_kind: factory_selection
```

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]

Output:
  Flow_2_1: FlowAlias: "RequirementsAuthoringFlow21"; TrustState: "trusted_learned"
  Flow_4_1: FlowAlias: "ImplementationFlow41"; TrustState: "deterministic_generic"

Teaching: No "MembershipTierFlow" or "LoyaltyFlow" for 4.1. ImplementationFlow41 handles all implementation.

---

## B06-0D-041-003 — MembershipTiers: DomainId invariant

```
source_id: xiigen-b06-0d-041-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for MembershipTiers.
  Anti-pattern: DomainId: "loyalty", "membership", "rewards" — all incorrect.
  The tier structure (Bronze/Silver/Gold/Platinum) is business domain vocabulary, not work domain.
