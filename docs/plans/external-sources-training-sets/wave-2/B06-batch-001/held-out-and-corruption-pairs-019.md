# Held-Out and Corruption Pairs (019)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-038 — TaxCalculation DOCUMENT+ARCHITECTURE → documentation

```
source_id: xiigen-b06-held-out-038
pair_kind: held_out
```

A:
  run_id: "B06-ho-038"
  user_request: "Document the tax calculation requirements and design the architecture for the tax service."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  TransitionType: "documentation"
  DecisionCount: 2; Routing: ["2.1", "3.1"]

Generalization test: DOCUMENT+ARCHITECTURE → documentation (ARCHITECTURE does not upgrade to feature_addition).

---

## Held-Out Pair B06B1-WDA-HO-039 — "develop the feature-brief" dual predicate

```
source_id: xiigen-b06-held-out-039
pair_kind: held_out
```

A:
  run_id: "B06-ho-039"
  user_request: "Develop the feature-brief for subscription checkout integration."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"
  DecisionCount: 2

Generalization test: "develop" fires IMPLEMENT; "feature-brief" fires DOCUMENT.

---

## Corruption Pair B06B1-WDA-COR-021 — v6-02: IMPLEMENT suppressed for "develop" + feature-brief

```
source_id: xiigen-b06-corruption-021
pair_kind: corruption
corruption_type: Corruption-v6-02-IMPLEMENT-suppressed
```

A:
  user_request: "Develop the feature-brief for subscription checkout."

B-correct:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT"]
  TransitionType: "documentation"

Corruption: "develop" suppressed because object is "feature-brief" (documentation-like).
Fix: "develop" is in IMPLEMENT lexicon. Object does not suppress IMPLEMENT firing.
  TransitionType must be feature_addition.
