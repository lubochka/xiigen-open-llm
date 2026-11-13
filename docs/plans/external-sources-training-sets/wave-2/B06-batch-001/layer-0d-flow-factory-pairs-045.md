# Layer 0-D — Flow Factory Selection Pairs (045)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-045-001 — BackorderManagement: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-045-001
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

## B06-0D-045-002 — BackorderManagement: DOCUMENT + IMPLEMENT → 2 flows

```
source_id: xiigen-b06-0d-045-002
source_license_state: first_party
pair_kind: factory_selection
```

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]

Output:
  Flow_2_1: FlowAlias: "RequirementsAuthoringFlow21"; TrustState: "trusted_learned"
  Flow_4_1: FlowAlias: "ImplementationFlow41"; TrustState: "deterministic_generic"

Teaching: No "BackorderFlow" or "FulfillmentFlow". ImplementationFlow41 for all 4.1 routing.

---

## B06-0D-045-003 — BackorderManagement: DomainId invariant

```
source_id: xiigen-b06-0d-045-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for BackorderManagement.
  Anti-pattern: DomainId: "inventory", "supply-chain", "warehouse" — all incorrect.
