# Layer 0-D — Flow Factory Selection Pairs (022)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 022 covers DataPrivacy factory selection — three pairs, including the
2.1+2.4 dual-flow scenario when stakeholder signal is present.

---

## B06-0D-022-001 — DataPrivacy: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-022-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "DataPrivacy requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-022-002 — DataPrivacy: stakeholder signal → 2.1+2.4 dual flows

```
source_id: xiigen-b06-0d-022-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "2.4"; EvidencePredicates: ["DOCUMENT"]
    Context: "DataPrivacy stakeholder requirements"

B:
  SelectedFlows:
    - FlowId: "universal-nine-node-development-flow-2.1"; Alias: "RequirementsAuthoringFlow21"
      TrustState: "trusted_learned"; FallbackUsed: false
    - FlowId: "universal-nine-node-development-flow-2.4"; Alias: "StakeholderRequirementsFlow24"
      TrustState: "deterministic_generic"; FallbackUsed: false
  FlowCount: 2

---

## B06-0D-022-003 — DataPrivacy: DOCUMENT+IMPLEMENT → 2.1+4.1

```
source_id: xiigen-b06-0d-022-003
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
