# Layer 0-D — Flow Factory Selection Pairs (020)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 020 covers SubscriptionBilling factory selection — three pairs.

---

## B06-0D-020-001 — SubscriptionBilling: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-020-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "SubscriptionBilling requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-020-002 — SubscriptionBilling: DOCUMENT+IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-020-002
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

## B06-0D-020-003 — SubscriptionBilling: trust state escalation path

```
source_id: xiigen-b06-0d-020-003
source_license_state: first_party
pair_kind: factory_selection
```

Trust state escalation for SubscriptionBillingFlow21 (hypothetical future flow):

  Step 1: No registered flow → TrustState: deterministic_generic
    Output: universal-nine-node-development-flow-2.1; FallbackUsed: false

  Step 2: Candidate registered, Gate B failed → TrustState: probationary_rejected
    Output: universal-nine-node-development-flow-2.1; FallbackUsed: true; Warnings: [3]

  Step 3: Candidate registered, Gate B passed, 20 pairs → TrustState: probationary_accepted
    Output: subscription-billing-flow-2.1-candidate; FallbackUsed: false; Warnings: [3]

  Step 4: Gate B passed, >= 25 pairs → TrustState: trusted_learned
    Output: subscription-billing-flow-2.1; FallbackUsed: false; Warnings: []

  Key point: TrustState transitions are unidirectional through the escalation path
    (except rejected which can be re-evaluated). trusted_learned is the final state.
