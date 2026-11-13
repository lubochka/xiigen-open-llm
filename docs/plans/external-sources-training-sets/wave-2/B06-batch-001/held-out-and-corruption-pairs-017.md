# Held-Out and Corruption Pairs (017)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-034 — FraudDetection "define requirements"

```
source_id: xiigen-b06-held-out-034
pair_kind: held_out
```

A:
  run_id: "B06-ho-034"
  user_request: "Define the fraud detection requirements for checkout."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  TransitionType: "documentation"
  DomainId: "software-development"
  Routing: ["2.1"]

Generalization test: "define" + "requirements" → DOCUMENT. DomainId software-development.

---

## Held-Out Pair B06B1-WDA-HO-035 — DataPrivacy "stakeholder requirements" → 2.1+2.4

```
source_id: xiigen-b06-held-out-035
pair_kind: held_out
```

A:
  run_id: "B06-ho-035"
  user_request: "Specify the stakeholder requirements for data privacy in checkout."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  Routing: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "2.4"}]
  DecisionCount: 2
  TransitionType: "documentation"
  DomainId: "software-development"

---

## Corruption Pair B06B1-WDA-COR-019 — v6-01: DOCUMENT missed for "document" + "behavior"

```
source_id: xiigen-b06-corruption-019
pair_kind: corruption
corruption_type: Corruption-v6-01-DOCUMENT-missed
```

A:
  user_request: "Document the fraud scoring behavior during checkout."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["document", "behavior"]

B-corrupt:
  CompiledPredicateIds: []

Corruption: v6-01 — "document" (lexicon term 1) and "behavior" (lexicon term 2) both missed.
Fix: "document" as a verb fires DOCUMENT (Rule 1). "behavior" fires DOCUMENT.
  Full-text scan required — both terms present in this request.
