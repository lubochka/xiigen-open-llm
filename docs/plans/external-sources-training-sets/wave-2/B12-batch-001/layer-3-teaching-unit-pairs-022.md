# Layer 3 — Teaching Unit Pairs (022)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-022 — Teaching: API versioning — "update" vs "implement" TransitionType

```
source_id: xiigen-b12-tu-022
rule_id: B12-RULE-UPDATE-VS-IMPLEMENT
pair_kind: teaching_unit
```

### Concept

The same versioning domain produces different TransitionTypes depending on the action verb.
"Update" an existing spec → documentation. "Implement" new behavior → feature_addition.

### Training Examples

A: "Update the Swagger spec to mark v1 API endpoints as deprecated."
B: "update" → NOT IMPLEMENT → documentation TransitionType
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   TransitionType: documentation
   Routes: 3.3 + 2.1

---

A: "Implement API version negotiation and write the response contract."
B: "implement" + "write" → IMPLEMENT fires → feature_addition TransitionType
   CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
   TransitionType: feature_addition
   Routes: 4.3 + 3.3 + 2.1

---

A: "Define the API versioning strategy and route contract for backward-compatible deprecation."
B: "define" → DOCUMENT; "route" → API_CONTRACT; no IMPLEMENT
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   TransitionType: documentation

### Why this matters for orchestration

documentation → APIContractFlow33 → DocumentationFlow21 → QA (sequential, 2 flows)
feature_addition → ImplementationFlow43 parallel APIContractFlow33 → DocumentationFlow21 → QA (3 flows, Phase1 parallel)

The verb changes the orchestration plan completely.
