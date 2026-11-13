# Layer 0-C — Knowledge Edge Pairs (024)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-024-001 — Complete predicate boundary summary for B12

```
source_id: xiigen-b12-0c-024-001
pair_kind: knowledge_edge_reference
```

### Boundary 1: "schema" with vs without API co-occurrence

Fires API_CONTRACT:
  "GraphQL schema", "REST schema", "OpenAPI schema", "API schema", "gRPC schema", "webhook schema"

Does NOT fire API_CONTRACT:
  "database schema", "data model schema", "table schema", "entity schema"

---

### Boundary 2: "interface" — API context vs system design context

Fires API_CONTRACT (only):
  "REST API interface", "gRPC interface", "GraphQL interface" (no "design" verb)

Fires ARCHITECTURE (only):
  "service interface architecture" (no API terms present)

Fires BOTH (dual-fire):
  "design the API interface", "design the service interface and API contract"

---

### Boundary 3: "contract" modifier rule

ALL of these fire API_CONTRACT (modifier is irrelevant):
  data contract, service contract, event contract, message contract, webhook contract,
  payload contract, response contract, versioning contract, idempotent contract

---

### Boundary 4: "authentication" unconditional SECURITY

Always fires SECURITY:
  "authentication requirements", "authentication scheme", "HMAC authentication",
  "JWT authentication", "OAuth authentication", "API key authentication",
  "authentication token", "authentication credentials"

Never suppressed by: API context, webhook context, documentation context, OpenAPI context.

---

### Boundary 5: IMPLEMENT verbs vs documentation verbs

IMPLEMENT fires: build, implement, create, write, develop, scaffold
IMPLEMENT does NOT fire: define, specify, document, describe, update, publish, review, integrate

---

## B12-0C-024-002 — Three-predicate knowledge edge: API_CONTRACT + SECURITY + DOCUMENT

```
source_id: xiigen-b12-0c-024-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Document the notification API authentication scheme and webhook security requirements."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Flow: FLOW_EXECUTING
  Routing: 3.3 + 2.1 + 3.4
  Rule applications:
    Rule B12-R01: "webhook" → API_CONTRACT ✓
    Rule B12-R06: "authentication" → SECURITY always ✓
    Rule B12-R04: "document" → DOCUMENT ✓
  No v6 corruptions.
  DecisionCount: 3 (three predicates, three routing decisions)

---

## B12-0C-024-003 — Summary: which rules are exercised in files 013–024

```
source_id: xiigen-b12-0c-024-003
pair_kind: knowledge_edge_reference
```

Rules exercised in knowledge-edge files 013–024:

| File | Primary rule |
|------|-------------|
| 013 | webhook contract double-trigger; idempotent; authentication in webhook |
| 014 | database schema vs API schema; route trigger; payload not independent |
| 015 | interface disambiguation; authentication unconditional; IMPLEMENT boundary |
| 016 | swagger+openapi dual trigger; idempotent in shipping; integration co-occurrence |
| 017 | design+graphql dual-fire; schema with/without GraphQL co-occurrence; implement verbs |
| 018 | spec fires DOCUMENT not API_CONTRACT; scaffold→IMPLEMENT; develop→IMPLEMENT |
| 019 | gRPC contract modifier; webhook contract double trigger; notification domain boundary |
| 020 | authentication in upload context; idempotent upload; multipart request in API context |
| 021 | integration co-occurrence; design+interface dual-fire; publish not IMPLEMENT |
| 022 | versioned modifier on contract; update not IMPLEMENT; design→ARCHITECTURE |
| 023 | build vs document TransitionType; update not IMPLEMENT; IMPLEMENT verb coverage |
| 024 | Complete boundary summary; three-predicate KE; rules index |
