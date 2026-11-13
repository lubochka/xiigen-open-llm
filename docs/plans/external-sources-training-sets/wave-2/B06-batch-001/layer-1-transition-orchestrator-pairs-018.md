# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 018 covers MultiCurrency transition orchestration — four pairs.

---

## B06-T1-018-001 — MultiCurrency: DOCUMENT alone → documentation

```
source_id: xiigen-b06-t1-018-001
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

## B06-T1-018-002 — MultiCurrency: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-018-002
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

## B06-T1-018-003 — MultiCurrency: DOCUMENT+ARCHITECTURE → documentation

```
source_id: xiigen-b06-t1-018-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "3.1"}]
  Context: "TaxRequirementsDoc — document and design the architecture for multi-currency"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
    - SubPhaseId: "3.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionType: documentation
  Note: DOCUMENT+ARCHITECTURE → documentation (not feature_addition).
    ARCHITECTURE describes the design, not the build. Only IMPLEMENT or UI → feature_addition.

---

## B06-T1-018-004 — MultiCurrency: TransitionType propagation to both transitions

```
source_id: xiigen-b06-t1-018-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

TransitionType propagation rule:

  When IMPLEMENT is in the predicate set, feature_addition applies to ALL transitions
  in the set — not just the 4.1 transition.

  Correct:
    DOCUMENT+IMPLEMENT → both 2.1 and 4.1 transitions are feature_addition.

  Incorrect (partial propagation):
    2.1 transition: documentation
    4.1 transition: feature_addition
    This is a v6-03 corruption — TransitionType must be consistent across all transitions
    when IMPLEMENT is present.
