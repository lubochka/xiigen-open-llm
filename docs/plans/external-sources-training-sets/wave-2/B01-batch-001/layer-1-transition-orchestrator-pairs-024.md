# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Complete B01 Orchestration Rules

---

## B01-L1-024-001 — Complete TransitionType table

```
source_id: xiigen-b01-l1-024-transition-table
source_license_state: first_party
pair_kind: transition_orchestrator
```

| Predicates | TransitionType | Primary SubPhaseId |
|---|---|---|
| [UI] | feature_addition | 1.1 |
| [DOCUMENT] | documentation | 2.1 |
| [ARCHITECTURE] | documentation | 3.1 |
| [IMPLEMENT] | feature_addition | 4.1 |
| [UI, DOCUMENT] | feature_addition | 1.1 |
| [UI, IMPLEMENT] | feature_addition | 1.1 |
| [UI, ARCHITECTURE] | documentation | 1.1 |
| [DOCUMENT, IMPLEMENT] | feature_addition | 4.1 |
| [UI, DOCUMENT, IMPLEMENT] | feature_addition | 1.1 |
| [UI, ARCHITECTURE, DOCUMENT, IMPLEMENT] | feature_addition | 1.1 |

Sub-phase hierarchy for primary selection: UI > ARCHITECTURE > IMPLEMENT > DOCUMENT

---

## B01-L1-024-002 — Complete flow selection rules

```
source_id: xiigen-b01-l1-024-flow-rules
source_license_state: first_party
pair_kind: transition_orchestrator
```

Flow selection rules:
  1. One flow per predicate: each predicate owns exactly one sub-phase and one flow
  2. FlowsSelected array contains one entry per unique predicate
  3. flow_execution_order mirrors FlowsSelected (parallel by default in B01)
  4. ContextCarryForward: always true for B01 batch

EM-9 guard:
  Layer 2 Node 01 A side = SelectedLogicalTransition (never raw string)
  SelectedLogicalTransition must contain: TransitionId, TransitionType, SubPhaseId, PredicateIds, UserRequest

---

## B01-L1-024-003 — Orchestration summary pair

```
source_id: xiigen-b01-l1-024-summary
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Map all screens in the SaaS onboarding UX."
   CompiledPredicateIds: ["UI"]

B:
  TransitionId: "transition-B01-l1-024-003-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11"]
  PredicateIds: ["UI"]
  ContextCarryForward: true

---

## B01-L1-024-004 — Orchestration knowledge-edge path

```
source_id: xiigen-b01-l1-024-kedge
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Make the onboarding flow feel more welcoming."
   CompiledPredicateIds: []

B:
  TransitionId: null
  TransitionType: null
  SubPhaseId: null
  FlowsSelected: ["knowledge-edge-missing-flow"]
  PredicateIds: []
  KnowledgeEdgePath: true
  ContextCarryForward: false

Knowledge-edge orchestration:
  No SelectedLogicalTransition is produced when CompiledPredicateIds is empty.
  The orchestrator produces a synthetic knowledge-edge result instead.
  Layer 2 does not execute for knowledge-edge inputs.
