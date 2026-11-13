# Layer 0-A — Compiler Pairs (043)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-043-001 — OAuthScopeAPI: "define the OAuth scope API contract" fires API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-043-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-043-001"
  user_request: "Define the OAuth scope API contract for the customer data access endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-043-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-043-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OAuth scope API contract for customer data access endpoints"
  CompiledText: "Define the OAuth scope API contract for the customer data access endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "endpoints" → API_CONTRACT fires on "endpoint"
  "OAuth" → SECURITY fires on "oauth" (authentication protocol trigger)
  No IMPLEMENT — "define" is a specification verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.94

---

## B12-0A-043-002 — OAuthScopeAPI: "implement OAuth scope validation" fires IMPLEMENT + API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-043-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-043-002"
  user_request: "Implement the OAuth scope validation middleware for the partner REST API endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-043-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-043-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OAuth scope validation middleware implementation for partner REST API endpoints"
  CompiledText: "Implement the OAuth scope validation middleware for the partner REST API endpoints."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "OAuth" → SECURITY fires on "oauth"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], Confidence: 0.94

---

## B12-0A-043-003 — OAuthScopeAPI: "connect OAuth scopes to the API gateway" fires API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-043-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-043-003"
  user_request: "Connect the OAuth scope enforcement to the API gateway for all protected endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-043-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-043-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OAuth scope enforcement connection to API gateway for protected endpoints"
  CompiledText: "Connect the OAuth scope enforcement to the API gateway for all protected endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "connect" → API_CONTRACT fires on "connect" (integration verb)
  "OAuth" → SECURITY fires on "oauth"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — "connect" is an integration verb, not a construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "SECURITY"], Confidence: 0.93

---

## B12-0A-043-004 — OAuthScopeAPI: "specify OAuth token scope requirements in the OpenAPI spec" fires API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-043-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-043-004"
  user_request: "Specify the OAuth token scope requirements for each endpoint in the OpenAPI specification."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-043-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-043-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OAuth token scope requirements specification per endpoint in OpenAPI specification"
  CompiledText: "Specify the OAuth token scope requirements for each endpoint in the OpenAPI specification."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "OAuth" → SECURITY fires on "oauth"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "OpenAPI" → API_CONTRACT fires on "openapi"
  "specification" → DOCUMENT fires on "specification" (spec → DOCUMENT)
  No IMPLEMENT — "specify" is a documentation verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.94
