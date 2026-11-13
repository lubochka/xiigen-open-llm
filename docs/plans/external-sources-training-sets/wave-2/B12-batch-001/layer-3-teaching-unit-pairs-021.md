# Layer 3 — Teaching Unit Pairs (021)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-021 — Teaching: partner API — "publish" is NOT IMPLEMENT

```
source_id: xiigen-b12-tu-021
rule_id: B12-RULE-PUBLISH-NOT-IMPLEMENT
pair_kind: teaching_unit
```

### Concept

"Publish" means making an existing artifact available, not creating it.
Publishing an API specification = release/deployment concern, not code creation.
Therefore "publish" does NOT fire IMPLEMENT and TransitionType remains "documentation".

### Training Examples

A: "Publish the OpenAPI (Swagger) specification for the partner integration API."
B: IMPLEMENT does NOT fire ("publish" is not a code-creation verb)
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   TransitionType: documentation

---

A: "Write the OpenAPI specification for the partner integration API."
B: IMPLEMENT fires on "write" (writing the spec = creating it from scratch)
   CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
   TransitionType: feature_addition

---

A: "Publish and document the new v2 API contract for external partners."
B: "publish" → NOT IMPLEMENT; "document" → DOCUMENT
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   TransitionType: documentation

### The distinction

"write the spec" = creating a new artifact → IMPLEMENT → feature_addition
"publish the spec" = releasing an existing artifact → no IMPLEMENT → documentation
"update the spec" = modifying an existing artifact → no IMPLEMENT → documentation

### Memory aid

Ask: Is new code or a new artifact being CREATED?
  CREATE (write/build/implement/develop/scaffold/create) → IMPLEMENT
  RELEASE (publish) or MODIFY (update) → no IMPLEMENT
