# Layer 0-C — Knowledge Edge Pairs (019)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-019-001 — NotificationAPI: "gRPC service contract" modifier rule

```
source_id: xiigen-b12-0c-019-001
rule_id: B12-RULE-CONTRACT-ALWAYS
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the gRPC service contract for the notification delivery microservice."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "define" → DOCUMENT fires
    "gRPC" → API_CONTRACT fires on "grpc"
    "service contract" → API_CONTRACT fires on "contract" (modifier "service" does not suppress)
    Double trigger: both "grpc" AND "contract" independently fire API_CONTRACT
  Result: ["API_CONTRACT", "DOCUMENT"]
  Teaching: "service contract" → "contract" is the trigger word; "service" is a modifier.
  Modifier rule: no word placed before "contract" suppresses API_CONTRACT.
  Full list: service/data/message/event/webhook/api/payload/response contract → all fire.

---

## B12-0C-019-002 — NotificationAPI: "webhook contract" is a double API_CONTRACT trigger

```
source_id: xiigen-b12-0c-019-002
rule_id: B12-RULE-WEBHOOK-CONTRACT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the notification webhook contract for push notification delivery events."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "define" → DOCUMENT fires
    "webhook" → API_CONTRACT fires (webhook is first-class trigger)
    "contract" → API_CONTRACT fires (contract is first-class trigger)
    Both "webhook" AND "contract" independently trigger API_CONTRACT
    Result: ONE API_CONTRACT instance (deduplication applies)
  Teaching: "webhook contract" has the highest API_CONTRACT evidence density.
  Two independent triggers → still one predicate. This makes "webhook contract" a
  very high-confidence API_CONTRACT case (confidence typically 0.95+).

---

## B12-0C-019-003 — NotificationAPI: boundary between notification domain and API contract domain

```
source_id: xiigen-b12-0c-019-003
rule_id: B12-RULE-DOMAIN-CONTEXT
pair_kind: knowledge_edge
```

A:
  Case 1: "Specify the notification API endpoints for sending and tracking requests."
  Case 2: "Design the notification scheduling algorithm and specify retry behavior."

B:
  Case 1: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    API_CONTRACT fires: "api", "endpoints", "requests"
    DOCUMENT fires: "specify"
    → 3.3 routing because API_CONTRACT fires

  Case 2: CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
    ARCHITECTURE fires: "design", "algorithm"
    DOCUMENT fires: "specify"
    No API_CONTRACT: no "api", "endpoint", "contract", "rest", "graphql" present
    → 3.1 routing (ARCHITECTURE) + 2.1 (DOCUMENT); 3.3 NOT included

  Teaching: The API_CONTRACT predicate is triggered by API-vocabulary words.
  "notification" is a domain word — it doesn't trigger any predicate on its own.
  The surrounding API vocabulary determines whether 3.3 is in the routing.
