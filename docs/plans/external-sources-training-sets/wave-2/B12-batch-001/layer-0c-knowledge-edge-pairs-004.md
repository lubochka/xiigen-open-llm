# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-004-001 — Rule: "interface" disambiguation — API context vs system design context

```
source_id: xiigen-b12-0c-004-001
rule_id: B12-RULE-INTERFACE-DISAMBIGUATION
pair_kind: knowledge_edge
```

### The Rule

"interface" is an ambiguous term that can fire different predicates depending on context:

Context A — API context:
  "interface" fires API_CONTRACT
  Signals: presence of API vocabulary (api, rest, grpc, graphql, webhook, openapi, swagger, contract, endpoint, route)

Context B — System design context:
  "interface" fires ARCHITECTURE
  Signals: presence of design vocabulary (design, architecture, system, component, service boundary, layer)

Context C — Both contexts simultaneously:
  "interface" fires BOTH API_CONTRACT AND ARCHITECTURE
  Signals: BOTH API vocabulary AND design vocabulary present in same request

### Examples for each context

Context A (API only → API_CONTRACT):
  "Specify the REST API interface for the product endpoint"
  → "REST API" present → API_CONTRACT fires
  → No "design" verb → ARCHITECTURE does NOT fire
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

Context B (system design only → ARCHITECTURE):
  "Design the interface between the order processing and inventory systems"
  → "design" present → ARCHITECTURE fires
  → No API vocabulary → API_CONTRACT does NOT fire
  CompiledPredicateIds: ["ARCHITECTURE"]

Context C (both → ARCHITECTURE + API_CONTRACT):
  "Design the service interface for the event bus and specify the API contract"
  → "design" present → ARCHITECTURE fires
  → "API contract" present → API_CONTRACT fires
  → Both signals simultaneously → both predicates fire
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]

---

## B12-0C-004-002 — Interface disambiguation examples table

```
source_id: xiigen-b12-0c-004-002
rule_id: B12-RULE-INTERFACE-DISAMBIGUATION
pair_kind: knowledge_edge
```

| Request phrase | Design signals | API signals | Predicates fired |
|----------------|----------------|-------------|-----------------|
| "design the interface between services" | design | none | ARCHITECTURE |
| "specify the REST interface" | none | REST | API_CONTRACT |
| "design the REST API interface" | design | REST, API | ARCHITECTURE + API_CONTRACT |
| "document the API interface" | none | API | API_CONTRACT |
| "design the messaging interface for event bus" | design | none explicit | ARCHITECTURE |
| "design the event bus API interface and document the contract" | design | API, contract | ARCHITECTURE + API_CONTRACT |
| "interface design for microservices" | design | none | ARCHITECTURE |
| "interface contract for the payment API" | none (no "design") | API, contract | API_CONTRACT |

---

## B12-0C-004-003 — "service interface" context: boundary case

```
source_id: xiigen-b12-0c-004-003
rule_id: B12-RULE-INTERFACE-DISAMBIGUATION
pair_kind: knowledge_edge
```

"service interface" is a common phrase that appears in both contexts.

Case A: "service interface" in system design context
  "Design the service interface for the inventory and warehouse systems"
  → "design" present → ARCHITECTURE fires on "interface"
  → No API vocabulary → API_CONTRACT does NOT fire via "interface"
  CompiledPredicateIds: ["ARCHITECTURE"]

Case B: "service interface" in API contract context
  "Specify the service interface contract for the inventory API"
  → "contract" present → API_CONTRACT fires on "contract"
  → "API" present → API_CONTRACT fires on "api"
  → No "design" verb → ARCHITECTURE does NOT fire
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

Case C: "service interface" in both contexts
  "Design the service interface contract for the inventory API"
  → "design" → ARCHITECTURE fires
  → "contract", "API" → API_CONTRACT fires
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]

Key distinction: The presence of "design" verb is the ARCHITECTURE trigger. The presence of API
vocabulary (api/rest/grpc/graphql/contract/endpoint) is the API_CONTRACT trigger.

---

## B12-0C-004-004 — "interface" in gRPC context: API_CONTRACT only

```
source_id: xiigen-b12-0c-004-004
rule_id: B12-RULE-INTERFACE-DISAMBIGUATION
pair_kind: knowledge_edge
```

gRPC and "interface" examples:

"Define the gRPC interface for the billing service"
  → "grpc" present → API_CONTRACT fires (on "grpc" AND on "interface" in API context)
  → No "design" → ARCHITECTURE does NOT fire
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

"Design the gRPC interface architecture for the billing service"
  → "design" present → ARCHITECTURE fires
  → "grpc" present → API_CONTRACT fires on "grpc" and "interface"
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]

Summary: gRPC + "interface" = API_CONTRACT (always, because gRPC is API context).
Adding "design" verb adds ARCHITECTURE as well.
