# Layer 3 — Teaching Unit Pairs (014)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-014 — Teaching: "database schema" vs "API schema" — the critical boundary

```
source_id: xiigen-b12-tu-014
rule_id: B12-RULE-SCHEMA-CO-OCCURRENCE
pair_kind: teaching_unit
```

### Concept

"Schema" is context-sensitive. It fires API_CONTRACT ONLY when an API co-occurrence term
is present. "Database" is NOT an API co-occurrence term.

### Training Examples

A: "Define the database schema for the inventory stock levels table."
B: CompiledPredicateIds: ["DOCUMENT"]
   Reason: "database" provides no API co-occurrence → "schema" does NOT fire API_CONTRACT

---

A: "Define the REST API response schema for the inventory stock-check endpoint."
B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   Reason: "REST" + "API" provide co-occurrence → "schema" fires API_CONTRACT

---

A: "Document the GraphQL schema types for product search."
B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   Reason: "GraphQL" provides co-occurrence → "schema" fires API_CONTRACT

### The rule in one sentence

"Schema" fires API_CONTRACT only when the word api, rest, graphql, openapi, swagger, grpc,
or webhook appears in the same request. Without these co-occurrence terms, "schema" contributes
DOCUMENT only (if "define/document/specify" also present).

### Common error (v6-01 variant)

CORRUPT reasoning: "schema" always fires API_CONTRACT
→ "Define the database schema" → API_CONTRACT fires → WRONG
CORRECT: "database schema" → no API co-occurrence → no API_CONTRACT
