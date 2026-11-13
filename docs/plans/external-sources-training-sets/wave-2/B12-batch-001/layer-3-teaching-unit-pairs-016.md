# Layer 3 — Teaching Unit Pairs (016)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-016 — Teaching: "spec" fires DOCUMENT not API_CONTRACT

```
source_id: xiigen-b12-tu-016
rule_id: B12-RULE-SPEC-DOCUMENT
pair_kind: teaching_unit
```

### Concept

"Spec" (short for "specification") fires DOCUMENT, not API_CONTRACT.
API_CONTRACT fires via api/openapi/swagger/rest/graphql/grpc/webhook/contract terms.
"Spec" contributes to DOCUMENT alongside definition verbs.

### Training Examples

A: "Define the OpenAPI spec for the reporting API."
B: API_CONTRACT fires via "OpenAPI" + "api"; DOCUMENT fires via "define" + "spec"
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   — "spec" contributes DOCUMENT, not the API_CONTRACT

---

A: "Specify the functional spec for the reporting dashboard."
B: DOCUMENT fires via "specify" + "spec"; no API_CONTRACT (no API terms)
   CompiledPredicateIds: ["DOCUMENT"]
   — "spec" without API co-occurrence → DOCUMENT only

---

A: "Write the spec for the GraphQL product search API."
B: IMPLEMENT fires via "write"; API_CONTRACT fires via "GraphQL" + "api"; DOCUMENT fires via "spec"
   CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
   — "spec" → DOCUMENT; "write" → IMPLEMENT; "GraphQL" → API_CONTRACT

### Memory aid

"spec" = "specification" = documentation artifact → DOCUMENT
API_CONTRACT comes from api/graphql/rest/openapi/swagger/etc., NOT from "spec"
