# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 020 covers SubscriptionBilling transition orchestration — four pairs.

---

## B06-T1-020-001 — SubscriptionBilling: DOCUMENT alone → documentation

```
source_id: xiigen-b06-t1-020-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}]

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 1

---

## B06-T1-020-002 — SubscriptionBilling: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-020-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "4.1"}]

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    - SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 2; Ordering: 2.1 → 4.1

---

## B06-T1-020-003 — SubscriptionBilling: FraudDetectionRequirements DOCUMENT+SECURITY

```
source_id: xiigen-b06-t1-020-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}]
  Context: "FraudDetectionRequirements — DOCUMENT alone (no SECURITY predicate)"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 1

Note: No SECURITY predicate exists in the predicate lexicon. Fraud detection
  and security requirements are handled as DOCUMENT requirements (use cases, business rules).
  There is no "SECURITY" predicate that routes to a separate sub-phase in 2.1.
  (The Layer 2 file 020 uses DOCUMENT+SECURITY as a scenario label — see that file for
  how the nine-node handles security context within the 2.1 flow.)

---

## B06-T1-020-004 — SubscriptionBilling: transition with SelectFlowId confirmed

```
source_id: xiigen-b06-t1-020-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

Complete transition output structure:

  SelectedLogicalTransition:
    TransitionId: "transition-subscription-020-001"
    SourceDocumentId: "B06-subscription-020-source"
    SubPhaseId: "2.1"
    TransitionType: "documentation"
    DomainId: "software-development"
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

  All fields must be populated. null SelectedFlowId is an error.
  null TransitionType is an error. null DomainId is an error.
