# Layer 3 — Teaching Unit Pairs (018)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-018 — Teaching: "idempotent" fires API_CONTRACT — it is an API contract term

```
source_id: xiigen-b12-tu-018
rule_id: B12-RULE-IDEMPOTENT
pair_kind: teaching_unit
```

### Concept

"Idempotent" is in the API_CONTRACT lexicon. Idempotency is a formal API contract guarantee
that specifies how the API behaves on repeated identical requests. It appears in API contracts
to document the safety of re-sending requests.

### Training Examples

A: "Define the idempotent upload contract for the file storage API."
B: API_CONTRACT fires on "idempotent" + "contract" + "api"; DOCUMENT fires on "define"
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

---

A: "Specify the idempotent delivery contract for the order webhook endpoint."
B: API_CONTRACT fires on "idempotent" + "contract" + "webhook" + "endpoint"
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

---

A: "Document the idempotent API contract for rate quote requests."
B: API_CONTRACT fires on "idempotent" + "api" + "contract"; DOCUMENT fires on "document"
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

### Why idempotency is an API contract concern

An idempotent endpoint guarantees that calling it multiple times with the same parameters
produces the same result as calling it once. This is a contractual promise from the API
to its consumers. Documenting it = API_CONTRACT scope.

Examples of idempotent endpoints:
  - PUT /resources/{id} — updating a resource (same payload = same state)
  - DELETE /resources/{id} — deleting (already deleted = same result: deleted)
  - POST /orders with Idempotency-Key header — prevent duplicate orders

### Memory aid

"idempotent" in a request involving an API = API contract term → API_CONTRACT fires
