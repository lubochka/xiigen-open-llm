# Layer 0-D — Flow Factory Selection Pairs (016)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 016 covers ReturnAndRefund factory selection — three pairs.

---

## B06-0D-016-001 — ReturnAndRefund: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-016-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "ReturnAndRefund requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-016-002 — ReturnAndRefund: DOCUMENT+IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-016-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]

B:
  SelectedFlows:
    - FlowId: "universal-nine-node-development-flow-2.1"; Alias: "RequirementsAuthoringFlow21"
      TrustState: "trusted_learned"; FallbackUsed: false
    - FlowId: "universal-nine-node-development-flow-4.1"; Alias: "ImplementationFlow41"
      TrustState: "deterministic_generic"; FallbackUsed: false
  FlowCount: 2

---

## B06-0D-016-003 — ReturnAndRefund: flow map for new domains

```
source_id: xiigen-b06-0d-016-003
source_license_state: first_party
pair_kind: factory_selection
```

New domain flow map for files 013-024:

  Domain                | Sub-phase | Flow
  ----------------------|-----------|---------------------------------------------
  ShippingRequirements  | 2.1       | RequirementsAuthoringFlow21
  InventoryManagement   | 2.1       | RequirementsAuthoringFlow21
  LoyaltyProgram        | 2.1+2.4*  | RequirementsAuthoringFlow21 + StakeholderRequirementsFlow24
  ReturnAndRefund       | 2.1       | RequirementsAuthoringFlow21
  TaxCalculation        | 2.1       | RequirementsAuthoringFlow21
  MultiCurrency         | 2.1       | RequirementsAuthoringFlow21
  BundleAndDiscount     | 2.1       | RequirementsAuthoringFlow21
  SubscriptionBilling   | 2.1       | RequirementsAuthoringFlow21
  FraudDetection        | 2.1       | RequirementsAuthoringFlow21
  DataPrivacy           | 2.1+2.4*  | RequirementsAuthoringFlow21 + StakeholderRequirementsFlow24

  * 2.4 added only when "stakeholder" term present.

  All DOCUMENT requests across all domains route to RequirementsAuthoringFlow21.
  Domain does not change the flow selection — sub-phase determines the flow.
