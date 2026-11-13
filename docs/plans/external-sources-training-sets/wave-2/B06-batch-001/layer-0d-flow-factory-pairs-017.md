# Layer 0-D — Flow Factory Selection Pairs (017)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 017 covers TaxCalculation factory selection — three pairs.

---

## B06-0D-017-001 — TaxCalculation: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-017-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "TaxCalculation requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-017-002 — TaxCalculation: DOCUMENT+IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-017-002
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

## B06-0D-017-003 — TaxCalculation: factory always produces valid flow

```
source_id: xiigen-b06-0d-017-003
source_license_state: first_party
pair_kind: factory_selection
```

Factory invariant demonstration:

  Scenario A: TrustState = trusted_learned
    Input:  PredictedSubPhaseId: "2.1"
    Output: SelectedFlowId: "universal-nine-node-development-flow-2.1"
            FallbackUsed: false

  Scenario B: TrustState = deterministic_generic (no learned flow)
    Input:  PredictedSubPhaseId: "2.1"
    Output: SelectedFlowId: "universal-nine-node-development-flow-2.1"
            FallbackUsed: false

  Scenario C: TrustState = probationary_rejected
    Input:  PredictedSubPhaseId: "2.1"
    Output: SelectedFlowId: "universal-nine-node-development-flow-2.1"
            FallbackUsed: true
            Warnings: [3 warnings]

  Invariant: SelectedFlowId is NEVER null. Pipeline never halts due to missing flow.
  The universal generic flow is always available as a fallback for any sub-phase.
