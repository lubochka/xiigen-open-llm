# Held-Out and Corruption Pairs (014)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-028 — LoyaltyProgram stakeholder dual route

```
source_id: xiigen-b06-held-out-028
pair_kind: held_out
```

A:
  run_id: "B06-ho-028"
  user_request: "Describe the stakeholder requirements for loyalty program checkout integration."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  Routing: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "2.4"}]
  DecisionCount: 2
  TransitionType: "documentation"

Generalization test: "stakeholder" → DOCUMENT fires; 2.1+2.4 routing; documentation.

---

## Held-Out Pair B06B1-WDA-HO-029 — ReturnAndRefund "define epics" DOCUMENT

```
source_id: xiigen-b06-held-out-029
pair_kind: held_out
```

A:
  run_id: "B06-ho-029"
  user_request: "Define the epics and capability requirements for end-to-end refund processing."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms fired: ["define", "epics", "capability", "requirements"]
  Routing: [{PredictedSubPhaseId: "2.1"}]
  TransitionType: "documentation"

Generalization test: epics (Rule 15) + capability (Rule 16) both fire DOCUMENT.

---

## Corruption Pair B06B1-WDA-COR-016 — v6-03: stakeholder routing collapsed

```
source_id: xiigen-b06-corruption-016
pair_kind: corruption
corruption_type: Corruption-v6-03-routing-collapsed
```

A:
  user_request: "Describe the stakeholder requirements for loyalty program checkout."

B-correct:
  Routing: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "2.4"}]
  DecisionCount: 2

B-corrupt:
  Routing: [{PredictedSubPhaseId: "2.1"}]
  DecisionCount: 1

Corruption: v6-03 — "stakeholder" present but 2.4 routing decision dropped.
Fix: "stakeholder" always produces 2.4 routing decision in addition to 2.1.
  DecisionCount must be 2 when stakeholder is present.
