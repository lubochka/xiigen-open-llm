# Layer 0-A — Compiler Pairs (035)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-035-001 — API Key Management: "define the API key contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-035-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-035-001"
  user_request: "Define the API key management contract for the developer portal."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-035-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-035-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API key management contract definition for developer portal"
  CompiledText: "Define the API key management contract for the developer portal."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-035-002 — API Key Management: "implement API key authentication" fires IMPLEMENT + API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-035-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-035-002"
  user_request: "Implement API key authentication for the partner integration endpoints and write the key validation schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-035-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-035-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API key authentication implementation for partner integration endpoints with key validation schema"
  CompiledText: "Implement API key authentication for the partner integration endpoints and write the key validation schema."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "write" → IMPLEMENT fires on "write" (code verb)
  "API" → API_CONTRACT fires on "api"
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  "endpoints" → API_CONTRACT fires on "endpoint"
  "schema" → API_CONTRACT fires on "schema" (with "api/endpoints" co-occurrence)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], Confidence: 0.93

---

## B12-0A-035-003 — OAuth Scope API: "define the OAuth scope contract" fires API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-035-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-035-003"
  user_request: "Define the OAuth scope contract for the customer data API and document the authentication requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-035-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-035-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OAuth scope contract definition and authentication requirements for customer data API"
  CompiledText: "Define the OAuth scope contract for the customer data API and document the authentication requirements."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "OAuth" → SECURITY fires on "oauth" (authentication protocol trigger)
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "API" → API_CONTRACT fires on "api"
  "document" → DOCUMENT fires on "document"
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — all verbs are documentation/specification
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

---

## B12-0A-035-004 — Tenant-Scoped API: "define the tenant-scoped API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-035-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-035-004"
  user_request: "Define the tenant-scoped API contract for the multi-tenant SaaS (Software as a Service) platform."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-035-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-035-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Tenant-scoped API contract definition for multi-tenant SaaS platform"
  CompiledText: "Define the tenant-scoped API contract for the multi-tenant SaaS platform."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93
