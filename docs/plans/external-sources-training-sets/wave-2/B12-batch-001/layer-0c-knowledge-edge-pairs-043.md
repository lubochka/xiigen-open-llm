# Layer 0-C — Knowledge Edge Pairs (043)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-043-001 — Rule: "OAuth" fires SECURITY; "OAuth scope" does not fire a second predicate

```
source_id: xiigen-b12-0c-043-001
pair_kind: knowledge_edge
```

### OAuth scope and SECURITY firing

"OAuth" and "OAuth2" fire SECURITY. "OAuth scope" = the OAuth vocabulary used in API contracts.
The word "scope" does not add a separate predicate — SECURITY fires once via OAuth.

Examples:
  "Define the OAuth scope API contract" → SECURITY (oauth) + API_CONTRACT + DOCUMENT
  "Implement OAuth scope validation" → IMPLEMENT + API_CONTRACT + SECURITY
  "Connect OAuth scopes to the API gateway" → API_CONTRACT + SECURITY

SECURITY multiplicity: if both "OAuth" and "authentication" are present, SECURITY fires ONCE
(B12-R27). CompiledPredicateIds includes "SECURITY" once; DecisionCount = 1 for SECURITY routing.

---

## B12-0C-043-002 — Rule: "specify OAuth token scope requirements in the OpenAPI specification" fires API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0c-043-002
pair_kind: knowledge_edge
```

### OpenAPI + OAuth + specify triple fire

"Specify the OAuth token scope requirements for each endpoint in the OpenAPI specification" fires:
  DOCUMENT: "specify" + "specification" (two DOCUMENT triggers → one DOCUMENT predicate)
  API_CONTRACT: "endpoint" + "openapi" (two API_CONTRACT triggers → one API_CONTRACT predicate)
  SECURITY: "oauth"

Multiplicity applies to each predicate independently:
  Multiple DOCUMENT triggers → one DOCUMENT predicate
  Multiple API_CONTRACT triggers → one API_CONTRACT predicate
  Multiple SECURITY triggers (oauth + authentication) → one SECURITY predicate

---

## B12-0C-043-003 — Rule: "connect OAuth enforcement to API gateway" = API_CONTRACT + SECURITY (no IMPLEMENT)

```
source_id: xiigen-b12-0c-043-003
pair_kind: knowledge_edge
```

### Integration verb with OAuth security

"Connect the OAuth scope enforcement to the API gateway for all protected endpoints" fires:
  API_CONTRACT: "connect" (integration verb) + "api" + "endpoints"
  SECURITY: "oauth"
  NOT IMPLEMENT: "connect" is an integration verb

This tests the interaction of two key rules:
  1. "connect" = integration verb → API_CONTRACT but NOT IMPLEMENT
  2. "oauth" = SECURITY trigger (unconditional)

Result: [API_CONTRACT, SECURITY] — two predicates, two routing decisions (3.3 + 3.4).
TransitionType = documentation (no construction verb present).
