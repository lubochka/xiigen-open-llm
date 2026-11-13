# Layer 0-D — Flow Factory Selection Pairs (057)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-057-001 — StockAlertSystem: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-057-001
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

## B06-0D-057-002 — StockAlertSystem: DOCUMENT + IMPLEMENT → 2 flows

```
source_id: xiigen-b06-0d-057-002
source_license_state: first_party
pair_kind: factory_selection
```

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]

Output:
  Flow_2_1: FlowAlias: "RequirementsAuthoringFlow21"; TrustState: "trusted_learned"
  Flow_4_1: FlowAlias: "ImplementationFlow41"; TrustState: "deterministic_generic"

---

## B06-0D-057-003 — StockAlertSystem: DomainId invariant

```
source_id: xiigen-b06-0d-057-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for StockAlertSystem.
  Anti-pattern: DomainId: "inventory", "alerting", "supply-chain" — all incorrect.
