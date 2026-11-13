# Held-Out and Corruption Pairs (016)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-016 — Held-out: "swagger" fires API_CONTRACT for shipping rates

```
source_id: xiigen-b12-ho-016
pair_kind: held_out
hold_out_reason: "Tests that 'swagger' is a first-class API_CONTRACT trigger independent of other API terms"
```

A:
  user_request: "Publish the Swagger documentation for the shipping rates calculation service."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"
  Reason: "swagger" → API_CONTRACT (first-class trigger, no co-occurrence needed); "documentation" → DOCUMENT
  Note: "publish" does NOT fire IMPLEMENT

Held-out evaluation:
  Pass: CompiledPredicateIds contains "API_CONTRACT"
  Fail (v6-01): "publish" misread as IMPLEMENT, or "swagger documentation" treated as DOCUMENT-only

---

## B12-COR-016 — Corruption: "publish" wrongly fires IMPLEMENT on Swagger docs

```
source_id: xiigen-b12-cor-016
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-016

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]  ← IMPLEMENT wrong
  CorruptReason: "v6-01: classifier treated 'publish' as IMPLEMENT verb; publish = distribution action, not construction; IMPLEMENT fires on build/implement/create/scaffold/write/develop only"

Correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
Rule: "publish" is NOT an IMPLEMENT trigger. It is a distribution/release verb only.

---

## B12-COR-016B — Corruption: DecisionCount=1 on two-predicate swagger request

```
source_id: xiigen-b12-cor-016b
corruption_type: v6-03
pair_kind: corruption
```

A: same request as B12-HO-016

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  RoutingDecisions: [{ predicate: "API_CONTRACT", routes_to: "3.3" }]
  DecisionCount: 1  ← CORRUPT (should be 2)

Correct: DecisionCount: 2 (3.3 + 2.1)
Corrupt: DecisionCount: 1 (3.3 only, DOCUMENT routing dropped)
CorruptReason: "v6-03: two predicates collapsed to single primary sub-phase; DOCUMENT routing to 2.1 must appear separately"
