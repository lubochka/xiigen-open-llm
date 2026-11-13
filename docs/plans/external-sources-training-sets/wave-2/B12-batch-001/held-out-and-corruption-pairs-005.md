# Held-Out and Corruption Pairs (005)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-005 — Held-out: "authentication" in API docs → SECURITY fires

```
source_id: xiigen-b12-ho-005
pair_kind: held_out
hold_out_reason: "Tests that authentication fires SECURITY even in API documentation context"
```

A:
  user_request: "Document the OpenAPI specification for the checkout service, including authentication requirements for the payment endpoint."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Routing: ["3.3", "2.1", "3.4"]
  TransitionType: "documentation"
  Reason:
    "OpenAPI" → API_CONTRACT fires
    "specification" → DOCUMENT fires
    "document" → DOCUMENT fires
    "authentication" → SECURITY fires ALWAYS (Rule 6)
    "endpoint" → API_CONTRACT fires

Pass condition: CompiledPredicateIds contains "SECURITY"
The "authentication requirements for the payment endpoint" phrasing makes SECURITY appear
within an API documentation context — this is the exact v6-02 corruption scenario.

---

## B12-COR-005 — Corruption: SECURITY suppressed in API documentation context (v6-02)

```
source_id: xiigen-b12-cor-005
corruption_type: v6-02
pair_kind: corruption
```

A: same request as B12-HO-005

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← SECURITY missing
  Routing: ["3.3", "2.1"]  ← 3.4 missing
  CorruptReason: "v6-02: 'authentication requirements for the payment endpoint' classified as
  API design vocabulary (what the API requires) → SECURITY suppressed because context is
  OpenAPI documentation"

Corruption label: SECURITY SUPPRESSED IN API DOCUMENTATION CONTEXT (v6-02)
Expected correct: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
Corrupt output: ["API_CONTRACT", "DOCUMENT"]

Teaching: "authentication requirements for the payment endpoint" = authentication in API docs.
But authentication is ALWAYS a SECURITY trigger. The API documentation context does not
absorb or suppress the SECURITY trigger.
Both API_CONTRACT (from "OpenAPI", "endpoint") and SECURITY (from "authentication") fire.
