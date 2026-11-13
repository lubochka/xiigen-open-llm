# Layer 0-A — Compiler Pairs (036)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-036-001 — Multi-Version API: "define the multi-version API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-036-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-036-001"
  user_request: "Define the multi-version API contract strategy for the payments REST service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-036-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-036-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Multi-version API contract strategy for payments REST service"
  CompiledText: "Define the multi-version API contract strategy for the payments REST service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "REST" → API_CONTRACT fires on "rest"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-036-002 — Backward Compat API: "define backward compatible API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-036-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-036-002"
  user_request: "Define the backward compatible API contract for the user service upgrade from v1 to v2."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-036-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-036-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backward compatible API contract for user service v1 to v2 upgrade"
  CompiledText: "Define the backward compatible API contract for the user service upgrade from v1 to v2."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  No IMPLEMENT — "define" is a specification verb; "upgrade" is not a build verb here
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-036-003 — API Doc Generator: "define the API documentation contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-036-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-036-003"
  user_request: "Define the API documentation contract for auto-generating the SDK reference from the REST endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-036-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-036-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API documentation contract for SDK reference auto-generation from REST endpoints"
  CompiledText: "Define the API documentation contract for auto-generating the SDK reference from the REST endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "REST" → API_CONTRACT fires on "rest"
  "endpoints" → API_CONTRACT fires on "endpoint"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-036-004 — API_CONTRACT + SECURITY combo: "define the API contract with security requirements" fires three predicates

```
source_id: xiigen-b12-0a-036-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-036-004"
  user_request: "Define the API contract and security requirements for the payment gateway authentication endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-036-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-036-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API contract and security requirements for payment gateway authentication endpoints"
  CompiledText: "Define the API contract and security requirements for the payment gateway authentication endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  "endpoints" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — all verbs are specification/documentation
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

Teaching note: "authentication" fires SECURITY even in a contract definition context. The
API_CONTRACT + SECURITY + DOCUMENT triple fires when specifying API contracts with auth requirements.
No IMPLEMENT fires unless a construction verb (build, implement, create, write, develop, scaffold) is present.
