# Layer 0-C — Knowledge Edge Pairs (022)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-022-001 — API versioning: "versioned" modifier on contract does not suppress

```
source_id: xiigen-b12-0c-022-001
rule_id: B12-RULE-CONTRACT-ALWAYS
pair_kind: knowledge_edge
```

A:
  CompiledText: "Document the versioned API contract changes between v1 and v2 of the order service."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "document" → DOCUMENT fires
    "API" → API_CONTRACT fires
    "contract" → API_CONTRACT fires (modifier "versioned" does not suppress)
    "endpoints" — domain context
  Result: ["API_CONTRACT", "DOCUMENT"]
  Teaching: "versioned API contract" — "versioned" is a modifier describing the state of the
  contract. The "contract" word is still the trigger. Modifier "versioned" is irrelevant to
  whether API_CONTRACT fires.

---

## B12-0C-022-002 — API versioning: "update" Swagger spec → documentation (not feature_addition)

```
source_id: xiigen-b12-0c-022-002
rule_id: B12-RULE-UPDATE-NOT-IMPLEMENT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Update the Swagger spec to mark v1 API endpoints as deprecated and specify the v2 contract migration path."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "Swagger" → API_CONTRACT fires
    "spec" → DOCUMENT fires
    "API" → API_CONTRACT fires
    "endpoints" → API_CONTRACT fires
    "specify" → DOCUMENT fires
    "contract" → API_CONTRACT fires
    "update" → NOT IMPLEMENT (maintenance/modification verb)
  TransitionType: documentation
  Teaching: "update" is a maintenance verb. Updating an existing spec = documentation.
  Contrast: "write the Swagger spec" → IMPLEMENT fires → feature_addition.
  Rule: IMPLEMENT fires on create-new, not update-existing.

---

## B12-0C-022-003 — API versioning: "design" versioning strategy → ARCHITECTURE fires

```
source_id: xiigen-b12-0c-022-003
rule_id: B12-RULE-DESIGN-ARCHITECTURE
pair_kind: knowledge_edge
```

A:
  CompiledText: "Design the API versioning strategy and specify the OpenAPI version migration contract."

B:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "design" → ARCHITECTURE fires
    "API" → API_CONTRACT fires
    "specify" → DOCUMENT fires
    "OpenAPI" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
  Result: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  Teaching: "design" fires ARCHITECTURE. "design" + API terms → both ARCHITECTURE and
  API_CONTRACT fire. The versioning strategy is a system-design (3.1) AND API-contract (3.3) concern.
  Both sub-phases receive routing decisions. Neither suppresses the other.
