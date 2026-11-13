# Layer 0-A — Compiler Pairs (031)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-031-001 — Filterable Search API: "define the filterable search API" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-031-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-031-001"
  user_request: "Define the filterable search API contract for the product discovery service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-031-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-031-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Filterable search API contract definition for product discovery service"
  CompiledText: "Define the filterable search API contract for the product discovery service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-031-002 — Filterable Search API: "implement the search endpoint with filter parameters" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-031-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-031-002"
  user_request: "Implement the search endpoint with filter parameters and write the request schema for the products API."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-031-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-031-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Search endpoint with filter parameters implementation and request schema for products API"
  CompiledText: "Implement the search endpoint with filter parameters and write the request schema for the products API."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "write" → IMPLEMENT fires on "write" (code verb)
  "endpoint" → API_CONTRACT fires on "endpoint"
  "schema" → API_CONTRACT fires on "schema" (with "endpoint", "api" co-occurrence)
  "request" → API_CONTRACT fires on "request" (in API context)
  "API" → API_CONTRACT fires on "api"
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-031-003 — Audit Trail API: "define the audit trail API" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-031-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-031-003"
  user_request: "Define the audit trail API contract for the compliance service and specify the event log schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-031-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-031-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Audit trail API contract and event log schema for compliance service"
  CompiledText: "Define the audit trail API contract for the compliance service and specify the event log schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "specify" → DOCUMENT fires on "specify"
  "schema" → API_CONTRACT fires on "schema" (with "api" co-occurrence)
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.93

---

## B12-0A-031-004 — Audit Trail API: "expose the audit API with authentication" fires API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-031-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-031-004"
  user_request: "Expose the audit trail API endpoint with authentication requirements for the compliance team."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-031-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-031-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Audit trail API endpoint exposure with authentication for compliance team"
  CompiledText: "Expose the audit trail API endpoint with authentication requirements for the compliance team."
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose API" (publish/expose trigger)
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: ALWAYS unconditional)
  No IMPLEMENT — "expose" is a release verb
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT", "SECURITY"], Confidence: 0.93

Teaching note: "authentication" fires SECURITY even in an API exposure context. Core Rule 6 is
unconditional — no context suppresses SECURITY when "authentication" is present.
