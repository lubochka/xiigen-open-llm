# Layer 3 — Teaching Unit Pairs (004)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-004 — Teaching: "interface" fires both API_CONTRACT and ARCHITECTURE — context determines

```
source_id: xiigen-b12-tu-004
rule_id: B12-RULE-INTERFACE-DISAMBIGUATION
pair_kind: teaching_unit
```

### Concept

"interface" is context-sensitive. It can fire one or both predicates:

  API context (api, rest, grpc, graphql, contract, endpoint) → API_CONTRACT fires
  System design context (design, architecture, system, component) → ARCHITECTURE fires
  Both contexts simultaneously → BOTH predicates fire

### The Three Cases

Case 1: API context only → API_CONTRACT
  Signals present: API vocabulary. Signals absent: design vocabulary.
  
  A: "Specify the REST API interface for the payment endpoint."
  B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
     Reason: "REST API" = API context → API_CONTRACT. No "design" → no ARCHITECTURE.

Case 2: Design context only → ARCHITECTURE
  Signals present: design vocabulary. Signals absent: API vocabulary.
  
  A: "Design the interface between the order and inventory services."
  B: CompiledPredicateIds: ["ARCHITECTURE"]
     Reason: "design" = design context → ARCHITECTURE. No API vocabulary → no API_CONTRACT.

Case 3: Both contexts → ARCHITECTURE + API_CONTRACT
  Signals present: BOTH design vocabulary AND API vocabulary.
  
  A: "Design the service interface for the event bus and specify the API contract."
  B: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
     Reason: "design" → ARCHITECTURE. "API contract" → API_CONTRACT. Both fire simultaneously.

### Training Examples for Each Case

Example A (API only):
  "Define the gRPC interface for the shipping service."
  → CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  → "gRPC" = API context → API_CONTRACT fires on "interface"
  → No "design" → ARCHITECTURE does NOT fire

Example B (design only):
  "Design the interface between the authentication and user profile systems."
  → CompiledPredicateIds: ["ARCHITECTURE"]
  → "design" = design context → ARCHITECTURE fires
  → No API vocabulary → API_CONTRACT does NOT fire

Example C (both):
  "Design the REST API interface for the admin panel and document the endpoint contracts."
  → CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  → "design" → ARCHITECTURE; "REST API", "endpoint", "contracts" → API_CONTRACT; "document" → DOCUMENT

### Routing Impact

Case 1 (API_CONTRACT only): routes to 3.3
Case 2 (ARCHITECTURE only): routes to 3.1
Case 3 (ARCHITECTURE + API_CONTRACT): routes to BOTH 3.1 AND 3.3

### Memory Aid

"interface" question:
  Is "design" or "architecture" present? YES → ARCHITECTURE fires
  Is API vocabulary (api/rest/grpc/graphql/contract/endpoint) present? YES → API_CONTRACT fires
  Both present? Both fire.
  Neither present? Neither fires (and "interface" alone without context doesn't trigger either).
