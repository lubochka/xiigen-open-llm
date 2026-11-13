# Layer 0-C — Knowledge Edge Pairs (055)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-055-001 — Rule: "monitoring" is a domain qualifier; health check endpoint fires API_CONTRACT

```
source_id: xiigen-b12-0c-055-001
pair_kind: knowledge_edge
```

### Monitoring domain qualifier and health check endpoint

"Monitoring" is an operational domain qualifier. It does not independently fire any predicate.
API_CONTRACT fires from the API vocabulary that accompanies monitoring context.

Examples:
  "Define the API monitoring contract" → API_CONTRACT (api + contract) + DOCUMENT (define)
  "Implement the API health check endpoint" → IMPLEMENT + API_CONTRACT (api + endpoint)
  "Integrate the API monitoring tool with the gateway" → API_CONTRACT (integrate + api + endpoint)

"Health check endpoint" provides API_CONTRACT via "endpoint" (first-class trigger).
The "health check" modifier is a domain qualifier describing the endpoint's function.

---

## B12-0C-055-002 — Rule: "integrate the API monitoring tool" fires API_CONTRACT (integrate ≠ IMPLEMENT)

```
source_id: xiigen-b12-0c-055-002
pair_kind: knowledge_edge
```

### Integrate verb in monitoring context

"Integrate the API monitoring tool with the gateway to track latency per endpoint":
  API_CONTRACT: "integrate" (with API co-occurrence) + "api" + "endpoint"
  NOT IMPLEMENT: "integrate" is not a construction verb (B12-R17)
  NOT DOCUMENT: no documentation verb present

"Tracking latency" is a monitoring activity description. It does not fire any predicate.
The monitoring purpose clause ("to track latency per endpoint") provides "endpoint" as an
API_CONTRACT trigger.

---

## B12-0C-055-003 — Rule: "implement health check endpoint" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0c-055-003
pair_kind: knowledge_edge
```

### Implement verb in health check context

"Implement the API health check endpoint that reports service status and dependency health":
  IMPLEMENT: "implement" (explicit construction verb)
  API_CONTRACT: "api" + "endpoint"
  NOT DOCUMENT: no documentation verb
  TransitionType: feature_addition (IMPLEMENT present)

The distinction:
  "Define the API health check contract" → API_CONTRACT + DOCUMENT (define = documentation verb)
  "Implement the API health check endpoint" → IMPLEMENT + API_CONTRACT (implement = construction verb)
  "Integrate the API monitoring tool" → API_CONTRACT (integrate = integration verb)

Three distinct verbs → three distinct predicate patterns.
