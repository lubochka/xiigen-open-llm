# Layer 3 — Teaching Unit Pairs (005)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-005 — Teaching: "authentication" → SECURITY always in any context

```
source_id: xiigen-b12-tu-005
rule_id: B12-RULE-AUTHENTICATION-ALWAYS-SECURITY
pair_kind: teaching_unit
```

### Concept

"authentication" fires SECURITY in ANY context, unconditionally.
No surrounding vocabulary suppresses SECURITY when "authentication" is present.
This is an absolute rule with no exceptions.

### Why This Rule Exists

Authentication is fundamentally a security concern regardless of what system it appears in:
  - Authentication in an API spec → still a security concern for that API
  - Authentication header requirements → security configuration
  - Authentication flow documentation → security process documentation

The fact that "authentication" appears in an API documentation request doesn't reclassify it
as API vocabulary. The API documentation is ABOUT authentication — and authentication is a
security topic. Both predicates fire simultaneously.

### Training Examples

Example 1: authentication in API documentation
  A: "Document the REST API endpoints and include authentication requirements."
  B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
     Reason: "REST API" → API_CONTRACT; "document" → DOCUMENT; "authentication" → SECURITY (always)

Example 2: authentication in OpenAPI spec
  A: "Write the OpenAPI spec for the cart service and include authentication requirements."
  B: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]
     Reason: "write" → IMPLEMENT; "OpenAPI" → API_CONTRACT; "spec" → DOCUMENT; "authentication" → SECURITY

Example 3: authentication in webhook contract
  A: "Specify the webhook contract for shipping notifications including authentication header requirements."
  B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
     Reason: "webhook", "contract" → API_CONTRACT; "specify" → DOCUMENT; "authentication" → SECURITY

Example 4: authentication in gRPC design
  A: "Design the gRPC service interface and document the authentication flow."
  B: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"]
     Reason: "design" → ARCHITECTURE; "gRPC", "interface" → API_CONTRACT; "document" → DOCUMENT; "authentication" → SECURITY

### The v6-02 Error to Avoid

WRONG: "authentication is in an API doc context → it's API vocabulary → suppress SECURITY"
CORRECT: "authentication → SECURITY fires, full stop, regardless of context"

Example of v6-02 corruption:
  A: "Write the OpenAPI spec with authentication requirements."
  CORRUPT: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]  (SECURITY missing)
  CORRECT: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]

### "authentication" vs "authorization" distinction

  "authentication" → SECURITY → routes to 3.4 (verify identity)
  "authorization" → AUTHORIZATION → routes to 3.4 (verify permissions)
  "oauth" → BOTH SECURITY + AUTHORIZATION → two decisions to 3.4

Both can appear in the same request (→ SECURITY + AUTHORIZATION both fire).

### Memory Aid

Whenever "authentication" appears: add SECURITY to CompiledPredicateIds.
Always. No exceptions. Even in:
  - API documentation requests
  - Architecture design requests
  - Spec writing requests
  - Contract definition requests
