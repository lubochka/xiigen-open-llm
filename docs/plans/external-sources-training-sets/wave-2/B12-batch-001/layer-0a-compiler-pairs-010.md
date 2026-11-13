# Layer 0-A — Compiler Pairs (010)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-010-001 — Full five-predicate: IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY+AUTHORIZATION

```
source_id: xiigen-b12-0a-010-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-010-001"
  user_request: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0 token endpoint, and document the security requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-010-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-010-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OpenAPI specification and OAuth 2.0 token endpoint implementation with security documentation for authentication service"
  CompiledText: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0 token endpoint, and document the security requirements."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write" (code verb creating a new spec)
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification"
  "authentication" → SECURITY fires on "authentication" ALWAYS (Core Rule 6)
  "implement" → IMPLEMENT fires on "implement"
  "OAuth 2.0" → AUTHORIZATION fires on "oauth" (Core Rule 7: oauth → AUTHORIZATION → 3.4)
               → SECURITY fires again on "oauth" (token context)
  "token" → SECURITY fires on "token" (Core Rule 8)
  "endpoint" → API_CONTRACT fires on "endpoint"
  "document" → DOCUMENT fires on "document"
  "security" — the word itself does not trigger SECURITY (the trigger is "authentication"/"token")
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  Confidence: 0.91
  TransitionType: feature_addition (IMPLEMENT fires)

Teaching note: "OAuth" fires BOTH SECURITY and AUTHORIZATION simultaneously. SECURITY routes to 3.4.
AUTHORIZATION also routes to 3.4. This means two separate DecisionCount entries to 3.4 (Core Rule 23:
DecisionCount = number of predicates fired, not unique sub-phases). Five predicates → five decisions
even though SECURITY and AUTHORIZATION both route to 3.4.

---

## B12-0A-010-002 — OAuth only: AUTHORIZATION fires + SECURITY fires together

```
source_id: xiigen-b12-0a-010-002
transition_type: documentation
phase: 3
sub_phase: 3.4
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-010-002"
  user_request: "Document the OAuth 2.0 flow for the payment service API and specify the authorization scopes."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-010-002-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  Predicate evidence:
    "document" → DOCUMENT fires on "document"
    "OAuth 2.0" → AUTHORIZATION fires on "oauth" + SECURITY fires
    "API" → API_CONTRACT fires on "api"
    "specify" → DOCUMENT fires on "specify"
    "authorization" → AUTHORIZATION fires on "authorization"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"], Confidence: 0.92

---

## B12-0A-010-003 — token endpoint: SECURITY fires on "token" in API context

```
source_id: xiigen-b12-0a-010-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-010-003"
  user_request: "Define the API endpoint for token refresh and describe the token rotation policy."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-010-003-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Predicate evidence:
    "define" → DOCUMENT fires on "define"
    "API" → API_CONTRACT fires on "api"
    "endpoint" → API_CONTRACT fires on "endpoint"
    "token" → SECURITY fires on "token" (Core Rule 8: token → SECURITY)
    "describe" → DOCUMENT fires on "describe"
  No AUTHORIZATION — no "oauth", "authorization", "access control" terms
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

---

## B12-0A-010-004 — five predicate count breakdown and DecisionCount

```
source_id: xiigen-b12-0a-010-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

DecisionCount analysis for request: "Write the OpenAPI specification for the authentication service,
implement the OAuth 2.0 token endpoint, and document the security requirements."

Predicates fired (in order of detection):
  1. IMPLEMENT   → fires on "write", "implement"       → routes to Phase 4.3
  2. API_CONTRACT → fires on "openapi", "endpoint"      → routes to Phase 3.3
  3. DOCUMENT    → fires on "specification", "document" → routes to Phase 2.1
  4. SECURITY    → fires on "authentication", "token", "oauth" → routes to Phase 3.4
  5. AUTHORIZATION → fires on "oauth"                  → routes to Phase 3.4

DecisionCount = 5 (five predicates = five decisions)

Note on SECURITY + AUTHORIZATION both routing to 3.4:
  SECURITY decision #4 → 3.4 (security audit sub-phase)
  AUTHORIZATION decision #5 → 3.4 (same sub-phase, separate decision)
  Total decisions to 3.4 = 2 (both SECURITY and AUTHORIZATION count separately)
  Core Rule 23: DecisionCount is count of predicates, not unique sub-phases

Routing result:
  Phase 4.3 ← IMPLEMENT
  Phase 3.3 ← API_CONTRACT
  Phase 2.1 ← DOCUMENT
  Phase 3.4 ← SECURITY
  Phase 3.4 ← AUTHORIZATION
  Total routing decisions: 5 (to 4 unique sub-phases)
