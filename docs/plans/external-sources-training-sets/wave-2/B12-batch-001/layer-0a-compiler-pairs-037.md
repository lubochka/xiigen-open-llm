# Layer 0-A — Compiler Pairs (037)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-037-001 — RateLimitingAPI: "define the rate limiting API contract" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0a-037-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-037-001"
  user_request: "Define the rate limiting API contract for the payments service public endpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-037-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-037-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Rate limiting API contract for payments service public endpoints"
  CompiledText: "Define the rate limiting API contract for the payments service public endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (Core Rule 2: unconditional)
  "endpoints" → API_CONTRACT fires on "endpoint"
  "rate limiting" is a domain qualifier — does NOT independently fire any predicate
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-037-002 — RateLimitingAPI: "implement the rate limiting middleware" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0a-037-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-037-002"
  user_request: "Implement the rate limiting middleware for the REST API gateway and write the throttle contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-037-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-037-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Rate limiting middleware implementation with throttle contract for REST API gateway"
  CompiledText: "Implement the rate limiting middleware for the REST API gateway and write the throttle contract."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "write" → IMPLEMENT fires on "write" (code creation verb)
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], Confidence: 0.93

---

## B12-0A-037-003 — RateLimitingAPI: "expose the rate limit headers in the API" fires API_CONTRACT

```
source_id: xiigen-b12-0a-037-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-037-003"
  user_request: "Expose the rate limit response headers (X-RateLimit-Limit, X-RateLimit-Remaining) in the API contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-037-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-037-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Rate limit response headers exposure in API contract"
  CompiledText: "Expose the rate limit response headers (X-RateLimit-Limit, X-RateLimit-Remaining) in the API contract."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "expose" → API_CONTRACT fires on "expose" (publish/expose endpoint verb)
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  No DOCUMENT — no documentation verb present
  No IMPLEMENT — "expose" is a release verb, not a code-creation verb
  Result: CompiledPredicateIds: ["API_CONTRACT"], Confidence: 0.91

---

## B12-0A-037-004 — RateLimitingAPI: "define rate limiting API with security" fires API_CONTRACT + DOCUMENT + SECURITY

```
source_id: xiigen-b12-0a-037-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-037-004"
  user_request: "Define the rate limiting API contract with authentication bypass rules for trusted service accounts."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-037-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-037-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Rate limiting API contract with authentication bypass for trusted service accounts"
  CompiledText: "Define the rate limiting API contract with authentication bypass rules for trusted service accounts."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  No IMPLEMENT — all verbs are specification verbs
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.93

Teaching note: "authentication" fires SECURITY even when the surrounding context is about bypassing
or exempting authentication. Core Rule 6 is unconditional: presence of "authentication" in the
request text always produces SECURITY predicate.
