# Held-Out and Corruption Pairs (013)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-026 — ShippingRequirements DOCUMENT predicate

```
source_id: xiigen-b06-held-out-026
pair_kind: held_out
```

A:
  run_id: "B06-ho-026"
  user_request: "Describe the shipping use cases for domestic and international delivery."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  Routing: [{PredictedSubPhaseId: "2.1"}]
  TransitionType: "documentation"
  FlowAlias: "RequirementsAuthoringFlow21"

Generalization test: "describe" + "use cases" → DOCUMENT. No IMPLEMENT verb present.

---

## Held-Out Pair B06B1-WDA-HO-027 — InventoryManagement "write the spec" dual

```
source_id: xiigen-b06-held-out-027
pair_kind: held_out
```

A:
  run_id: "B06-ho-027"
  user_request: "Write the inventory reservation spec for the checkout flow."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Routing: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "4.1"}]
  TransitionType: "feature_addition"
  DecisionCount: 2

Generalization test: "write" + "spec" → ["DOCUMENT", "IMPLEMENT"], feature_addition.

---

## Corruption Pair B06B1-WDA-COR-015 — v6-01: DOCUMENT missed for shipping requirements

```
source_id: xiigen-b06-corruption-015
pair_kind: corruption
corruption_type: Corruption-v6-01-DOCUMENT-missed
```

A:
  user_request: "Define the shipping requirements for the checkout flow."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]

B-corrupt:
  CompiledPredicateIds: []

Corruption: v6-01 — DOCUMENT missed. "define" and "requirements" are both DOCUMENT terms.
Fix: Scan for all 17 DOCUMENT lexicon terms. "define" is Rule 6 (fires DOCUMENT).
