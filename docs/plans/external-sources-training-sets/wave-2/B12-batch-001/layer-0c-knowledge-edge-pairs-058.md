# Layer 0-C — Knowledge Edge Pairs (058)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-058-001 — Rule: "mock server" is a domain qualifier; API_CONTRACT fires from api/contract terms

```
source_id: xiigen-b12-0c-058-001
pair_kind: knowledge_edge
```

### Mock server domain qualifier

"Mock server" describes a test double or stub server. It is a domain qualifier.
API_CONTRACT fires from the API vocabulary present alongside "mock server".

Examples:
  "Define the mock server API contract" → API_CONTRACT (api + contract) + DOCUMENT (define)
  "Wire the mock server into the integration test environment for API contract validation" → API_CONTRACT (wire + api + contract)
  "Build the mock server from the OpenAPI specification" → IMPLEMENT (build) + API_CONTRACT (openapi)

"Integration test environment", "stub response" — domain qualifiers. Do not independently fire predicates.

---

## B12-0C-058-002 — Rule: "build the mock server from the OpenAPI specification and implement the stub response handlers" fires IMPLEMENT + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0c-058-002
pair_kind: knowledge_edge
```

### Specification fires DOCUMENT independently even when IMPLEMENT is present

"Build the mock server from the OpenAPI specification and implement the stub response handlers":
  IMPLEMENT: "build" + "implement" (two construction verbs → one IMPLEMENT predicate)
  API_CONTRACT: "openapi"
  DOCUMENT: "specification" (fires independently — spec is a DOCUMENT trigger regardless of context)

Key rule: IMPLEMENT does NOT suppress DOCUMENT. When both a construction verb and a documentation
term ("specification", "spec", "document") are present, both IMPLEMENT and DOCUMENT fire.

This is a 3-predicate case: [IMPLEMENT, API_CONTRACT, DOCUMENT].
Routing: [4.3, 3.3, 2.1]. TransitionType: feature_addition. DecisionCount: 3.

---

## B12-0C-058-003 — Rule: "expose mock API endpoints with token authentication" fires API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0c-058-003
pair_kind: knowledge_edge
```

### Authentication fires SECURITY even in mock/test context

"Expose the mock API endpoints with token authentication for the staging test environment":
  API_CONTRACT: "expose" (publication verb) + "api" + "endpoints"
  SECURITY: "authentication" (Core Rule 6: unconditional — even in test/staging context)
  NOT IMPLEMENT: "expose" is not a construction verb

Core Rule 6 applies even in mock server, staging, or test contexts. "Authentication" fires SECURITY
unconditionally. The test/staging context does not demote or suppress SECURITY.

Corrupt (v6-02): treating "token authentication" in a test environment as a staging artifact rather
than a SECURITY trigger. Authentication keywords fire SECURITY in all contexts.
