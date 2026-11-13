# Held-Out and Corruption Pairs (013)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-013 — Held-out: "order webhook contract" fires API_CONTRACT

```
source_id: xiigen-b12-ho-013
pair_kind: held_out
hold_out_reason: "Tests generalization of webhook+contract double trigger"
```

A:
  user_request: "Specify the order webhook contract for shipment and cancellation events."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"
  Reason: "webhook" + "contract" → API_CONTRACT (double trigger); "specify" → DOCUMENT

Held-out evaluation:
  Pass: CompiledPredicateIds contains "API_CONTRACT"
  Fail (v6-01): "order" domain modifier suppresses API_CONTRACT → ["DOCUMENT"] only

---

## B12-COR-013 — Corruption: API_CONTRACT suppressed on "order webhook contract"

```
source_id: xiigen-b12-cor-013
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-013

B (corrupt):
  CompiledPredicateIds: ["DOCUMENT"]  ← API_CONTRACT missing
  CorruptReason: "v6-01: classifier saw 'order' domain word and treated request as order management, not API contract"

Correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt: ["DOCUMENT"]

---

## B12-COR-013B — Corruption: DecisionCount=1 for two-predicate order webhook

```
source_id: xiigen-b12-cor-013b
corruption_type: v6-03
pair_kind: corruption
```

A: same request as B12-HO-013

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  RoutingDecisions: [{ predicate: "API_CONTRACT", routes_to: "3.3" }]
  DecisionCount: 1  ← CORRUPT (should be 2)

Correct: DecisionCount: 2 (3.3 + 2.1)
Corrupt: DecisionCount: 1 (3.3 only, DOCUMENT routing dropped)
CorruptReason: "v6-03: two predicates collapsed to single primary sub-phase"
