# Layer 0-A — Compiler Pairs (044)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-044-001 — TenantScopedAPI: "define the tenant-scoped API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-044-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-044-001"
  user_request: "Define the tenant-scoped API contract for the multi-tenant SaaS (Software as a Service) platform."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-044-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-044-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Tenant-scoped API contract for multi-tenant SaaS platform"
  CompiledText: "Define the tenant-scoped API contract for the multi-tenant SaaS platform."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "tenant-scoped" is a domain qualifier (B12-R25) — does NOT independently fire any predicate
  "multi-tenant" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-044-002 — TenantScopedAPI: "implement tenant isolation in the API endpoints" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-044-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-044-002"
  user_request: "Implement tenant isolation in the REST API endpoints using the X-Tenant-ID header."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-044-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-044-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Tenant isolation implementation in REST API endpoints using X-Tenant-ID header"
  CompiledText: "Implement tenant isolation in the REST API endpoints using the X-Tenant-ID header."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-044-003 — TenantScopedAPI: "expose the tenant API contract" fires API_CONTRACT

```
source_id: xiigen-b12-0a-044-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-044-003"
  user_request: "Expose the tenant-scoped API contract to external partners via the developer portal."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-044-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-044-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Tenant-scoped API contract exposure to external partners via developer portal"
  CompiledText: "Expose the tenant-scoped API contract to external partners via the developer portal."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose" (publication verb)
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  No DOCUMENT — no documentation verb present
  No IMPLEMENT — "expose" is a release verb, not a construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.92

---

## B12-0A-044-004 — TenantScopedAPI: "define tenant API with authentication" fires API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-044-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-044-004"
  user_request: "Define the tenant-scoped API contract with authentication and tenant-level authorization policies."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-044-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-044-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Tenant-scoped API contract with authentication and tenant-level authorization policies"
  CompiledText: "Define the tenant-scoped API contract with authentication and tenant-level authorization policies."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — "define" is a specification verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93
