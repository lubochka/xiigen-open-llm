# Layer 0-D — Flow Factory Selection Pairs (023)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 023 covers DOCUMENT+IMPLEMENT factory selection sweep — three pairs
demonstrating that all DOCUMENT+IMPLEMENT combinations select RequirementsAuthoringFlow21
plus ImplementationFlow41 regardless of the specific domain.

---

## B06-0D-023-001 — DOCUMENT+IMPLEMENT sweep: all new domains → same flow pair

```
source_id: xiigen-b06-0d-023-001
source_license_state: first_party
pair_kind: factory_selection
```

Factory invariant for DOCUMENT+IMPLEMENT across new domains:

  Domain               | 2.1 Flow                    | 4.1 Flow
  ---------------------|-----------------------------|-----------------------
  ShippingRequirements | RequirementsAuthoringFlow21 | ImplementationFlow41
  InventoryManagement  | RequirementsAuthoringFlow21 | ImplementationFlow41
  LoyaltyProgram       | RequirementsAuthoringFlow21 | ImplementationFlow41
  ReturnAndRefund      | RequirementsAuthoringFlow21 | ImplementationFlow41
  TaxCalculation       | RequirementsAuthoringFlow21 | ImplementationFlow41
  MultiCurrency        | RequirementsAuthoringFlow21 | ImplementationFlow41
  BundleAndDiscount    | RequirementsAuthoringFlow21 | ImplementationFlow41
  SubscriptionBilling  | RequirementsAuthoringFlow21 | ImplementationFlow41
  FraudDetection       | RequirementsAuthoringFlow21 | ImplementationFlow41
  DataPrivacy          | RequirementsAuthoringFlow21 | ImplementationFlow41

  The factory does NOT create domain-specific flows. All DOCUMENT predicates
  in all domains use RequirementsAuthoringFlow21. Domain context is handled
  within the flow (N02 DomainTranslatorUnit), not by flow selection.

---

## B06-0D-023-002 — DOCUMENT+IMPLEMENT: FlowCount always 2

```
source_id: xiigen-b06-0d-023-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: (any DOCUMENT+IMPLEMENT request from files 013-022)
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]

B:
  SelectedFlows:
    - FlowId: "universal-nine-node-development-flow-2.1"; Alias: "RequirementsAuthoringFlow21"
    - FlowId: "universal-nine-node-development-flow-4.1"; Alias: "ImplementationFlow41"
  FlowCount: 2

Anti-pattern: FlowCount: 1 for DOCUMENT+IMPLEMENT requests.
  Correct: DOCUMENT → 2.1 → one flow; IMPLEMENT → 4.1 → one flow; Total: 2.

---

## B06-0D-023-003 — TransitionType: feature_addition applies to both flows

```
source_id: xiigen-b06-0d-023-003
source_license_state: first_party
pair_kind: factory_selection
```

TransitionType propagation rule for DOCUMENT+IMPLEMENT:

  Both flows receive TransitionType: feature_addition.
  The factory does NOT set TransitionType — it comes from Layer 1 (TransitionOrchestratorUnit).
  The factory only selects which flow runs; Layer 1 determines TransitionType.

  Correct pipeline:
    Layer 0-A: CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
    Layer 0-B: RoutingDecisions: [2.1, 4.1]
    Layer 0-D: SelectedFlows: [RequirementsAuthoringFlow21, ImplementationFlow41]
    Layer 1:   Transitions: [{SubPhaseId: "2.1", TransitionType: "feature_addition"},
                              {SubPhaseId: "4.1", TransitionType: "feature_addition"}]
    Layer 2:   Both nine-node flows receive TransitionType: feature_addition
