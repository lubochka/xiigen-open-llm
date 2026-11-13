# Layer 0-C — Knowledge Edge Pairs (025)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-025-001 — GraphQL schema: fires API_CONTRACT when "graphql" is co-occurrence

```
source_id: xiigen-b12-0c-025-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the GraphQL API schema for the product catalog service."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "graphql" → API_CONTRACT ✓
    Rule B12-R02: "schema" fires API_CONTRACT because "graphql" is API co-occurrence ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  No v6 corruptions.
  Edge: "schema" with graphql co-occurrence → API_CONTRACT (contrast: database schema → no API_CONTRACT)

---

## B12-0C-025-002 — GraphQL "design" dual-fire: ARCHITECTURE + API_CONTRACT both present

```
source_id: xiigen-b12-0c-025-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Design the GraphQL API for the product catalog service and specify the query types."

B:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R12: "design" → ARCHITECTURE unconditional ✓
    Rule B12-R01: "graphql" → API_CONTRACT ✓
    Rule B12-R13: design + API term → dual-fire; neither suppresses the other ✓
    Rule B12-R04: "specify" → DOCUMENT ✓
  Key edge: ARCHITECTURE does NOT suppress API_CONTRACT and vice versa.
  v6 corruption risk: v6-01 — suppressing ARCHITECTURE because context is an API schema definition.

---

## B12-0C-025-003 — GraphQL wire up: API_CONTRACT fires; DOCUMENT does NOT fire without documentation verb

```
source_id: xiigen-b12-0c-025-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Wire up the GraphQL endpoint for the storefront to the product catalog service."

B:
  CompiledPredicateIds: ["API_CONTRACT"]
  Rule applications:
    Rule B12-R17b: "wire up" → API_CONTRACT (integration verb with API context) ✓
    Rule B12-R01: "graphql" → API_CONTRACT ✓
    Rule B12-R08: "endpoint" → API_CONTRACT ✓
  Boundary: "wire up" is an integration verb, not a documentation verb. DOCUMENT does NOT fire.
  No IMPLEMENT — "wire up" is not a construction verb.
  Edge: Integration verbs (wire up, connect, integrate, expose, publish) fire API_CONTRACT but NOT IMPLEMENT.
