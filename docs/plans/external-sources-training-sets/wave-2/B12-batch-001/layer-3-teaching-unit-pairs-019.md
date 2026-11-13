# Layer 3 — Teaching Unit Pairs (019)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-019 — Teaching: "integration" does NOT fire API_CONTRACT alone

```
source_id: xiigen-b12-tu-019
rule_id: B12-RULE-INTEGRATION-CO-OCCURRENCE
pair_kind: teaching_unit
```

### Concept

"Integration" fires API_CONTRACT ONLY when an API co-occurrence term is also present.
Without API co-occurrence, "integration" contributes no predicate.
This is Core Rule 17: "integrate" requires co-occurrence.

### Training Examples

A: "Define the partner API contract for third-party integration."
B: API_CONTRACT fires on "api" + "contract"; "integration" also fires API_CONTRACT (api co-occurrence satisfied)
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

---

A: "Define the partner integration onboarding process."
B: "integration" → no API co-occurrence → API_CONTRACT does NOT fire
   CompiledPredicateIds: ["DOCUMENT"]
   Reason: no api/rest/graphql/endpoint/contract/webhook present

---

A: "Specify the carrier integration requirements."
B: "integration" → no API co-occurrence → no API_CONTRACT
   CompiledPredicateIds: ["DOCUMENT"]

---

A: "Specify the API contract for carrier integrations."
B: "api" + "contract" provide API_CONTRACT triggers; "integrations" also fires (api co-occurrence)
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

### Rule

"integration"/"integrate" fires API_CONTRACT ONLY WITH: api, rest, graphql, openapi, swagger, grpc, webhook, endpoint, contract co-occurrence in same request.
Without these: "integration" fires no predicate.

### Memory aid

Integration is a general word. API integration is an API_CONTRACT word.
"integration" alone = general software concern → no predicate
"API integration" / "integration contract" / "REST integration" = API boundary concern → API_CONTRACT
