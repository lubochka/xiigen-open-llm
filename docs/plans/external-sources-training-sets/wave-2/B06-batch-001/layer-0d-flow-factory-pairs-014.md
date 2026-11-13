# Layer 0-D — Flow Factory Selection Pairs (014)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 014 covers InventoryManagement factory selection — three pairs.

---

## B06-0D-014-001 — InventoryManagement: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-014-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "InventoryManagement requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-014-002 — InventoryManagement: DOCUMENT+IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-014-002
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

## B06-0D-014-003 — InventoryManagement: probationary_rejected → universal fallback

```
source_id: xiigen-b06-0d-014-003
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "InventoryManagement — candidate flow registered but Gate B failed"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "probationary_rejected"
  FallbackUsed: true
  Warnings: ["candidate-flow-gate-b-failed", "using-universal-generic-fallback", "gate-b-retry-required"]

Factory note: probationary_rejected means a candidate flow was registered but
  failed Gate B. The factory falls back to universal generic. FallbackUsed: true.
  Three warnings are produced. Production continues — quality may be reduced.
