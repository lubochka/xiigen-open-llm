# Layer 0-D — Flow Factory Selection Pairs (013)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 013 covers ShippingRequirements factory selection — three pairs showing
ILogicalFlowFactory selecting RequirementsAuthoringFlow21 for DOCUMENT decisions.

---

## B06-0D-013-001 — ShippingRequirements: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-013-001
source_license_state: first_party
contract_signature: ILogicalFlowFactory<RoutingDecision> selects ILogicalFlow
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"
    EvidencePredicates: ["DOCUMENT"]
    Context: "ShippingRequirements — define shipping requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []

Factory rationale: Sub-phase 2.1 → RequirementsAuthoringFlow21. Trust state is
  trusted_learned after B06 batch-001 promotion.

---

## B06-0D-013-002 — ShippingRequirements: DOCUMENT+IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-013-002
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

## B06-0D-013-003 — ShippingRequirements: deterministic_generic fallback scenario

```
source_id: xiigen-b06-0d-013-003
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"
    EvidencePredicates: ["DOCUMENT"]
    Context: "New checkout instance — no registered learned flow yet"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []

Factory note: deterministic_generic is used when no learned flow is registered.
  The factory always produces a valid SelectedFlowId — never null, never halts.
