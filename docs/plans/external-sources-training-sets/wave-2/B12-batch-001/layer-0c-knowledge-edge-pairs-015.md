# Layer 0-C — Knowledge Edge Pairs (015)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-015-001 — "interface" disambiguation: API context vs system design context (UserManagement)

```
source_id: xiigen-b12-0c-015-001
rule_id: B12-RULE-INTERFACE-DUAL
pair_kind: knowledge_edge
```

A:
  Case 1: "Define the user profile REST API interface including request and response fields."
  Case 2: "Design the user management service interface architecture."
  Case 3: "Design the user account management API and define the interface contract."

B:
  Case 1: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    "REST API interface" → "interface" fires API_CONTRACT (API context: REST + API present)
    No ARCHITECTURE — no "design" verb
    Result: API_CONTRACT + DOCUMENT

  Case 2: CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
    "design" → ARCHITECTURE fires
    "interface" → ARCHITECTURE fires (system design context; no API co-occurrence)
    No API_CONTRACT — no "api", "rest", "endpoint", "contract" present
    Result: ARCHITECTURE + DOCUMENT

  Case 3: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
    "design" → ARCHITECTURE fires
    "API" → API_CONTRACT fires
    "interface" → BOTH fire (design context present AND api context present)
    "contract" → API_CONTRACT fires
    Result: ARCHITECTURE + API_CONTRACT + DOCUMENT (dual-fire)

---

## B12-0C-015-002 — "authentication" in user API doc fires SECURITY (not optional)

```
source_id: xiigen-b12-0c-015-002
rule_id: B12-RULE-AUTHENTICATION-ALWAYS
pair_kind: knowledge_edge
```

A:
  CompiledText: "Document the user authentication API endpoints and token response schema."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Predicate analysis:
    "document" → DOCUMENT fires
    "authentication" → SECURITY fires ALWAYS (Core Rule 6 — unconditional)
    "API" → API_CONTRACT fires
    "endpoints" → API_CONTRACT fires
    "token response" — domain context (token = authentication artifact)
    "schema" → API_CONTRACT fires (API co-occurrence present)
  Result: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Teaching: SECURITY firing is NOT optional even when the dominant theme is API documentation.
  "User authentication API" — "authentication" is present → SECURITY must fire.

---

## B12-0C-015-003 — UserManagement: "implement" boundary for TransitionType

```
source_id: xiigen-b12-0c-015-003
rule_id: B12-RULE-TRANSITONTYPE-IMPLEMENT
pair_kind: knowledge_edge
```

A:
  Case 1: "Specify the user management API contract including CRUD operations."
  Case 2: "Implement the user registration API endpoint and write the OpenAPI spec."

B:
  Case 1: TransitionType = "documentation"
    "specify" → DOCUMENT fires (not IMPLEMENT)
    No IMPLEMENT trigger
    Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], TransitionType: documentation

  Case 2: TransitionType = "feature_addition"
    "implement" → IMPLEMENT fires
    "write" → IMPLEMENT fires
    Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"], TransitionType: feature_addition

  Teaching: TransitionType is determined solely by whether IMPLEMENT fires.
  "specify" is a documentation verb. "implement" and "write" are IMPLEMENT verbs.
  The same API context (user management, CRUD) produces different TransitionTypes
  based only on the VERB used.
