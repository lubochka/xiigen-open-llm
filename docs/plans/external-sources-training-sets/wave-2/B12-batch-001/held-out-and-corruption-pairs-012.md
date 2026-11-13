# Held-Out and Corruption Pairs (012)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-012-A — Held-out: "message contract" + ARCHITECTURE context

```
source_id: xiigen-b12-ho-012-a
pair_kind: held_out
hold_out_reason: "Tests combination: design context + contract modifier → ARCHITECTURE + API_CONTRACT"
```

A:
  user_request: "Design the message contract architecture for the event-driven microservices platform."

B (expected):
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]
  TransitionType: "documentation"
  Routing: ["3.1", "3.3"]
  Reason:
    "design" → ARCHITECTURE fires
    "architecture" → ARCHITECTURE fires again (same predicate)
    "message contract" → API_CONTRACT fires on "contract" (Rule 2)

Pass condition: both "ARCHITECTURE" AND "API_CONTRACT" present in CompiledPredicateIds

---

## B12-HO-012-B — Held-out: gRPC + schema + protobuf → API_CONTRACT triple trigger

```
source_id: xiigen-b12-ho-012-b
pair_kind: held_out
hold_out_reason: "Tests three API_CONTRACT triggers in one request (gRPC, schema, protobuf)"
```

A:
  user_request: "Define the gRPC service schema and document the protobuf message types for the inventory service."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  Routing: ["3.3", "2.1"]
  Reason:
    "gRPC" → API_CONTRACT fires
    "service schema" → API_CONTRACT fires on "schema" (with gRPC co-occurrence)
    "protobuf" → API_CONTRACT fires (with gRPC co-occurrence — Rule 28)
    "define" → DOCUMENT fires
    "document" → DOCUMENT fires
    Three API_CONTRACT triggers → single API_CONTRACT predicate entry (not three entries)

Pass condition: CompiledPredicateIds = ["API_CONTRACT", "DOCUMENT"]
(Three triggers → single predicate. Multiple triggers increase confidence but don't duplicate.)

---

## B12-HO-012-C — Held-out: "integrate" with REST API → API_CONTRACT but not IMPLEMENT

```
source_id: xiigen-b12-ho-012-c
pair_kind: held_out
hold_out_reason: "Tests Rule 17 in complex integration description"
```

A:
  user_request: "Describe how to integrate the customer rewards REST API and document the endpoint request formats."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  Routing: ["3.3", "2.1"]
  Reason:
    "integrate" → API_CONTRACT fires (Rule 17: integrate → API_CONTRACT, NOT IMPLEMENT)
    "REST API" → API_CONTRACT fires
    "endpoint" → API_CONTRACT fires
    "describe" → DOCUMENT fires
    "document" → DOCUMENT fires
    IMPLEMENT does NOT fire ("integrate" + "describe" + "document" — none are build verbs)

Pass condition: CompiledPredicateIds does NOT contain "IMPLEMENT"

---

## B12-HO-012-D — Held-out: full five-predicate without ARCHITECTURE

```
source_id: xiigen-b12-ho-012-d
pair_kind: held_out
hold_out_reason: "Tests five-predicate without architecture context"
```

A:
  user_request: "Implement the OAuth 2.0 token endpoint, write the OpenAPI specification, and document the authentication and authorization requirements."

B (expected):
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  TransitionType: "feature_addition"
  DecisionCount: 5
  Routing: ["4.3", "3.3", "2.1", "3.4", "3.4"]
  Reason:
    "implement" → IMPLEMENT
    "OAuth 2.0" → AUTHORIZATION + SECURITY
    "token" → SECURITY
    "OpenAPI" → API_CONTRACT
    "specification" → DOCUMENT
    "write" → IMPLEMENT
    "document" → DOCUMENT
    "authentication" → SECURITY
    "authorization" → AUTHORIZATION

Pass conditions:
  - CompiledPredicateIds contains all five predicates
  - DecisionCount = 5
  - ARCHITECTURE NOT in CompiledPredicateIds (no "design/architecture" in request)
  - TransitionType = "feature_addition"
