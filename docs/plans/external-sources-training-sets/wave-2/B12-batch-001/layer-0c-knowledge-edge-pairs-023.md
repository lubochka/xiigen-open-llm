# Layer 0-C — Knowledge Edge Pairs (023)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-023-001 — IMPLEMENT+API_CONTRACT: "build" creates code, not just documentation

```
source_id: xiigen-b12-0c-023-001
rule_id: B12-RULE-IMPLEMENT-FEATURE-ADDITION
pair_kind: knowledge_edge
```

A:
  Case 1: "Build the REST API for the product catalog service."
  Case 2: "Document the REST API for the product catalog service."

B:
  Case 1: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
    "build" → IMPLEMENT fires (build = create code)
    TransitionType: feature_addition
    Routing: 4.3 + 3.3 + 2.1

  Case 2: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    "document" → DOCUMENT fires (documentation verb, not code creation)
    No IMPLEMENT fires
    TransitionType: documentation
    Routing: 3.3 + 2.1

  Teaching: The verb is the sole determinant of TransitionType.
  "build" vs "document" with identical API context → different TransitionTypes and routing counts.

---

## B12-0C-023-002 — "update" does NOT fire IMPLEMENT

```
source_id: xiigen-b12-0c-023-002
rule_id: B12-RULE-UPDATE-NOT-IMPLEMENT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Update the API contract documentation to reflect the new response fields added to the order endpoint."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "API" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
    "documentation" → DOCUMENT fires
    "response" → API_CONTRACT fires
    "endpoint" → API_CONTRACT fires
    "update" → NOT IMPLEMENT (updating existing doc = maintenance, not code creation)
  TransitionType: documentation
  Teaching: "update" modifies an existing artifact. The key IMPLEMENT verbs (build/create/write/
  implement/develop/scaffold) all CREATE new artifacts. "update" does not create; it modifies.

---

## B12-0C-023-003 — IMPLEMENT verb coverage: six triggers compared

```
source_id: xiigen-b12-0c-023-003
rule_id: B12-RULE-IMPLEMENT-VERBS
pair_kind: knowledge_edge
```

All six IMPLEMENT verbs for B12 with API context examples:

1. "implement" → "Implement the API endpoint"
   IMPLEMENT fires ✓

2. "build" → "Build the REST API"
   IMPLEMENT fires ✓

3. "create" → "Create the GraphQL schema"
   IMPLEMENT fires ✓

4. "write" → "Write the OpenAPI specification"
   IMPLEMENT fires ✓

5. "develop" → "Develop the reporting API"
   IMPLEMENT fires ✓

6. "scaffold" → "Scaffold the API controller"
   IMPLEMENT fires ✓

Non-IMPLEMENT verbs (for contrast):
  "define" → DOCUMENT only
  "specify" → DOCUMENT only
  "document" → DOCUMENT only
  "update" → DOCUMENT only
  "publish" → neither
  "review" → neither
  "integrate" → API_CONTRACT (with co-occurrence), NOT IMPLEMENT

Teaching: IMPLEMENT fires on code-creation verbs only. Specification and documentation
verbs (define/specify/document/describe) do NOT fire IMPLEMENT.
