# Held-Out and Corruption Pairs (020)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-040 — "specify" alone fires DOCUMENT (not IMPLEMENT)

```
source_id: xiigen-b06-held-out-040
pair_kind: held_out
```

A:
  run_id: "B06-ho-040"
  user_request: "Specify the fraud detection spec for chargebacks."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["specify", "spec"]
  TransitionType: "documentation"
  Routing: ["2.1"]

Generalization test: "specify" fires DOCUMENT only — NOT IMPLEMENT.
Anti-pattern: Treating "specify" as an IMPLEMENT verb.

---

## Held-Out Pair B06B1-WDA-HO-041 — "describe" alone fires DOCUMENT (not IMPLEMENT)

```
source_id: xiigen-b06-held-out-041
pair_kind: held_out
```

A:
  run_id: "B06-ho-041"
  user_request: "Describe the functional behavior for subscription renewal."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["describe", "functional", "behavior"]
  TransitionType: "documentation"

Generalization test: "describe" fires DOCUMENT only — NOT IMPLEMENT. No IMPLEMENT verb present.

---

## Corruption Pair B06B1-WDA-COR-022 — v6-02: IMPLEMENT falsely fired for "describe"

```
source_id: xiigen-b06-corruption-022
pair_kind: corruption
corruption_type: Corruption-v6-02-IMPLEMENT-false-positive
```

A:
  user_request: "Describe the subscription billing use cases."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  TransitionType: "documentation"

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"

Corruption: "describe" incorrectly treated as an IMPLEMENT verb.
Fix: "describe" is in DOCUMENT lexicon — fires DOCUMENT only.
  IMPLEMENT verbs are: write, create, build, develop, implement, code, refactor, etc.
  "describe" is NOT in IMPLEMENT lexicon.
