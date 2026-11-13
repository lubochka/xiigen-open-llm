# Held-Out and Corruption Pairs (014)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-014 — Held-out: "inventory API endpoints" fires API_CONTRACT

```
source_id: xiigen-b12-ho-014
pair_kind: held_out
hold_out_reason: "Tests that domain noun 'inventory' does not suppress API_CONTRACT when 'api' + 'endpoints' are present"
```

A:
  user_request: "Document the inventory API endpoints and their request/response schemas."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"
  Reason: "api" + "endpoints" → API_CONTRACT; "request"/"response" reinforce; "document" → DOCUMENT

Held-out evaluation:
  Pass: CompiledPredicateIds contains "API_CONTRACT"
  Fail (v6-01): "inventory" treated as order-management domain → API_CONTRACT suppressed → ["DOCUMENT"] only

---

## B12-COR-014 — Corruption: API_CONTRACT suppressed on "inventory API endpoints"

```
source_id: xiigen-b12-cor-014
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-014

B (corrupt):
  CompiledPredicateIds: ["DOCUMENT"]  ← API_CONTRACT missing
  CorruptReason: "v6-01: classifier saw 'inventory' and routed to inventory management sub-phase, bypassing API_CONTRACT detection"

Correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt: ["DOCUMENT"]

---

## B12-COR-014B — Corruption: "database schema" wrongly fires API_CONTRACT

```
source_id: xiigen-b12-cor-014b
corruption_type: v6-01
pair_kind: corruption
```

A:
  user_request: "Document the database schema for the inventory tables."

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← API_CONTRACT wrong
  CorruptReason: "v6-01: classifier saw 'schema' and fired API_CONTRACT without checking API co-occurrence; 'database' is not an API term"

Correct: ["DOCUMENT"]
Corrupt: ["API_CONTRACT", "DOCUMENT"]
Rule: "schema" fires API_CONTRACT ONLY with api/rest/graphql/openapi/swagger/grpc/webhook co-occurrence. "database schema" has no such co-occurrence.
