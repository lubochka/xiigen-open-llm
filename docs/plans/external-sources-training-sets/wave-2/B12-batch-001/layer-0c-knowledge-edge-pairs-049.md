# Layer 0-C — Knowledge Edge Pairs (049)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-049-001 — Rule: "OpenAPI" is a first-class API_CONTRACT trigger

```
source_id: xiigen-b12-0c-049-001
pair_kind: knowledge_edge
```

### OpenAPI trigger rule

"OpenAPI" (and "Swagger") are first-class API_CONTRACT triggers. They fire API_CONTRACT
unconditionally when present in a request.

"Specification" and "spec" fire DOCUMENT (not API_CONTRACT).

Examples:
  "Define the OpenAPI specification" → API_CONTRACT (openapi) + DOCUMENT (define + specification)
  "Write the OpenAPI YAML" → IMPLEMENT (write) + API_CONTRACT (openapi)
  "Integrate the OpenAPI spec with the gateway" → API_CONTRACT (integrate with api) + DOCUMENT (spec)

Boundary: "specification" → DOCUMENT. "OpenAPI" → API_CONTRACT. These are independent triggers
for two different predicates.

---

## B12-0C-049-002 — Rule: "write the OpenAPI spec" fires IMPLEMENT because "write" is a construction verb

```
source_id: xiigen-b12-0c-049-002
pair_kind: knowledge_edge
```

### Write as construction verb in OpenAPI context

"Write the OpenAPI specification YAML for the inventory management REST API and document the schemas":
  IMPLEMENT: "write" (code/artifact creation verb — creating a YAML file)
  API_CONTRACT: "openapi" + "rest" + "api"
  DOCUMENT: "specification" + "document"

"Write" fires IMPLEMENT because it creates an artifact (the YAML file). This distinguishes:
  "Define the OpenAPI spec" → DOCUMENT only (define = documentation verb)
  "Write the OpenAPI spec" → IMPLEMENT + DOCUMENT (write = construction verb; spec = documentation trigger)

---

## B12-0C-049-003 — Rule: "design and specify the OpenAPI contract" fires ARCHITECTURE + API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0c-049-003
pair_kind: knowledge_edge
```

### Four-predicate fire in OpenAPI context

"Design and specify the OpenAPI contract for the billing service including authentication scheme definitions":
  ARCHITECTURE: "design" (unconditional)
  API_CONTRACT: "openapi" + "contract"
  DOCUMENT: "specify"
  SECURITY: "authentication" (Core Rule 6: unconditional)
  NOT IMPLEMENT: "design" and "specify" are not construction verbs

This is a 4-predicate request. DecisionCount = 4. Routes = [3.1, 3.3, 2.1, 3.4].
ARCHITECTURE executes first (tier 0). IMPLEMENT is absent → feature_addition does not apply.
TransitionType = documentation.
