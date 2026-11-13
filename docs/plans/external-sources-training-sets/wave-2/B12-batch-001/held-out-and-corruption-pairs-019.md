# Held-Out and Corruption Pairs (019)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-019 — Held-out: "gRPC contract" fires API_CONTRACT for notification service

```
source_id: xiigen-b12-ho-019
pair_kind: held_out
hold_out_reason: "Tests that gRPC is recognised as a first-class API_CONTRACT trigger, and that contract modifier reinforces the trigger"
```

A:
  user_request: "Specify the gRPC service contract for the notification delivery system."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"
  Reason: "gRPC" → API_CONTRACT (first-class); "contract" → API_CONTRACT (unconditional); "service contract" = double trigger; "specify" → DOCUMENT

Held-out evaluation:
  Pass: CompiledPredicateIds contains "API_CONTRACT"
  Fail (v6-01): "gRPC" unrecognised as API term → API_CONTRACT suppressed → ["DOCUMENT"] only

---

## B12-COR-019 — Corruption: gRPC not recognised as API_CONTRACT trigger

```
source_id: xiigen-b12-cor-019
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-019

B (corrupt):
  CompiledPredicateIds: ["DOCUMENT"]  ← API_CONTRACT missing
  CorruptReason: "v6-01: classifier did not recognise 'gRPC' as an API protocol term; gRPC is a first-class API_CONTRACT trigger equivalent to REST/GraphQL/OpenAPI/Swagger"

Correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt: ["DOCUMENT"]

---

## B12-COR-019B — Corruption: "service contract" treated as non-API contract type

```
source_id: xiigen-b12-cor-019b
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-019

B (corrupt):
  CompiledPredicateIds: ["DOCUMENT"]  ← API_CONTRACT missing
  CorruptReason: "v6-01: classifier saw 'service contract' and treated 'service' as a domain modifier that overrides 'contract'; any modifier + 'contract' fires API_CONTRACT unconditionally — data contract, service contract, event contract, delivery contract all fire API_CONTRACT"

Correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt: ["DOCUMENT"]
Rule: "contract" fires API_CONTRACT regardless of preceding modifier. "service contract" = API_CONTRACT trigger.
