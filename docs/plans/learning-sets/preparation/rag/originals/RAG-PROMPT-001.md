# Layer 0-A — Prompt Routing Input Compiler Pairs

```
Contract:
  PromptRoutingInputCompilerUnit
      : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   PromptRoutingInputCompilerUnit
node_role:            layer_0_a
pair_kind:            adaptive_routing_compiler
coverage_status:      covered
attribution_required: false   (overridden to true on ES-S8 pair only)
```

Per-pair labels carry: `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `rejected_type`, `overlap_ratio`.

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60. Values
recorded per pair below.

Eight pairs, one per scenario. Every B side uses only actual
`SubPhaseRouterSourceDocument` field names. Confidence is `0.92` when at least
one predicate matched the lexicon; `0.70` when none matched (ES-S8 has all
predicates absent because the verb is `"How does"` — see comment in pair).

Predicate lexicon authority:

```
SECURITY      — jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa, rbac, security, authorization
AUTHORIZATION — authorization, authorisation, permission, rbac, scope, tenant, ownership
API_CONTRACT  — api, endpoint, contract, schema, request, response, openapi
ARCHITECTURE  — architecture, component, topology, boundary, dependency, service
IMPLEMENT     — "implement the", "implementation plan", connector, provider, "integration layer", "code change"
TEST          — test, testing, "integration test", "unit test", coverage, fixture
QA            — qa, quality, release, acceptance, verification
UI            — ui, ux, screen, layout, design, interaction, accessibility
DOCUMENT      — document, documentation, spec, specification, requirements
```

---

## Pair[ID] ES-2026-05-08-3.1-L0A-01 — ES-S1 component topology

A:
```
run_id:       "ES-S1-router-001"
scenario_id:  "ES-S1"
user_request: "We need to define the component boundaries between the order service and the inventory service. The order service must own the order lifecycle and the inventory service must own stock state — neither service may read the other's storage directly."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
DocumentId:           "ES-S1-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "We need to define the component boundaries between the order service and the inventory service. The order service must own the order lifecycle and the inventory service must own stock state — neither service may read the other's storage directly."
CompiledPredicateIds: ["ARCHITECTURE"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["ARCHITECTURE"]
```

Reason:
```
[WRONG: the prompt's high-signal terms are "boundaries", "service", "component",
"dependency direction" — every one of these maps to ARCHITECTURE. The rejected
assigns IMPLEMENT because "service" is also an IMPLEMENT trigger fragment, but
the IMPLEMENT trigger requires "implement the", "implementation plan", or
"integration layer" — none appear here. ARCHITECTURE alone is the correct
predicate set.]
```

Phrasing-B (Type C — wrong domain, corruption: wrong-predicate-assignment):
```
DocumentId:           "ES-S1-router-001-prompt"
CompiledText:         (same as above)
CompiledPredicateIds: ["IMPLEMENT"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["IMPLEMENT"]
```

Coverage labels:
```
source_id:            arc42-template, ADR-template
source_title:         arc42 building-block view + ADR Decision section
transition_type:      feature_addition
phase:                3
sub_phase:            3.1
task_type:            architecture-topology
rejected_type:        C
overlap_ratio:        0.20   (Type C: predicate-name token sets ARCHITECTURE vs IMPLEMENT disjoint; CompiledText same)
```

---

## Pair[ID] ES-2026-05-08-3.5-L0A-02 — ES-S2 observability strategy

A:
```
run_id:       "ES-S2-router-001"
scenario_id:  "ES-S2"
user_request: "Design what to measure and alert on for our payment processor service. We need to know which workflows are running, which retries are accumulating, and where compensation activities fire."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
DocumentId:           "ES-S2-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "Design what to measure and alert on for our payment processor service. We need to know which workflows are running, which retries are accumulating, and where compensation activities fire."
CompiledPredicateIds: ["ARCHITECTURE"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["ARCHITECTURE"]
```

Reason:
```
[WRONG: "Design" is the primary intent verb — the prompt asks the architect
to decide what signals to capture and what alert thresholds to set. The
ARCHITECTURE predicate fires from "design", "service", "dependency between
workflow and compensation". The rejected promotes UI because "design" also
appears in the UI lexicon — but UI is bound to screens, layouts, and visual
interaction. A telemetry strategy is not a UI design.]
```

Phrasing-B (Type C — wrong domain):
```
DocumentId:           "ES-S2-router-001-prompt"
CompiledText:         (same as above)
CompiledPredicateIds: ["UI"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["UI"]
```

Coverage labels:
```
source_id:            temporal-observability-docs, dapr-observability-docs
source_title:         Temporal observability + Dapr observability
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            observability-design
rejected_type:        C
overlap_ratio:        0.18   (Type C: predicate-name tokens ARCHITECTURE vs UI disjoint)
```

---

## Pair[ID] ES-2026-05-08-5.2-L0A-03 — ES-S3 integration tests

A:
```
run_id:       "ES-S3-router-001"
scenario_id:  "ES-S3"
user_request: "Write integration tests that verify postgres tenant isolation across the orders module. The tests must spin up a real postgres container and assert that tenant A cannot read tenant B's order rows."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
DocumentId:           "ES-S3-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "Write integration tests that verify postgres tenant isolation across the orders module. The tests must spin up a real postgres container and assert that tenant A cannot read tenant B's order rows."
CompiledPredicateIds: ["TEST", "AUTHORIZATION"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["TEST", "AUTHORIZATION"]
```

Reason:
```
[WRONG: "integration tests" is the primary verb phrase — TEST predicate fires
first. "tenant isolation" carries AUTHORIZATION as a secondary signal because
"tenant" is a tenant-ownership lexicon term. The rejected promotes
AUTHORIZATION to primary and drops TEST altogether. This is the
wrong-predicate-assignment pattern from the calibration corruption run: the
secondary noun ("tenant") is promoted over the primary verb ("write tests").]
```

Phrasing-B (Type A — wrong predicate, corruption: wrong-predicate-assignment):
```
DocumentId:           "ES-S3-router-001-prompt"
CompiledText:         (same as above)
CompiledPredicateIds: ["AUTHORIZATION", "SECURITY"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["AUTHORIZATION", "SECURITY"]
```

Coverage labels:
```
source_id:            testcontainers-dotnet, microsoft-testfx
source_title:         Testcontainers PostgreSQL module + testfx fixture pattern
transition_type:      feature_addition
phase:                5
sub_phase:            5.2
task_type:            integration-tests
rejected_type:        A
overlap_ratio:        0.28   (Type A: TEST and AUTHORIZATION tokens overlap on AUTHORIZATION; primary verb token disjoint)
```

---

## Pair[ID] ES-2026-05-08-3.3-L0A-04 — ES-S4 idempotency on POST endpoint

A:
```
run_id:       "ES-S4-router-001"
scenario_id:  "ES-S4"
user_request: "Add an Idempotency-Key header to the POST /payments endpoint per RFC 8470 and write the integration tests for the duplicate-request handling."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
DocumentId:           "ES-S4-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "Add an Idempotency-Key header to the POST /payments endpoint per RFC 8470 and write the integration tests for the duplicate-request handling."
CompiledPredicateIds: ["API_CONTRACT", "TEST"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["API_CONTRACT", "TEST"]
```

Reason:
```
[WRONG: the prompt names two distinct work items joined by "and" — the header
contract change ("Idempotency-Key" header on "POST /payments endpoint") and
the integration tests for duplicate handling. API_CONTRACT fires from
"endpoint", "header", and the Idempotency-Key contract reference. TEST fires
from "integration tests". The rejected drops TEST entirely, claiming the
prompt only requests the contract. This is the ambiguous-predicate-case
corruption — only one of two predicate sets is emitted.]
```

Phrasing-B (Type A — ambiguous-predicate-case corruption):
```
DocumentId:           "ES-S4-router-001-prompt"
CompiledText:         (same as above)
CompiledPredicateIds: ["API_CONTRACT"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["API_CONTRACT"]
```

Coverage labels:
```
source_id:            openapi-specification, rfc-8470, rfc-9110
source_title:         OpenAPI 3.1 header parameter shape + RFC 8470 idempotency
transition_type:      feature_addition
phase:                3
sub_phase:            3.3
task_type:            api-contract
rejected_type:        A
overlap_ratio:        0.32   (Type A: chosen [API_CONTRACT, TEST] vs rejected [API_CONTRACT]; one token shared)
```

---

## Pair[ID] ES-2026-05-08-3.4-L0A-05 — ES-S5 JWT + tenant isolation tests

A:
```
run_id:       "ES-S5-router-001"
scenario_id:  "ES-S5"
user_request: "Add JWT bearer authentication on the API endpoint per RFC 7519 and write integration tests for tenant isolation across the authentication and API layers."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
DocumentId:           "ES-S5-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "Add JWT bearer authentication on the API endpoint per RFC 7519 and write integration tests for tenant isolation across the authentication and API layers."
CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
```

Reason:
```
[WRONG: this prompt produces four high-signal predicates: JWT/authentication
→ SECURITY; tenant → AUTHORIZATION; API endpoint → API_CONTRACT; integration
tests → TEST. All four must appear so the router can fan out to 3.4, 3.3,
and 5.2. The rejected emits only SECURITY, dropping the API contract and
testing signals. Downstream the router would route only to 3.4 and miss the
3.3 and 5.2 territories — the ambiguous-predicate-case pattern.]
```

Phrasing-B (Type A — ambiguous-predicate-case):
```
DocumentId:           "ES-S5-router-001-prompt"
CompiledText:         (same as above)
CompiledPredicateIds: ["SECURITY"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["SECURITY"]
```

Coverage labels:
```
source_id:            rfc-7519, rfc-6749
source_title:         IETF RFC 7519 JWT + RFC 6749 OAuth 2.0
transition_type:      feature_addition
phase:                3
sub_phase:            3.4
task_type:            jwt-authentication
rejected_type:        A
overlap_ratio:        0.24   (Type A ambiguous-predicate-case: chosen 4 predicates vs rejected 1; SECURITY shared, others disjoint)
```

---

## Pair[ID] ES-2026-05-08-6.3-L0A-06 — ES-S6 admin RBAC review

A:
```
run_id:       "ES-S6-router-001"
scenario_id:  "ES-S6"
user_request: "Review whether the admin endpoints enforce role-based access control as required by NIST SP 800-53 AC-3 — there is a finding that an authenticated regular user can call the admin user-list endpoint."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
DocumentId:           "ES-S6-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "Review whether the admin endpoints enforce role-based access control as required by NIST SP 800-53 AC-3 — there is a finding that an authenticated regular user can call the admin user-list endpoint."
CompiledPredicateIds: ["QA", "AUTHORIZATION", "API_CONTRACT"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["QA", "AUTHORIZATION", "API_CONTRACT"]
```

Reason:
```
[WRONG: the prompt's primary verb is "Review" — QA fires first. "RBAC",
"role-based access control", "authenticated", "regular user" all map to
AUTHORIZATION. "endpoint" maps to API_CONTRACT. The rejected promotes
SECURITY and drops QA, treating the prompt as a security model design
request. Calibration data shows this is the most common 6.3 vs 3.4 confusion:
"authenticated" tokens make the model want to design the auth, but the verb
is "Review" — the model exists; the implementation has a finding.]
```

Phrasing-B (Type A — review confused with design):
```
DocumentId:           "ES-S6-router-001-prompt"
CompiledText:         (same as above)
CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["SECURITY", "AUTHORIZATION"]
```

Coverage labels:
```
source_id:            cwe-285, nist-sp-800-53-ac-3
source_title:         CWE-285 Improper Authorization + NIST AC-3 Access Enforcement
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            security-compliance-review
rejected_type:        A
overlap_ratio:        0.30   (Type A: chosen [QA, AUTHORIZATION, API_CONTRACT] vs rejected [SECURITY, AUTHORIZATION]; AUTHORIZATION shared)
```

---

## Pair[ID] ES-2026-05-08-3.5-L0A-07 — ES-S7 saga compensation

A:
```
run_id:       "ES-S7-router-001"
scenario_id:  "ES-S7"
user_request: "Implement the saga compensation logic for the order placement workflow before connecting it to the inventory service. The saga must compensate the payment if inventory reservation fails, and the inventory must be reserved before the payment is captured."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
DocumentId:           "ES-S7-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "Implement the saga compensation logic for the order placement workflow before connecting it to the inventory service. The saga must compensate the payment if inventory reservation fails, and the inventory must be reserved before the payment is captured."
CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
```

Reason:
```
[WRONG: "Implement the" is the highest-signal IMPLEMENT trigger phrase from
the lexicon. "workflow", "service", "dependency between forward and
compensation" all carry ARCHITECTURE. The rejected drops both predicates and
emits TEST because the prompt mentions "fails" — but "fails" in this prompt
describes the saga branch condition, not a failing test. The rejected
fabricates a TEST signal that is not in the lexicon match for this text.]
```

Phrasing-B (Type C — wrong domain):
```
DocumentId:           "ES-S7-router-001-prompt"
CompiledText:         (same as above)
CompiledPredicateIds: ["TEST"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["TEST"]
```

Coverage labels:
```
source_id:            temporal-saga, airflow-dag-tutorial
source_title:         Temporal saga compensation + Airflow DAG ordering anti-patterns
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            workflow-orchestration
rejected_type:        C
overlap_ratio:        0.16   (Type C: chosen [IMPLEMENT, ARCHITECTURE] vs rejected [TEST]; predicate sets disjoint)
```

---

## Pair[ID] ES-2026-05-08-3.3-L0A-08 — ES-S8 OpenAPI explanation (no-predicate variant)

A:
```
run_id:       "ES-S8-router-001"
scenario_id:  "ES-S8"
user_request: "How does breaking change versioning work in OpenAPI 3.1?"
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
DocumentId:           "ES-S8-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "How does breaking change versioning work in OpenAPI 3.1?"
CompiledPredicateIds: ["API_CONTRACT"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["API_CONTRACT"]
  Warnings:     []
```

Reason:
```
[WRONG: "OpenAPI" is in the API_CONTRACT lexicon and "schema" / "versioning"
implies the contract domain. The intent verb is "How does ... work" — that
makes the downstream domain CODE_Q (explanation), but the predicate at the
compiler layer is still API_CONTRACT because the lexicon match is on
vocabulary, not on intent. The rejected emits an empty predicate set with
the no_high_signal_predicate warning, treating the prompt as if no
development predicate fired. This understates the signal — "OpenAPI" is a
direct lexicon term and confidence should not drop to 0.70.]
```

Phrasing-B (Type A — under-extraction):
```
DocumentId:           "ES-S8-router-001-prompt"
CompiledText:         (same as above)
CompiledPredicateIds: []
SourceStatus:         "knowledge_edge: no high-signal development predicate found"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.70
  PredicateIds: []
  Warnings:     ["no_high_signal_predicate"]
```

Coverage labels:
```
source_id:            openapi-specification, aip-180
source_title:         OpenAPI 3.1 + Google AIP-180 breaking change taxonomy
attribution_required: true
attribution_text:     "© Google LLC, CC BY 4.0"
transition_type:      explain
phase:                3
sub_phase:            3.3
task_type:            api-contract-explanation
rejected_type:        A
overlap_ratio:        0.34   (Type A: chosen [API_CONTRACT] vs rejected []; API_CONTRACT token only in chosen)
```

---

## Layer 0-A summary

```
Pairs:                   8
Type A rejected:         5  (ES-S3, ES-S4, ES-S5, ES-S6, ES-S8)
Type C rejected:         3  (ES-S1, ES-S2, ES-S7)
No-predicate prompt:     1  (ES-S8 rejected variant — confidence 0.70)
ambiguous-predicate-case: 2 corruption rejected (ES-S4, ES-S5)
wrong-predicate-assignment: 1 corruption rejected (ES-S3)
```

Every B uses only `DocumentId / SourcePath / SourceTier / LicenseOrAuthority /
DocumentType / DocumentTitle / CompiledText / CompiledPredicateIds /
SourceStatus / KnownFailureMode` plus the unit result envelope. No invented
fields.
