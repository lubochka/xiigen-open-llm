# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 022 covers DataPrivacy transition orchestration — four pairs.

---

## B06-T1-022-001 — DataPrivacy: DOCUMENT alone → documentation

```
source_id: xiigen-b06-t1-022-001
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

## B06-T1-022-002 — DataPrivacy: stakeholder → 2.1+2.4 documentation

```
source_id: xiigen-b06-t1-022-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "2.4"
  Context: "DataPrivacy stakeholder requirements — legal, security, product owner"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
    - SubPhaseId: "2.4"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 2; Ordering: 2.1 before 2.4
  TransitionType: documentation (DOCUMENT only — no IMPLEMENT)

---

## B06-T1-022-003 — DataPrivacy: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-022-003
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

## B06-T1-022-004 — DataPrivacy: StakeholderShipping 2.1+2.4 orchestration

```
source_id: xiigen-b06-t1-022-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "2.4"
  Context: "StakeholderShipping — stakeholder shipping requirements for carrier partners"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
    - SubPhaseId: "2.4"; TransitionType: "documentation"; DomainId: "software-development"
      SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TransitionCount: 2
  Note: "StakeholderShipping routes to both 2.1 and 2.4 — see Layer 2 file 022 for nine-node"
