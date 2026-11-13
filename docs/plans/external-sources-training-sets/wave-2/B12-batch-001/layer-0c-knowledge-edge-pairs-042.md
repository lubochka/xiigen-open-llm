# Layer 0-C — Knowledge Edge Pairs (042)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-042-001 — Rule: "API key" alone does NOT fire SECURITY; "authentication" fires SECURITY unconditionally

```
source_id: xiigen-b12-0c-042-001
pair_kind: knowledge_edge
```

### API key vs authentication SECURITY trigger distinction

"API key" is a credential type. The phrase "API key" alone does NOT fire SECURITY.
SECURITY fires on "authentication" (Core Rule 6: unconditional).

Examples:
  "Expose the API key management endpoints" → API_CONTRACT only ("api" + "endpoints")
  "Define the API key contract with key rotation" → API_CONTRACT + DOCUMENT (no SECURITY)
  "Define the API key contract with authentication" → API_CONTRACT + DOCUMENT + SECURITY
    The third case fires SECURITY because "authentication" is explicitly present.

The distinction:
  "API key" = credential type → API_CONTRACT context, not SECURITY alone
  "authentication" = security mechanism keyword → SECURITY (Core Rule 6: unconditional)

---

## B12-0C-042-002 — Rule: "connect the API key authentication layer" fires API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0c-042-002
pair_kind: knowledge_edge
```

### Authentication present with connect verb

"Connect the API key authentication layer to all service REST endpoints" fires:
  API_CONTRACT: "connect" (integration verb) + "api" + "rest" + "endpoints"
  SECURITY: "authentication" (Core Rule 6: unconditional)
  NOT IMPLEMENT: "connect" is an integration verb

Key teaching: even when "API key" precedes "authentication", SECURITY fires from "authentication"
alone. The SECURITY predicate fires from the keyword "authentication", not from "API key".

---

## B12-0C-042-003 — Rule: "implement the API key generation endpoint with authentication" fires IMPLEMENT + API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0c-042-003
pair_kind: knowledge_edge
```

### Three-predicate fire with API key management

"Implement the API key generation endpoint with authentication checks" fires:
  IMPLEMENT: "implement" (construction verb)
  API_CONTRACT: "api" + "endpoint"
  SECURITY: "authentication" (Core Rule 6: unconditional)
  TransitionType: feature_addition (IMPLEMENT present)

The "authentication checks" clause fires SECURITY even though it is secondary to the main
"implement the API key generation endpoint" clause. Core Rule 6 applies to the entire request
text; "authentication" fires SECURITY wherever it appears.
