# Held-Out and Corruption Pairs (015)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-030 — TaxCalculation "create spec" dual

```
source_id: xiigen-b06-held-out-030
pair_kind: held_out
```

A:
  run_id: "B06-ho-030"
  user_request: "Create a specification for the tax calculation engine."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"
  DecisionCount: 2
  Routing: ["2.1", "4.1"]

Generalization test: "create" fires IMPLEMENT; "specification" fires DOCUMENT.

---

## Held-Out Pair B06B1-WDA-HO-031 — MultiCurrency DOCUMENT alone

```
source_id: xiigen-b06-held-out-031
pair_kind: held_out
```

A:
  run_id: "B06-ho-031"
  user_request: "Document the currency conversion behavior during checkout."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  TransitionType: "documentation"
  Routing: [{PredictedSubPhaseId: "2.1"}]

Generalization test: "document" + "behavior" → DOCUMENT alone → documentation.

---

## Corruption Pair B06B1-WDA-COR-017 — v6-02: IMPLEMENT suppressed for "create spec"

```
source_id: xiigen-b06-corruption-017
pair_kind: corruption
corruption_type: Corruption-v6-02-IMPLEMENT-suppressed
```

A:
  user_request: "Create a specification for the tax calculation engine."

B-correct:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT"]
  TransitionType: "documentation"

Corruption: v6-02 — "create" fires IMPLEMENT but is suppressed because object is "specification".
Fix: "create" is in IMPLEMENT lexicon. Compiler is lexicon-based — fires regardless of object.
  TransitionType must be feature_addition when IMPLEMENT is present.
