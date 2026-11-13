# Layer 3 — Teaching Unit Pairs (015)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-015 — Teaching: IMPLEMENT verb list — which verbs create code

```
source_id: xiigen-b12-tu-015
rule_id: B12-RULE-IMPLEMENT-VERBS
pair_kind: teaching_unit
```

### Concept

IMPLEMENT fires on code-creation verbs. Documentation and specification verbs do NOT fire IMPLEMENT.
This distinction determines TransitionType (feature_addition vs documentation) and whether
ImplementationFlow43 (4.3) appears in the routing.

### Training Examples

A: "Build the REST API for the product catalog service."
B: IMPLEMENT fires on "build" → feature_addition

---

A: "Create the GraphQL schema and resolver contract."
B: IMPLEMENT fires on "create" → feature_addition

---

A: "Specify the user management API contract."
B: IMPLEMENT does NOT fire ("specify" is a documentation verb) → documentation

### Complete verb classification table

IMPLEMENT fires (code-creation verbs):
  implement, build, create, write, develop, scaffold

IMPLEMENT does NOT fire (specification/documentation verbs):
  define, specify, document, describe, update, publish, review, outline, draft

IMPLEMENT does NOT fire (per explicit rule):
  integrate (Core Rule 17: "integrate" → API_CONTRACT requires co-occurrence; NOT IMPLEMENT)

### Memory aid

Ask: "Is this person creating new code or artifacts from scratch?"
YES (build/implement/create/write/develop/scaffold) → IMPLEMENT → feature_addition
NO (define/specify/document/update) → no IMPLEMENT → documentation
