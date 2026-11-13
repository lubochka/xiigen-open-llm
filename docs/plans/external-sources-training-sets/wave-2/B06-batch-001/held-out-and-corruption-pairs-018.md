# Held-Out and Corruption Pairs (018)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-036 — "plan the sprint for requirements" partial predicate

```
source_id: xiigen-b06-held-out-036
pair_kind: held_out
```

A:
  run_id: "B06-ho-036"
  user_request: "Plan the sprint for privacy consent requirements implementation."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "requirements" fires DOCUMENT; "plan" does not fire any predicate.
  KnowledgeEdge: false
  Routing: [{PredictedSubPhaseId: "2.1"}]

Generalization test: Partial-predicate — "requirements" fires DOCUMENT even in sprint-planning sentence.

---

## Held-Out Pair B06B1-WDA-HO-037 — "manage inventory levels" → knowledge-edge

```
source_id: xiigen-b06-held-out-037
pair_kind: held_out
```

A:
  run_id: "B06-ho-037"
  user_request: "Manage inventory levels across checkout sessions."

Expected B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Routing: []
  DecisionCount: 0

Generalization test: "manage" and "levels" are not predicate terms → knowledge-edge.

---

## Corruption Pair B06B1-WDA-COR-020 — v6-03: DomainId wrong for fraud domain

```
source_id: xiigen-b06-corruption-020
pair_kind: corruption
corruption_type: Corruption-v6-03-DomainId-wrong
```

A:
  user_request: "Define the fraud detection requirements for checkout."

B-correct:
  DomainId: "software-development"

B-corrupt:
  DomainId: "security"

Corruption: DomainId set to business domain instead of work domain.
Fix: Rule 17 — DomainId is always "software-development" for B06 regardless of subject matter.
  Fraud detection, privacy, compliance are subject matter — not the DomainId.
