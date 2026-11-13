# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 015 covers LoyaltyProgram transition orchestration — four pairs, including
the 2.1+2.4 dual-transition case for stakeholder signals.

---

## B06-T1-015-001 — LoyaltyProgram: DOCUMENT → documentation single transition

```
source_id: xiigen-b06-t1-015-001
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

## B06-T1-015-002 — LoyaltyProgram: stakeholder → two transitions (2.1+2.4), documentation

```
source_id: xiigen-b06-t1-015-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "2.4"
  Context: "Loyalty stakeholder behavior expectations"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
    - SubPhaseId: "2.4"; TransitionType: "documentation"; DomainId: "software-development"
      SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TransitionCount: 2
  TransitionType: documentation (DOCUMENT only — no IMPLEMENT or UI)
  Ordering: 2.1 before 2.4

Teaching: DOCUMENT alone with stakeholder → 2 routing decisions → 2 transitions.
  TransitionType is still documentation (not feature_addition) because no IMPLEMENT is present.

---

## B06-T1-015-003 — LoyaltyProgram: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-015-003
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

## B06-T1-015-004 — LoyaltyProgram: 2.1+2.4+4.1 three-transition ordering

```
source_id: xiigen-b06-t1-015-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "2.4"
    - PredictedSubPhaseId: "4.1"
  Context: "Write loyalty stakeholder requirements"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    - SubPhaseId: "2.4"; TransitionType: "feature_addition"; DomainId: "software-development"
    - SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 3
  TransitionOrdering: 2.1 → 2.4 → 4.1 (canonical ordering: requirements → stakeholder → implementation)
