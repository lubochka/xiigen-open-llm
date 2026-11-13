# Held-Out and Corruption Pairs (021)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-042 — Multi-domain DOCUMENT sweep

```
source_id: xiigen-b06-held-out-042
pair_kind: held_out
```

A:
  run_id: "B06-ho-042"
  user_request: "Define the requirements and use cases for shipping, tax, and fraud detection in the checkout flow."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["define", "requirements", "use cases"]
  TransitionType: "documentation"
  Routing: ["2.1"]
  DecisionCount: 1
  Note: Multiple domain subjects (shipping, tax, fraud) do not add predicates or sub-phases.

---

## Held-Out Pair B06B1-WDA-HO-043 — "write requirements" across new domains

```
source_id: xiigen-b06-held-out-043
pair_kind: held_out
```

A:
  run_id: "B06-ho-043"
  user_request: "Write the requirements for multi-currency, subscription billing, and data privacy."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"
  DecisionCount: 2
  Routing: ["2.1", "4.1"]

Generalization test: "write" fires IMPLEMENT; "requirements" fires DOCUMENT.
Multiple domain subjects do not add routing decisions.

---

## Corruption Pair B06B1-WDA-COR-023 — v6-03: extra sub-phase added for domain

```
source_id: xiigen-b06-corruption-023
pair_kind: corruption
corruption_type: Corruption-v6-03-extra-subphase-for-domain
```

A:
  user_request: "Define the fraud detection requirements for checkout."

B-correct:
  Routing: [{PredictedSubPhaseId: "2.1"}]
  DecisionCount: 1

B-corrupt:
  Routing: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "2.5"}]
  DecisionCount: 2

Corruption: Non-existent sub-phase 2.5 ("security requirements") added for fraud domain.
Fix: Sub-phase 2.5 does not exist. Fraud detection is a 2.1 (business requirements) domain.
  The sub-phase map is fixed: 2.1, 2.2, 2.3, 2.4. No 2.5 exists.
