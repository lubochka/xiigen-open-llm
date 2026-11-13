# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-006-001 — Rule: "authentication" → SECURITY fires ALWAYS in any context

```
source_id: xiigen-b12-0c-006-001
rule_id: B12-RULE-AUTHENTICATION-ALWAYS-SECURITY
pair_kind: knowledge_edge
```

### The Rule (Core Rule 6)

"authentication" fires SECURITY in ANY context, regardless of surrounding request vocabulary.
There are no context exceptions. "authentication" → SECURITY is unconditional.

This applies even when:
  - "authentication" appears in an API documentation request
  - "authentication" appears in an endpoint definition request
  - "authentication" appears in a specification writing request
  - "authentication" appears in an architectural design request
  - "authentication" appears in a data contract request

### Examples (all fire SECURITY)

In API doc context:
  "Document the REST API authentication requirements"
  → SECURITY fires on "authentication" (even though primary predicate is API_CONTRACT)
  → CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]

In gRPC contract context:
  "Define the gRPC contract including authentication flow"
  → SECURITY fires on "authentication"
  → CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]

In webhook context:
  "Specify the webhook authentication header"
  → SECURITY fires on "authentication"
  → CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]

In OpenAPI spec context:
  "Write the OpenAPI spec with authentication requirements"
  → SECURITY fires on "authentication"
  → CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]

In architecture context:
  "Design the API authentication architecture"
  → SECURITY fires on "authentication"
  → CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "SECURITY"]

---

## B12-0C-006-002 — "authentication" vs "authorization" distinction

```
source_id: xiigen-b12-0c-006-002
rule_id: B12-RULE-AUTH-DISTINCTION
pair_kind: knowledge_edge
```

Two similar terms that fire DIFFERENT predicates:

"authentication" → SECURITY (Core Rule 6)
  Routes to: 3.4 (SecurityAuditFlow34)
  Concept: verifying identity (who are you?)

"authorization" / "access control" / "oauth" → AUTHORIZATION (Core Rule 7)
  Routes to: 3.4 (AuthorizationFlow34)
  Concept: verifying permissions (what can you do?)

"token" → SECURITY (Core Rule 8)
  Routes to: 3.4 (SecurityAuditFlow34)
  Concept: credential/bearer token in security context

"oauth" → AUTHORIZATION + SECURITY (fires both simultaneously)
  Routes to: 3.4 twice (two separate routing decisions)
  Concept: OAuth is both an authorization protocol AND involves tokens (security)

### Examples

"Document the API authentication mechanism"
  → "authentication" → SECURITY fires
  → CompiledPredicateIds includes SECURITY
  → DecisionCount +1 for SECURITY (routes to 3.4)

"Document the API authorization scopes"
  → "authorization" → AUTHORIZATION fires (NOT SECURITY — no "authentication" term)
  → CompiledPredicateIds includes AUTHORIZATION
  → DecisionCount +1 for AUTHORIZATION (routes to 3.4)

"Document the OAuth 2.0 authentication and authorization flow"
  → "authentication" → SECURITY fires
  → "OAuth" → AUTHORIZATION fires + SECURITY fires again
  → CompiledPredicateIds includes SECURITY, AUTHORIZATION
  → DecisionCount +2 to 3.4

---

## B12-0C-006-003 — "token" fires SECURITY: examples

```
source_id: xiigen-b12-0c-006-003
rule_id: B12-RULE-TOKEN-SECURITY
pair_kind: knowledge_edge
```

"token" fires SECURITY (Core Rule 8) in API context.

Examples:
  "Define the API token endpoint" → "token" → SECURITY fires
  "Specify the token refresh mechanism" → "token" → SECURITY fires
  "Document the bearer token authentication" → "token" + "authentication" → SECURITY fires (twice)
  "Write the JWT token validation endpoint" → "token" → SECURITY fires; "authentication" absent

Note: "token" alone fires SECURITY without needing "authentication" as a co-occurrence.
When both "token" and "authentication" appear, SECURITY fires on both terms but produces
a single SECURITY predicate entry (not two SECURITY entries in CompiledPredicateIds).

---

## B12-0C-006-004 — v6-02 corruption: SECURITY suppressed in API doc context (the error explained)

```
source_id: xiigen-b12-0c-006-004
rule_id: B12-RULE-AUTHENTICATION-ALWAYS-SECURITY
corruption_type: v6-02
pair_kind: knowledge_edge
```

The v6-02 corruption happens when the model reasons:
  "This is an API documentation request → vocabulary like 'authentication' is just
  describing what the API does → it's API_CONTRACT vocabulary, not SECURITY vocabulary"

This reasoning is incorrect because:
  1. "authentication" fires SECURITY regardless of surrounding vocabulary
  2. API documentation requests CAN and DO involve security predicates
  3. SECURITY does not suppress API_CONTRACT and API_CONTRACT does not suppress SECURITY
  4. Both predicates fire simultaneously when both their trigger terms appear

Correct behavior:
  Request: "Document the OpenAPI authentication requirements"
  → Both API_CONTRACT fires (via "openapi", "api") AND SECURITY fires (via "authentication")
  → CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  → Routing: 3.3 + 2.1 + 3.4

Corrupted v6-02 behavior:
  → SECURITY suppressed because "authentication" classified as API context vocabulary
  → CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"] (SECURITY missing — WRONG)
  → Routing: 3.3 + 2.1 (3.4 missing — security review skipped)
