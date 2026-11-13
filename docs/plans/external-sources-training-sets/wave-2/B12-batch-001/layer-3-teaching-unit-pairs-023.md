# Layer 3 — Teaching Unit Pairs (023)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-023 — Teaching: IMPLEMENT+API_CONTRACT dual-predicate pattern summary

```
source_id: xiigen-b12-tu-023
rule_id: B12-RULE-IMPLEMENT-API-CONTRACT
pair_kind: teaching_unit
```

### Concept

When both IMPLEMENT and API_CONTRACT fire, the pair produces:
1. TransitionType = feature_addition
2. ImplementationFlow43 (4.3) added to FlowOrder at front
3. DecisionCount increases by 1 compared to API_CONTRACT alone

### Training Examples

A: "Build the REST API for the product catalog service and document the endpoint contract."
B: IMPLEMENT fires on "build"; API_CONTRACT fires on "REST" + "api" + "endpoint" + "contract"
   CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
   TransitionType: feature_addition; Routes: 4.3 + 3.3 + 2.1

---

A: "Create the GraphQL schema and resolver contract for the customer profile service."
B: IMPLEMENT fires on "create"; API_CONTRACT fires on "GraphQL" + "schema" + "contract"
   CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
   TransitionType: feature_addition; Routes: 4.3 + 3.3 + 2.1

---

A: "Develop the webhook delivery system and specify the event contract for published topics."
B: IMPLEMENT fires on "develop"; API_CONTRACT fires on "webhook" + "contract"
   CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
   TransitionType: feature_addition; Routes: 4.3 + 3.3 + 2.1

### Pattern recognition

All six IMPLEMENT verbs (build/implement/create/write/develop/scaffold) + any API term
→ IMPLEMENT + API_CONTRACT dual-fire → feature_addition → 4.3 in routes

### Contrast

WITHOUT IMPLEMENT: "Document the REST API endpoints"
  → [API_CONTRACT, DOCUMENT], documentation, routes: 3.3 + 2.1

WITH IMPLEMENT: "Build the REST API endpoints"
  → [IMPLEMENT, API_CONTRACT, DOCUMENT], feature_addition, routes: 4.3 + 3.3 + 2.1
