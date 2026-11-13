# Layer 0-D — Flow Factory Selection Pairs (015)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 015 covers LoyaltyProgram factory selection — three pairs, including
the 2.1+2.4 dual-flow case when stakeholder signal is present.

---

## B06-0D-015-001 — LoyaltyProgram: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-015-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "LoyaltyProgram requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-015-002 — LoyaltyProgram: stakeholder signal → 2.1+2.4 dual flows

```
source_id: xiigen-b06-0d-015-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "2.4"; EvidencePredicates: ["DOCUMENT"]
    Context: "Loyalty stakeholder behavior requirements"

B:
  SelectedFlows:
    - FlowId: "universal-nine-node-development-flow-2.1"; Alias: "RequirementsAuthoringFlow21"
      TrustState: "trusted_learned"; FallbackUsed: false
    - FlowId: "universal-nine-node-development-flow-2.4"; Alias: "StakeholderRequirementsFlow24"
      TrustState: "deterministic_generic"; FallbackUsed: false
  FlowCount: 2

Factory note: Both flows selected independently. RequirementsAuthoringFlow21 runs
  for business requirements; StakeholderRequirementsFlow24 runs for stakeholder needs.
  One predicate (DOCUMENT) produces two routing decisions → two flows.

---

## B06-0D-015-003 — LoyaltyProgram: trusted_learned → probationary_accepted boundary

```
source_id: xiigen-b06-0d-015-003
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "LoyaltyProgram — newly registered candidate flow, Gate B passed, 18 pairs"

B:
  SelectedFlowId: "loyalty-requirements-flow-2.1-candidate"
  FlowAlias: "RequirementsAuthoringFlow21-LoyaltyCandidate"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  Warnings: ["candidate-flow-below-25-pairs", "monitor-for-regression", "gate-b-passed"]

Factory note: probationary_accepted: Gate B passed but fewer than 25 pairs.
  The candidate flow is used (FallbackUsed: false) but with 3 warnings.
  Promotion to trusted_learned requires >= 25 pairs AND Gate B pass.
