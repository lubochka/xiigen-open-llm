# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 023 covers multi-domain sweep — four pairs summarizing transition patterns.

---

## B06-T1-023-001 — DOCUMENT sweep: all new domains → documentation, 1 transition

```
source_id: xiigen-b06-t1-023-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

Pattern: Any DOCUMENT-alone request from new domains (013-022)

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 1

Applicable to:
  ShippingRequirements, InventoryManagement, TaxCalculation, MultiCurrency,
  BundleAndDiscount, SubscriptionBilling, FraudDetection, DataPrivacy (without stakeholder),
  ReturnAndRefund, LoyaltyProgram (without stakeholder)

---

## B06-T1-023-002 — DOCUMENT+IMPLEMENT sweep: all new domains → feature_addition, 2 transitions

```
source_id: xiigen-b06-t1-023-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

Pattern: Any DOCUMENT+IMPLEMENT request from new domains (013-022)

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    - SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 2; Ordering: 2.1 → 4.1

---

## B06-T1-023-003 — ProbationaryRejected: probationary flow rejected, orchestration continues

```
source_id: xiigen-b06-t1-023-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}]
  FlowTrustState: "probationary_rejected"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
      FallbackUsed: true
      Warnings: ["probationary-rejected", "using-generic-fallback", "gate-b-retry-required"]
  TransitionCount: 1

Note: Orchestration does NOT halt when TrustState = probationary_rejected.
  The transition is produced with FallbackUsed: true. Production continues.
  The orchestrator is responsible for adding warnings from the factory.

---

## B06-T1-023-004 — Stakeholder dual route: DOCUMENT → 2.1+2.4 documentation

```
source_id: xiigen-b06-t1-023-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

Pattern: DOCUMENT+stakeholder from LoyaltyProgram or DataPrivacy domains

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
    - SubPhaseId: "2.4"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 2
  TransitionType: documentation (DOCUMENT only; no IMPLEMENT)
  Ordering: 2.1 before 2.4
