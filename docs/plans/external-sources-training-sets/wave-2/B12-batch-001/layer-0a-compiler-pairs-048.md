# Layer 0-A — Compiler Pairs (048)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler

---

## B12-0A-048-001 — API_CONTRACT+SECURITY combo: "define secure API key and OAuth contract" fires three predicates

```
source_id: xiigen-b12-0a-048-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-048-001"
  user_request: "Define the API contract for the payment service including API key authentication and OAuth2 token validation."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-048-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-048-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment service API contract with API key authentication and OAuth2 token validation"
  CompiledText: "Define the API contract for the payment service including API key authentication and OAuth2 token validation."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "API" → API_CONTRACT fires on "api"
  "contract" → API_CONTRACT fires on "contract" (unconditional)
  "authentication" → SECURITY fires on "authentication" (Core Rule 6: unconditional)
  "OAuth2" → SECURITY fires on "oauth2" (SECURITY multiplicity: two triggers = ONE SECURITY predicate)
  No IMPLEMENT — "define" is a specification verb
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"], Confidence: 0.94

Teaching note: "authentication" and "OAuth2" both fire SECURITY, but SECURITY multiplicity rule
(B12-R27) ensures only one SECURITY entry in CompiledPredicateIds. DecisionCount for this
combination is 3 (not 4).

---

## B12-0A-048-002 — API_CONTRACT+SECURITY combo: "implement secure API with OAuth" fires IMPLEMENT + API_CONTRACT + SECURITY

```
source_id: xiigen-b12-0a-048-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-048-002"
  user_request: "Implement the secure REST API endpoint with OAuth2 bearer token authentication for the admin panel."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-048-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-048-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Secure REST API endpoint implementation with OAuth2 bearer token authentication for admin panel"
  CompiledText: "Implement the secure REST API endpoint with OAuth2 bearer token authentication for the admin panel."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" → IMPLEMENT fires on "implement"
  "REST" → API_CONTRACT fires on "rest"
  "API" → API_CONTRACT fires on "api"
  "endpoint" → API_CONTRACT fires on "endpoint"
  "OAuth2" → SECURITY fires on "oauth2"
  "authentication" → SECURITY fires on "authentication" (SECURITY multiplicity: one predicate)
  TransitionType: feature_addition (IMPLEMENT present)
  Result: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], Confidence: 0.94

---

## B12-0A-048-003 — Round 3 boundary summary: topics 037–048

```
source_id: xiigen-b12-0a-048-003
pair_kind: compiler_reference
```

### Topics exercised in compiler files 037–048

| File | Primary topic | Key pattern |
|------|--------------|-------------|
| 037 | RateLimitingAPI | domain qualifier; integration verb; IMPLEMENT when "write" present |
| 038 | CachingAPI | cache-control = domain qualifier; ETag contract = API_CONTRACT |
| 039 | IdempotencyAPI | idempotency fires API_CONTRACT; expose = not IMPLEMENT |
| 040 | VersionedEndpoint | versioned = qualifier; build = IMPLEMENT; wire up = not IMPLEMENT |
| 041 | DeprecatedEndpoint | deprecated = lifecycle label, not IMPLEMENT; upgrade != build |
| 042 | APIKeyManagement | API key auth fires SECURITY; expose key = API_CONTRACT only |
| 043 | OAuthScopeAPI | OAuth fires SECURITY; connect = integration verb; specify = DOCUMENT |
| 044 | TenantScopedAPI | tenant-scoped = domain qualifier; authentication fires SECURITY |
| 045 | MultiVersionAPI | routing fires API_CONTRACT; build fires IMPLEMENT |
| 046 | BackwardCompatAPI | backward compatible = qualifier; upgrade != construction |
| 047 | APIDocGenerator | auto-generate not IMPLEMENT unless build/write verb present |
| 048 | API_CONTRACT+SECURITY combo | authentication+OAuth2 = ONE SECURITY; SECURITY multiplicity rule |

---

## B12-0A-048-004 — Round 3 vocabulary additions: new API_CONTRACT triggers confirmed

```
source_id: xiigen-b12-0a-048-004
pair_kind: compiler_reference
```

New triggers confirmed in Round 3 (files 037–048):

| Term | Fires | Rule |
|------|-------|------|
| idempotency / idempotent | API_CONTRACT | B12-R18 extended |
| rate limit (with API co-occurrence) | API_CONTRACT | domain qualifier rule |
| cache-control (with API co-occurrence) | API_CONTRACT | domain qualifier rule |
| tenant-scoped (with API co-occurrence) | API_CONTRACT qualifier | B12-R25 |
| multi-version (with API co-occurrence) | API_CONTRACT qualifier | B12-R25 |
| deprecated (with endpoint co-occurrence) | API_CONTRACT via endpoint | lifecycle label |
| backward compatible (with API co-occurrence) | API_CONTRACT qualifier | B12-R25 |

SECURITY multiplicity (B12-R27): authentication + OAuth2 → one SECURITY predicate, not two.
DecisionCount is always COUNT(CompiledPredicateIds), not COUNT(trigger terms).
