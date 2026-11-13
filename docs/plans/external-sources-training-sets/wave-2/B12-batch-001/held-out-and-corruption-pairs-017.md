# Held-Out and Corruption Pairs (017)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-017 — Held-out: "design GraphQL API" fires both ARCHITECTURE and API_CONTRACT

```
source_id: xiigen-b12-ho-017
pair_kind: held_out
hold_out_reason: "Tests that 'design' fires ARCHITECTURE and 'GraphQL' + 'api' fire API_CONTRACT simultaneously — neither suppresses the other"
```

A:
  user_request: "Design the GraphQL API schema for the product search service."

B (expected):
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  Routing: ["3.1", "3.3", "2.1"]
  TransitionType: "documentation"
  DecisionCount: 3
  Reason: "design" → ARCHITECTURE; "GraphQL" + "api" → API_CONTRACT; "schema" reinforces API_CONTRACT (graphql co-occurrence); implied "specify" → DOCUMENT

Held-out evaluation:
  Pass: CompiledPredicateIds contains both "ARCHITECTURE" and "API_CONTRACT"
  Fail (v6-01): ARCHITECTURE suppressed, only API_CONTRACT fires — or vice versa

---

## B12-COR-017 — Corruption: ARCHITECTURE suppressed on "design GraphQL API"

```
source_id: xiigen-b12-cor-017
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-017

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← ARCHITECTURE missing
  CorruptReason: "v6-01: classifier saw 'GraphQL' + 'api' and prioritised API_CONTRACT, treating 'design' as a description verb rather than an ARCHITECTURE trigger"

Correct: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
Corrupt: ["API_CONTRACT", "DOCUMENT"]
Rule: "design" always fires ARCHITECTURE. It does NOT get suppressed by domain-specific API terms. Both predicates fire.

---

## B12-COR-017B — Corruption: DecisionCount=2 instead of 3 for three-predicate design request

```
source_id: xiigen-b12-cor-017b
corruption_type: v6-03
pair_kind: corruption
```

A: same request as B12-HO-017

B (corrupt):
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  RoutingDecisions: [
    { predicate: "ARCHITECTURE", routes_to: "3.1" },
    { predicate: "API_CONTRACT", routes_to: "3.3" }
  ]
  DecisionCount: 2  ← CORRUPT (should be 3)

Correct: DecisionCount: 3 (3.1 + 3.3 + 2.1)
Corrupt: DecisionCount: 2 (DOCUMENT routing to 2.1 dropped)
CorruptReason: "v6-03: DOCUMENT predicate routing omitted; three predicates must produce three routing decisions"
