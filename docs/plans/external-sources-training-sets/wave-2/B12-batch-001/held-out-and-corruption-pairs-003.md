# Held-Out and Corruption Pairs (003)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-003-A — Held-out: "event schema" + API context → API_CONTRACT

```
source_id: xiigen-b12-ho-003-a
pair_kind: held_out
hold_out_reason: "Tests schema co-occurrence rule: webhook context satisfies API co-occurrence"
```

A:
  user_request: "Specify the event schema for the webhook payload of inventory updates."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  Reason: "schema" + "webhook" co-occurrence → API_CONTRACT fires; "event schema" alone would not

Pass condition: CompiledPredicateIds contains "API_CONTRACT"
Key: "webhook" is the API co-occurrence term that enables "schema" to fire API_CONTRACT

---

## B12-HO-003-B — Held-out: "database schema" → no API_CONTRACT

```
source_id: xiigen-b12-ho-003-b
pair_kind: held_out
hold_out_reason: "Tests schema without API co-occurrence → no API_CONTRACT"
```

A:
  user_request: "Define the database schema for the event log table."

B (expected):
  CompiledPredicateIds: ["DOCUMENT"]
  Routing: ["2.1"]
  Reason: "database schema" → "database" is not API co-occurrence → schema does NOT fire API_CONTRACT

Pass condition: CompiledPredicateIds does NOT contain "API_CONTRACT"

---

## B12-COR-003 — Corruption: "event schema" → API_CONTRACT fires even without API co-occurrence (wrong)

```
source_id: xiigen-b12-cor-003
corruption_type: schema_over-firing
pair_kind: corruption
```

A:
  user_request: "Define the event schema for the inventory update processor."
  (Note: no API co-occurrence — "processor" is not API context; no webhook/grpc/api/rest terms)

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← API_CONTRACT should NOT fire here
  CorruptReason: "schema over-firing: classifier incorrectly treated 'event' as API context
  and fired API_CONTRACT on 'schema' without checking for required API co-occurrence terms"

Corruption label: SCHEMA FIRES WITHOUT API CO-OCCURRENCE
Expected correct: ["DOCUMENT"]
Corrupt output: ["API_CONTRACT", "DOCUMENT"]

Teaching: "event schema for the inventory update processor" has no API co-occurrence terms.
"event" alone is not an API context term. The co-occurrence must be explicit API vocabulary
(api/rest/graphql/openapi/swagger/grpc/webhook). Without it, "schema" → no API_CONTRACT.

Contrast with held-out B12-HO-003-A: "webhook payload" provides the API co-occurrence
→ schema fires in that case. But "inventory update processor" provides no such co-occurrence.
