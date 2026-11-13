# Layer 0-A — Compiler Pairs (042)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-042-001 — APIKeyManagement: "define the API key management contract" fires API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-042-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-042-001"
  user_request: "Define the API key management contract including authentication, key rotation, and revocation policies."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-042-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-042-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API key management contract with authentication, rotation, and revocation policies"
  CompiledText: "Define the API key management contract including authentication, key rotation, and revocation policies."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — "define" is a specification verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.94

---

## B12-0A-042-002 — APIKeyManagement: "implement the API key generation endpoint" fires IMPLEMENT + API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-042-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-042-002"
  user_request: "Implement the API key generation endpoint with authentication checks and write the key issuance contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-042-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-042-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API key generation endpoint implementation with authentication checks and key issuance contract"
  CompiledText: "Implement the API key generation endpoint with authentication checks and write the key issuance contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "write" → IMPLEMENT fires on "write" (code verb)
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], Confidence: 0.94

---

## B12-0A-042-003 — APIKeyManagement: "expose the API key in the partner portal" fires API_CONTRACT

```
source_id: xiigen-b12-0a-042-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-042-003"
  user_request: "Expose the API key management endpoints in the partner portal dashboard."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-042-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-042-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API key management endpoints exposure in partner portal dashboard"
  CompiledText: "Expose the API key management endpoints in the partner portal dashboard."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose" (publication verb)
  "API" → API_CONTRACT fires on "api"
  "endpoints" → API_CONTRACT fires on "endpoint"
  No SECURITY — "API key" alone without "authentication" does not fire SECURITY independently
  No DOCUMENT — no documentation verb present
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-042-004 — APIKeyManagement: "connect API key authentication to all service endpoints" fires API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-042-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-042-004"
  user_request: "Connect the API key authentication layer to all service REST endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-042-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-042-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API key authentication layer connection to all service REST endpoints"
  CompiledText: "Connect the API key authentication layer to all service REST endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "connect" → API_CONTRACT fires on "connect" (integration verb)
  "API" → API_CONTRACT fires on "api"
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  "REST" → API_CONTRACT fires on "rest"
  "endpoints" → API_CONTRACT fires on "endpoint"
  No IMPLEMENT — "connect" is a wiring/integration verb, not a construction verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "SECURITY"], Confidence: 0.93
