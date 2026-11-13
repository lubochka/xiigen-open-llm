# Held-Out and Corruption Pairs (021)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-021 — Held-out: "partner API integration contract" fires API_CONTRACT (integration with co-occurrence)

```
source_id: xiigen-b12-ho-021
pair_kind: held_out
hold_out_reason: "Tests Core Rule 17: 'integration' fires API_CONTRACT when API co-occurrence is present"
```

A:
  user_request: "Specify the partner API integration contract for third-party payment processors."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"
  Reason: "api" + "contract" → API_CONTRACT (double trigger); "integration" reinforces API_CONTRACT (api co-occurrence satisfied); "specify" → DOCUMENT

Held-out evaluation:
  Pass: CompiledPredicateIds contains "API_CONTRACT"
  Fail (v6-01): "integration" treated as independent domain word → overrides API signals → API_CONTRACT suppressed

---

## B12-COR-021 — Corruption: "integration" suppresses API_CONTRACT on partner API contract

```
source_id: xiigen-b12-cor-021
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-021

B (corrupt):
  CompiledPredicateIds: ["DOCUMENT"]  ← API_CONTRACT missing
  CorruptReason: "v6-01: classifier saw 'integration' and routed to integration-management domain, ignoring 'api' + 'contract' triggers; API_CONTRACT should fire on 'api' + 'contract' regardless of 'integration' presence"

Correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt: ["DOCUMENT"]

---

## B12-COR-021B — Corruption: "publish" wrongly fires IMPLEMENT on partner API spec

```
source_id: xiigen-b12-cor-021b
corruption_type: v6-01
pair_kind: corruption
```

A:
  user_request: "Publish the OpenAPI specification for the partner payment integration API."

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]  ← IMPLEMENT wrong
  CorruptReason: "v6-01: classifier fired IMPLEMENT on 'publish'; publish = distribution/release action, not construction; IMPLEMENT triggers are: build, implement, create, scaffold, write, develop"

Correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
Correct reasoning: "OpenAPI" → API_CONTRACT; "specification" → DOCUMENT; "publish" → no predicate
