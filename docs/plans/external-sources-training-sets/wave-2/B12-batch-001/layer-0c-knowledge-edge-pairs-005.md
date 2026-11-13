# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-005-001 — Rule: "design" + API vocabulary → ARCHITECTURE + API_CONTRACT both fire

```
source_id: xiigen-b12-0c-005-001
rule_id: B12-RULE-DESIGN-API-COMBINATION
pair_kind: knowledge_edge
```

### The Rule

"design" alone → ARCHITECTURE (Core Rule 5, first clause)
"design the API" (with API vocabulary) → ARCHITECTURE + API_CONTRACT (Core Rule 5, second clause)

The combination rule: when "design" (or "architecture") appears with any API_CONTRACT vocabulary,
BOTH predicates fire. The API vocabulary does not suppress ARCHITECTURE, and ARCHITECTURE does
not suppress API_CONTRACT.

### Examples

"design" alone → ARCHITECTURE only:
  "Design the microservice architecture for the platform"
  → "design", "architecture" → ARCHITECTURE fires
  → No API vocabulary → API_CONTRACT does NOT fire
  CompiledPredicateIds: ["ARCHITECTURE"]

"design" + API vocabulary → ARCHITECTURE + API_CONTRACT:
  "Design the REST API for the product catalog"
  → "design" → ARCHITECTURE fires
  → "REST", "API" → API_CONTRACT fires
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]

"design the API architecture" (both design and architecture):
  "Design the REST API architecture for the checkout service"
  → "design", "architecture" → ARCHITECTURE fires
  → "REST", "API" → API_CONTRACT fires
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]

---

## B12-0C-005-002 — "design" + API vocabulary combination table

```
source_id: xiigen-b12-0c-005-002
rule_id: B12-RULE-DESIGN-API-COMBINATION
pair_kind: knowledge_edge
```

| Request contains | ARCHITECTURE? | API_CONTRACT? |
|------------------|---------------|----------------|
| "design" (alone, no API vocab) | YES | NO |
| "architecture" (alone, no API vocab) | YES | NO |
| "design" + "rest" | YES | YES |
| "design" + "api" | YES | YES |
| "design" + "graphql" | YES | YES |
| "design" + "endpoint" | YES | YES |
| "design" + "contract" | YES | YES |
| "design" + "route" | YES | YES |
| "design" + "webhook" | YES | YES |
| "design" + "grpc" | YES | YES |
| "design" + "openapi" | YES | YES |
| "design" + "interface" + API vocab | YES | YES |
| "design" + "schema" + API co-occurrence | YES | YES |
| "design" + "schema" (no API co-occurrence) | YES | NO |

---

## B12-0C-005-003 — "design the route structure": ARCHITECTURE + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0c-005-003
rule_id: B12-RULE-DESIGN-API-COMBINATION
pair_kind: knowledge_edge
```

Example: "Design the REST API architecture for the admin panel and document the route structure."

Analysis:
  "design" → ARCHITECTURE fires
  "REST API architecture" → ARCHITECTURE fires (on "architecture"), API_CONTRACT fires (on "rest", "api")
  "document" → DOCUMENT fires
  "route" → API_CONTRACT fires (on "route" in API context)

Final: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]

Note: "route" in API context = URL routing patterns, HTTP method mappings, API endpoint paths.
"route" fires API_CONTRACT when used in the context of API URL routing or API path definitions.

---

## B12-0C-005-004 — "design" + "schema" requires separate co-occurrence check

```
source_id: xiigen-b12-0c-005-004
rule_id: B12-RULE-DESIGN-API-COMBINATION
pair_kind: knowledge_edge
```

When "design" appears with "schema", there are two separate checks:

Check 1: Does "design" fire ARCHITECTURE? → YES (always)
Check 2: Does "schema" fire API_CONTRACT? → ONLY if API co-occurrence present

Example A: "Design the database schema for the product catalog"
  "design" → ARCHITECTURE fires
  "schema" → needs API co-occurrence → "database" is not API co-occurrence → NO
  CompiledPredicateIds: ["ARCHITECTURE"]

Example B: "Design the GraphQL schema for the product catalog"
  "design" → ARCHITECTURE fires
  "GraphQL" → API_CONTRACT fires on "graphql"
  "schema" → API_CONTRACT fires (with "GraphQL" co-occurrence present)
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]

Summary: "design" + "schema" → ARCHITECTURE always fires; API_CONTRACT fires only if API
co-occurrence accompanies "schema". The checks are independent.
