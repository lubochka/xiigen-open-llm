# Held-Out and Corruption Pairs (023)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-046 — probationary_rejected flow: FallbackUsed: true

```
source_id: xiigen-b06-held-out-046
pair_kind: held_out
```

A:
  run_id: "B06-ho-046"
  user_request: "Define the checkout flow requirements."
  FlowTrustState: "probationary_rejected"

Expected B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FallbackUsed: true
  Warnings: ["candidate-flow-gate-b-failed", "using-universal-generic-fallback", "gate-b-retry-required"]
  TransitionType: "documentation"

Generalization test: probationary_rejected → FallbackUsed: true; 3 warnings; production continues.

---

## Held-Out Pair B06B1-WDA-HO-047 — "define" fires DOCUMENT; "define" does NOT fire IMPLEMENT

```
source_id: xiigen-b06-held-out-047
pair_kind: held_out
```

A:
  run_id: "B06-ho-047"
  user_request: "Define the data privacy requirements for checkout."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "define" fires DOCUMENT ONLY. It does NOT fire IMPLEMENT.
  Anti-pattern avoided: treating "define" as an IMPLEMENT verb.

---

## Corruption Pair B06B1-WDA-COR-025 — v6-02: "define" incorrectly fires IMPLEMENT

```
source_id: xiigen-b06-corruption-025
pair_kind: corruption
corruption_type: Corruption-v6-02-IMPLEMENT-false-positive
```

A:
  user_request: "Define the fraud detection requirements."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  TransitionType: "documentation"

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"

Corruption: "define" incorrectly treated as IMPLEMENT verb.
Fix: "define" is Rule 6 — fires DOCUMENT only. It is NOT in IMPLEMENT lexicon.
  IMPLEMENT verbs are: write, create, build, develop, implement, code, refactor.
  "define" is not in this list.
