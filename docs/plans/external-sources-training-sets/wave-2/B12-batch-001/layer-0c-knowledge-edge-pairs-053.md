# Layer 0-C — Knowledge Edge Pairs (053)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-053-001 — Rule: "retry policy" is a domain qualifier; API_CONTRACT fires from API vocabulary

```
source_id: xiigen-b12-0c-053-001
pair_kind: knowledge_edge
```

### Retry policy domain qualifier

"Retry policy" is a resilience pattern term. It is a domain qualifier (like circuit breaker, timeout).
API_CONTRACT fires from the API vocabulary that accompanies it.

Examples:
  "Define the retry policy API contract" → API_CONTRACT (api + contract) + DOCUMENT (define)
  "Implement exponential backoff retry for all outbound REST API calls" → IMPLEMENT + API_CONTRACT
  "Connect the retry policy to the API client" → API_CONTRACT (connect + api)

"Exponential backoff", "jitter", "max retries" — all resilience configuration qualifiers.
None of these independently fire predicates.

---

## B12-0C-053-002 — Rule: idempotency + retry contract fires API_CONTRACT via both "contract" and "idempotent"

```
source_id: xiigen-b12-0c-053-002
pair_kind: knowledge_edge
```

### Idempotency reinforces API_CONTRACT in retry context

"Specify the retry contract for idempotent REST API operations to avoid duplicate processing":
  API_CONTRACT: "contract" (Core Rule 2) + "idempotent" (B12-R18) + "rest" + "api"
  DOCUMENT: "specify"
  Multiple API_CONTRACT triggers → ONE API_CONTRACT predicate
  DecisionCount: 2 (not 4)

The multiple API_CONTRACT triggers ("contract", "idempotent", "rest", "api") all reinforce
API_CONTRACT confidence but produce ONE predicate. Confidence is higher (0.95) due to multiple
triggers, but the predicate count remains 1.

---

## B12-0C-053-003 — Boundary: "connect retry policy to API client" — "API client" fires API_CONTRACT

```
source_id: xiigen-b12-0c-053-003
pair_kind: knowledge_edge
```

### API client as API context term

"Connect the retry policy configuration to the API client used for downstream service calls":
  API_CONTRACT: "connect" (integration verb) + "api" (from "API client")
  "api client" provides the API co-occurrence needed for "connect" to fire API_CONTRACT

Note: "API client" is a consumer of an API contract. Its presence provides API co-occurrence.
"Connect ... to the API client" → API_CONTRACT fires.
