# Layer 0-C — Knowledge Edge Pairs (016)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-016-001 — "swagger" and "openapi" both fire API_CONTRACT (ShippingRatesAPI)

```
source_id: xiigen-b12-0c-016-001
rule_id: B12-RULE-SWAGGER-OPENAPI
pair_kind: knowledge_edge
```

A:
  CompiledText: "Publish the OpenAPI (Swagger) specification for the shipping rates API."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "OpenAPI" → API_CONTRACT fires on "openapi"
    "Swagger" → API_CONTRACT fires on "swagger"
    Both are in API_CONTRACT lexicon; both trigger API_CONTRACT
    Result: still ONE API_CONTRACT instance (not two)
    "specification" → DOCUMENT fires
    "API" → API_CONTRACT fires
    "publish" → NOT an IMPLEMENT verb (release/distribution verb)
  Result: ["API_CONTRACT", "DOCUMENT"], TransitionType: documentation
  Teaching: "publish" is not IMPLEMENT. Publishing a spec = documentation, not feature_addition.

---

## B12-0C-016-002 — "idempotent" in shipping context (rate quote idempotency)

```
source_id: xiigen-b12-0c-016-002
rule_id: B12-RULE-IDEMPOTENT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Document the idempotent API contract for rate quote requests to prevent duplicate charges."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "document" → DOCUMENT fires
    "idempotent" → API_CONTRACT fires (API contract term)
    "API" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
    "requests" → API_CONTRACT fires (in API context)
  Result: ["API_CONTRACT", "DOCUMENT"]
  Teaching: Idempotency is a fundamental API contract guarantee.
  "idempotent" signals that the caller and API have agreed on repeated-call behavior.
  This is always an API_CONTRACT concern, regardless of domain (shipping, payments, etc.).

---

## B12-0C-016-003 — "carrier integration" does NOT fire API_CONTRACT standalone

```
source_id: xiigen-b12-0c-016-003
rule_id: B12-RULE-INTEGRATION-CO-OCCURRENCE
pair_kind: knowledge_edge
```

A:
  Case 1: "Specify the API contract for carrier integrations."
  Case 2: "Specify the carrier integration requirements."

B:
  Case 1: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    "api" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
    "integration" — fires API_CONTRACT only WITH api co-occurrence (api IS present)
    Result: ["API_CONTRACT", "DOCUMENT"]

  Case 2: CompiledPredicateIds: ["DOCUMENT"]
    "specify" → DOCUMENT fires
    "integration" — no API co-occurrence present → API_CONTRACT does NOT fire
    "requirements" → DOCUMENT fires
    Result: ["DOCUMENT"] only
  
  Teaching: "integration" does NOT fire API_CONTRACT alone. It fires API_CONTRACT ONLY
  when "api" (or other API co-occurrence) is also present in the request.
  Core Rule 17: "integrate" → API_CONTRACT requires API co-occurrence.
