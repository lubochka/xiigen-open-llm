# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 014 covers InventoryManagement transition orchestration — four pairs.

---

## B06-T1-014-001 — InventoryManagement: DOCUMENT alone → documentation

```
source_id: xiigen-b06-t1-014-001
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

## B06-T1-014-002 — InventoryManagement: DOCUMENT+IMPLEMENT → feature_addition both

```
source_id: xiigen-b06-t1-014-002
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

## B06-T1-014-003 — InventoryManagement: documentation vs feature_addition rule

```
source_id: xiigen-b06-t1-014-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

TransitionType determination table for file 014 pairs:

  Predicates              | TransitionType
  ------------------------|------------------
  ["DOCUMENT"]            | documentation
  ["DOCUMENT","IMPLEMENT"]| feature_addition
  ["DOCUMENT","ARCHITECTURE"] | documentation
  ["DOCUMENT","IMPLEMENT","ARCHITECTURE"] | feature_addition

  Rule: feature_addition if and only if IMPLEMENT or UI is present.
  ARCHITECTURE alone does not upgrade documentation to feature_addition.

---

## B06-T1-014-004 — InventoryManagement: transition count equals routing decision count

```
source_id: xiigen-b06-t1-014-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

Transition count invariant:

  TransitionCount == RoutingDecisionCount (always)

  DOCUMENT alone: 1 routing decision → 1 transition
  DOCUMENT+IMPLEMENT: 2 routing decisions → 2 transitions
  DOCUMENT+stakeholder: 2 routing decisions (2.1+2.4) → 2 transitions
  DOCUMENT+IMPLEMENT+stakeholder: 3 routing decisions → 3 transitions

  Anti-pattern: TransitionCount != RoutingDecisionCount.
    If the orchestrator produces fewer transitions than routing decisions, a
    sub-phase is dropped. This is a v6-03 routing collapse corruption.
