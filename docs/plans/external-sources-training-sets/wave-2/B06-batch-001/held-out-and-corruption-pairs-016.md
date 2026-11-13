# Held-Out and Corruption Pairs (016)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-032 — BundleAndDiscount "describe use cases"

```
source_id: xiigen-b06-held-out-032
pair_kind: held_out
```

A:
  run_id: "B06-ho-032"
  user_request: "Describe the use cases and narrative for discount application."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["describe", "use cases", "narrative"]
  TransitionType: "documentation"
  Routing: ["2.1"]

---

## Held-Out Pair B06B1-WDA-HO-033 — SubscriptionBilling "write user stories" dual

```
source_id: xiigen-b06-held-out-033
pair_kind: held_out
```

A:
  run_id: "B06-ho-033"
  user_request: "Write the user stories for subscription checkout."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"
  DecisionCount: 2

Generalization test: "write" fires IMPLEMENT; "stories" fires DOCUMENT.

---

## Corruption Pair B06B1-WDA-COR-018 — v6-03: TransitionType wrong for DOCUMENT+IMPLEMENT

```
source_id: xiigen-b06-corruption-018
pair_kind: corruption
corruption_type: Corruption-v6-03-TransitionType-wrong
```

A:
  user_request: "Write the bundle discount specification."

B-correct:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition" (on ALL transitions)

B-corrupt:
  Transition_2.1: TransitionType: "documentation"
  Transition_4.1: TransitionType: "feature_addition"

Corruption: v6-03 — TransitionType partially propagated. 2.1 transition set to documentation
  even though IMPLEMENT is present. All transitions must be feature_addition when IMPLEMENT fires.
Fix: TransitionType from IMPLEMENT propagates to ALL transitions in the set.
