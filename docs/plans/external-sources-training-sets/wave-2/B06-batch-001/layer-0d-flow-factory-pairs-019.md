# Layer 0-D — Flow Factory Selection Pairs (019)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 019 covers BundleAndDiscount factory selection — three pairs.

---

## B06-0D-019-001 — BundleAndDiscount: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-019-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "BundleAndDiscount requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-019-002 — BundleAndDiscount: DOCUMENT+IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-019-002
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

## B06-0D-019-003 — BundleAndDiscount: feature_addition flows run concurrently

```
source_id: xiigen-b06-0d-019-003
source_license_state: first_party
pair_kind: factory_selection
```

Factory concurrency teaching:

  When multiple flows are selected (e.g., RequirementsAuthoringFlow21 + ImplementationFlow41),
  they run concurrently — not sequentially. The factory does not sequence the flows.
  Sequencing is controlled by the TransitionOrchestratorUnit at Layer 1.

  Correct: Both flows are instantiated simultaneously; each has independent N01-N09 nodes.
  Incorrect: Waiting for RequirementsAuthoringFlow21 to complete before starting ImplementationFlow41.

  Exception: Canonical ordering (2.1 → 4.1) applies to RESULT delivery order,
    not flow execution. Both flows may execute in parallel.
