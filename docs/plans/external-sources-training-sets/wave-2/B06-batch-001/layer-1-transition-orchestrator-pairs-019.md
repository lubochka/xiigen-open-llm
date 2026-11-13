# Layer 1 — Transition Orchestrator Pairs (019)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 019 covers BundleAndDiscount transition orchestration — four pairs.

---

## B06-T1-019-001 — BundleAndDiscount: DOCUMENT alone → documentation

```
source_id: xiigen-b06-t1-019-001
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

## B06-T1-019-002 — BundleAndDiscount: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-019-002
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

## B06-T1-019-003 — BundleAndDiscount: SubscriptionBillingSpec with DOCUMENT+IMPLEMENT

```
source_id: xiigen-b06-t1-019-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "4.1"}]
  Context: "SubscriptionBillingSpec — write subscription spec"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
      Note: "SubscriptionBilling requirements phase"
    - SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
      SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TransitionCount: 2
  ScenarioNote: "Subscription billing spec with IMPLEMENT → feature_addition (see Layer 2 file 019)"

---

## B06-T1-019-004 — BundleAndDiscount: feature_addition vs documentation decision tree

```
source_id: xiigen-b06-t1-019-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

TransitionType decision tree for all new domain files:

  If IMPLEMENT in predicates → feature_addition (unconditional)
  If UI in predicates → feature_addition (unconditional)
  If only DOCUMENT → documentation
  If only ARCHITECTURE → documentation
  If only DOCUMENT+ARCHITECTURE → documentation

  New domain examples:
    "define bundle requirements" → DOCUMENT → documentation
    "write bundle spec" → DOCUMENT+IMPLEMENT → feature_addition
    "document discount behavior" → DOCUMENT → documentation
    "develop discount feature-brief" → DOCUMENT+IMPLEMENT → feature_addition
