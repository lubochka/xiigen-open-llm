# Held-Out and Corruption Pairs (022)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-044 — "business" fires DOCUMENT (Rule 7 test)

```
source_id: xiigen-b06-held-out-044
pair_kind: held_out
```

A:
  run_id: "B06-ho-044"
  user_request: "Document the business rules for discount stacking at checkout."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["document", "business"]
  TransitionType: "documentation"
  Routing: ["2.1"]

Generalization test: "business" fires DOCUMENT (Rule 7). "document" fires DOCUMENT.

---

## Held-Out Pair B06B1-WDA-HO-045 — "narrative" fires DOCUMENT (Rule 13 test)

```
source_id: xiigen-b06-held-out-045
pair_kind: held_out
```

A:
  run_id: "B06-ho-045"
  user_request: "Describe the narrative for multi-currency checkout and loyalty redemption."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["describe", "narrative"]
  TransitionType: "documentation"
  Routing: ["2.1"]

Generalization test: "narrative" fires DOCUMENT (Rule 13).

---

## Corruption Pair B06B1-WDA-COR-024 — v6-01: "narrative" missed

```
source_id: xiigen-b06-corruption-024
pair_kind: corruption
corruption_type: Corruption-v6-01-DOCUMENT-missed
```

A:
  user_request: "Document the narrative for loyalty program checkout."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["document", "narrative"]

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT"]
  DOCUMENT terms: ["document"]
  Note: "narrative" missed even though it IS a DOCUMENT term.

Corruption: v6-01 variant — partial DOCUMENT term coverage. "narrative" missed.
Fix: Rule 13 — "narrative" is in DOCUMENT lexicon. Both "document" and "narrative" must fire.
  All lexicon terms must be detected, not just the first match.
