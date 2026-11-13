# Layer 0-C — Knowledge Edge Pairs (017)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-017-001 — "graphql" fires API_CONTRACT; "design" adds ARCHITECTURE (ProductSearch)

```
source_id: xiigen-b12-0c-017-001
rule_id: B12-RULE-DESIGN-GRAPHQL
pair_kind: knowledge_edge
```

A:
  Case 1: "Specify the GraphQL query contract for the product search API."
  Case 2: "Design the GraphQL API for the product search service."

B:
  Case 1: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    "specify" → DOCUMENT fires
    "GraphQL" → API_CONTRACT fires
    "query contract" → API_CONTRACT fires on "contract"
    "API" → API_CONTRACT fires
    No ARCHITECTURE ("specify" is not "design")

  Case 2: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
    "design" → ARCHITECTURE fires
    "GraphQL" → API_CONTRACT fires
    "API" → API_CONTRACT fires
    BOTH ARCHITECTURE and API_CONTRACT fire
    Teaching: "design" + API term → BOTH predicates fire simultaneously.

---

## B12-0C-017-002 — "schema" with GraphQL vs "schema" without API co-occurrence

```
source_id: xiigen-b12-0c-017-002
rule_id: B12-RULE-SCHEMA-CO-OCCURRENCE
pair_kind: knowledge_edge
```

A:
  Case 1: "Document the GraphQL schema types for product search."
  Case 2: "Document the product schema for the product catalog database."

B:
  Case 1: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    "document" → DOCUMENT fires
    "GraphQL" → API co-occurrence satisfied
    "schema" → API_CONTRACT fires (GraphQL co-occurrence present)

  Case 2: CompiledPredicateIds: ["DOCUMENT"]
    "document" → DOCUMENT fires
    "database" → NOT an API co-occurrence
    "schema" → does NOT fire API_CONTRACT (no API co-occurrence)
    Result: ["DOCUMENT"] only

  Rule: "schema" is context-sensitive. GraphQL/REST/API/OpenAPI/gRPC/webhook = API co-occurrence.
  "database/table/data model" = NOT API co-occurrence.

---

## B12-0C-017-003 — ProductSearch: "implement" GraphQL resolvers fires IMPLEMENT

```
source_id: xiigen-b12-0c-017-003
rule_id: B12-RULE-IMPLEMENT-GRAPHQL
pair_kind: knowledge_edge
```

A:
  CompiledText: "Implement the GraphQL resolvers for product search and create the schema definition."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "implement" → IMPLEMENT fires
    "GraphQL" → API_CONTRACT fires
    "create" → IMPLEMENT fires
    "schema" → API_CONTRACT fires (GraphQL co-occurrence)
    "definition" → DOCUMENT fires
  TransitionType: feature_addition (IMPLEMENT fires)
  Teaching: Both "implement" and "create" in the same request → IMPLEMENT fires.
  Multiple IMPLEMENT triggers do not add IMPLEMENT twice — still one IMPLEMENT instance.
  TransitionType is feature_addition whenever IMPLEMENT fires, regardless of how many
  IMPLEMENT triggers appear.
