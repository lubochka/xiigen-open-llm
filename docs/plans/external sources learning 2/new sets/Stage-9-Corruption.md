# Stage 9 — Corruption Cases

Per v7 pipeline plan §9 and v8 examination method §"Stage 9 — Corruption
Cases", every batch must include **at least one of each** of three
corruption kinds:

2026-05-11 routing-flow correction: Stage 9 routing corruption cases remain
valuable, but they are not an acceptance substitute for full routing. A batch
that contains only compiler/router corruption cases must still add Layer 0-F
flow-level held-out evidence before routing can be promoted. Otherwise record
`PAIRSET_READY_ROUTING_FLOW_PENDING`, not `ACCEPT_BATCH`.

```
wrong-predicate-assignment    — compiler assigns wrong predicate set
ambiguous-predicate-case      — router emits one decision when two predicates support two sub-phases
knowledge-edge-suppressed     — PASS returned when KNOWLEDGE_EDGE required
```

These pairs are not regular DPO pairs that teach a sub-phase. They teach
the model to detect specific failure modes and to never produce them.

This file contains exactly three pairs — one per kind.

**Authority separation.** Stage 9 pairs use two distinct kinds of authority:

```
external authority           — the RFC, CWE, NIST, OpenAPI text that grounds
                               the prompt content and the [WRONG: ...]
                               annotation. Recorded in source_id /
                               source_title / source_license_state per pair.

internal compiler contract   — the nine-predicate lexicon, the
                               SubPhaseRouterSourceDocument shape, and the
                               LicenseOrAuthority field value
                               "user_request". These are NOT external
                               sources. They are the runtime contract the
                               compiler unit honours.
```

Every Stage 9 pair below records its external authority under
`source_id` + `source_license_state: external_license_checked` and
separately records the internal contract reference in a non-source
field (`internal_contract_reference`) so the two are not conflated.

The runtime compiler field `LicenseOrAuthority: "user_request"` inside
each B-side document is the runtime contract value the compiler unit
emits — it does not claim the user's prompt itself is an external source.

---

---

## Pair[ID] ES-2026-05-08-COR-01 — wrong-predicate-assignment

Construction rule (per DPO authoring guidance §9): find a prompt where one
lexicon term appears as a noun modifier (secondary) and another appears as
the primary verb or subject. The corrupted version promotes the modifier
to primary.

External authority: RFC 7519 (defines "authentication" as a domain noun,
not a verb) + Microsoft testfx documentation (defines "test coverage" as
a measurement verb).

Internal contract reference: the nine-predicate lexicon
(`PromptRoutingInputCompilerUnit` predicate definitions). The lexicon is
the compiler's internal vocabulary — it is not an external source and is
not recorded as one in the coverage labels.

```
Contract:
  PromptRoutingInputCompilerUnit
      : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
  pair_kind: corruption_case
  corruption_kind: wrong-predicate-assignment
```

A:
```
run_id:       "COR-01-router-001"
scenario_id:  "Stage9-COR-01"
user_request: "Test coverage shows 42% on the authentication module — we need to bring this up to 80% before the release."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B (correct — chosen):
```
DocumentId:           "COR-01-router-001-prompt"
SourcePath:           "logical-orchestrator://user-prompt"
SourceTier:           "runtime_prompt"
LicenseOrAuthority:   "user_request"
DocumentType:         "user_prompt"
DocumentTitle:        "Runtime user prompt"
CompiledText:         "Test coverage shows 42% on the authentication module — we need to bring this up to 80% before the release."
CompiledPredicateIds: ["TEST", "SECURITY"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["TEST", "SECURITY"]
```

Reason:
```
[WRONG: "Test coverage" is the primary verb phrase — TEST predicate fires
first. "authentication module" is the object being measured, not the
action. SECURITY appears as a secondary predicate because "authentication"
is in the lexicon. The corruption promotes SECURITY to primary and
demotes TEST out of the set entirely. Calibration corruption rule: the
secondary noun ("authentication module") cannot displace the primary
verb ("test coverage"). Routing on the corrupted predicate set would send
this prompt to 3.4 Security Model design when the actual work is 5.1 or
7.4 test coverage uplift.]
```

Phrasing-B (corruption — wrong-predicate-assignment):
```
DocumentId:           "COR-01-router-001-prompt"
CompiledText:         "Test coverage shows 42% on the authentication module — we need to bring this up to 80% before the release."
CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
SourceStatus:         "ready"
KnownFailureMode:     "runtime_prompt_compiled"
[unit result envelope]
  Confidence:   0.92
  PredicateIds: ["SECURITY", "AUTHORIZATION"]
```

Coverage labels:
```
source_id:                   rfc-7519, microsoft-testfx
source_title:                IETF RFC 7519 JWT + Microsoft testfx coverage docs
source_license_state:        external_license_checked
attribution_required:        false
internal_contract_reference: nine-predicate-lexicon
contract_signature:          PromptRoutingInputCompilerUnit
transition_type:             repair
phase:                       5
sub_phase:                   5.1+7.4
task_type:                   coverage-and-reporting
node_role:                   stage_9
coverage_status:             covered
pair_kind:                   corruption_case
corruption_kind:             wrong-predicate-assignment
rejected_type:               corruption_case
overlap_ratio:               0.42  (chosen TEST+SECURITY vs corrupted SECURITY+AUTHORIZATION; predicate-name tokens overlap on SECURITY only)
```

---

## Pair[ID] ES-2026-05-08-COR-02 — ambiguous-predicate-case

Construction rule (per DPO authoring guidance §9): find a prompt where two
different predicates each independently justify a different sub-phase. The
corrupted version routes only to the higher-confidence sub-phase, dropping
the second.

External authority: OpenAPI 3.1 §4 security scheme reference shape +
RFC 6749 §3.3 (scope semantics). Both authorities support both routing
decisions; the router must emit both.

Internal contract reference: `MultiSubPhaseRouterUnit` scoring behavior and
`SubPhaseRoutingDecision` schema. The scoring formula is the router's runtime
contract, not an external source. If a historical copy of this set says
`70/30 scoring`, treat that as stale wording; it must not be used as the
authority for current routing acceptance. Flow-level acceptance still requires
Layer 0-F held-out execution.

```
Contract:
  MultiSubPhaseRouterUnit
      : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
  pair_kind: corruption_case
  corruption_kind: ambiguous-predicate-case
```

A:
```
DocumentId:           "COR-02-router-001-prompt"
CompiledText:         "Add OAuth 2.0 scope-based authorization to the API endpoint /admin/users — the endpoint must require the admin.read scope."
CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"]
SourceStatus:         "ready"
```

B (correct — chosen):
```
routing_decisions:
  - DocumentId:                "COR-02-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.4"
    Confidence:                0.89
    NearestWrongSubPhaseScore: 0.51
    EvidencePredicates:        ["SECURITY", "AUTHORIZATION"]
    KnowledgeEdge:             false
    FailureClassification:     "none"
  - DocumentId:                "COR-02-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.3"
    Confidence:                0.81
    NearestWrongSubPhaseScore: 0.49
    EvidencePredicates:        ["API_CONTRACT"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: SECURITY + AUTHORIZATION (RFC 6749 scope semantics) supports
3.4. API_CONTRACT (OpenAPI endpoint shape, including security scheme
references) supports 3.3. Both must be routed. The corrupted version
emits only the 3.4 decision and drops 3.3 — the operator never sees
the API contract change requirement, so the OpenAPI document is never
updated to reference the new security scheme. Per v7 pipeline plan
§2.5b: when multiple predicates match, all corresponding sub-phases
must be routed.]
```

Phrasing-B (corruption — ambiguous-predicate-case):
```
routing_decisions:
  - DocumentId:                "COR-02-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.4"
    Confidence:                0.89
    NearestWrongSubPhaseScore: 0.51
    EvidencePredicates:        ["SECURITY", "AUTHORIZATION", "API_CONTRACT"]
    KnowledgeEdge:             false
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:                   openapi-specification, rfc-6749
source_title:                OpenAPI 3.1 + IETF RFC 6749 OAuth 2.0
source_license_state:        external_license_checked
attribution_required:        false
internal_contract_reference: multi-sub-phase-router-70-30-scoring + SubPhaseRoutingDecision schema
contract_signature:          MultiSubPhaseRouterUnit
transition_type:             feature_addition
phase:                       3
sub_phase:                   3.4+3.3
task_type:                   adaptive-routing
node_role:                   stage_9
coverage_status:             covered
pair_kind:                   corruption_case
corruption_kind:             ambiguous-predicate-case
rejected_type:               corruption_case
overlap_ratio:               0.38  (chosen 2-decision array vs corrupted 1-decision array; sub-phase-id tokens differ)
```

---

## Pair[ID] ES-2026-05-08-COR-03 — knowledge-edge-suppressed

Construction rule (per DPO authoring guidance §9): use any prompt that
routes to a sub-phase with no registered flow. The corrupted version
returns PASS without the six required knowledge-edge metrics.

External authority: MITRE CWE-287 (Improper Authentication) supplies the
prompt content. Routing target is 5.3 End-to-End Tests, which has no
registered flow.

Internal contract reference: `AdaptiveLogicalOrchestrator`
KNOWLEDGE_EDGE branch shape with all six required metrics. The
six-metric requirement is the orchestrator's runtime contract, not an
external authority.

```
Contract:
  AdaptiveLogicalOrchestrator : ILogicalOrchestrator
  pair_kind: corruption_case
  corruption_kind: knowledge-edge-suppressed
```

A:
```
run_id:       "COR-03-orch-001"
scenario_id:  "Stage9-COR-03"
user_request: "Set up the end-to-end test suite for the checkout flow — sign-in, add to cart, complete payment, verify the order shows in the user's history. Authentication failures must be exercised per CWE-287."
flags:
  UseKnowledgeEdgeDetection: true
```

B (correct — chosen):
```
ScenarioId:      "Stage9-COR-03"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '5.3', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "5.3"
      missing_flow:          "True"
      routed_phase_id:       "5"
      routing_confidence:    "0.83"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["TEST"]
    Warnings:    ["knowledge_edge"]
```

Reason:
```
[WRONG: 5.3 End-to-End Tests has no registered flow. The orchestrator
cannot produce a real PASS verdict for an unregistered sub-phase — there
is no flow to score against. The required output is KNOWLEDGE_EDGE with
all six required metrics. The corruption returns PASS with a fabricated
finding ("end-to-end test suite ready, CWE-287 cases included"). The
system has not actually executed any 5.3 work — there is no flow that
could verify CWE-287 coverage. Per v8 §"corruption case
knowledge-edge-suppressed": PASS fabricates a result the system cannot
produce. The model must never produce this output.]
```

Phrasing-B (corruption — knowledge-edge-suppressed):
```
ScenarioId:      "Stage9-COR-03"
SelectedFlowId:  "fabricated-5.3-pass"
OverallVerdict:  "PASS"
BranchResults:
  - FlowId:           "fabricated-5.3-pass"
    OverallVerdict:   "PASS"
    FinalHumanResult: "End-to-end test suite ready for the checkout flow. CWE-287 authentication failure cases are included."
    Metrics: { }
    EvidenceIds: []
    Warnings:    []
```

Coverage labels:
```
source_id:                   cwe-287
source_title:                MITRE CWE-287 — Improper Authentication
source_license_state:        external_license_checked
attribution_required:        false
internal_contract_reference: AdaptiveLogicalOrchestrator KNOWLEDGE_EDGE six-metric shape
contract_signature:          AdaptiveLogicalOrchestrator
transition_type:             feature_addition
phase:                       5
sub_phase:                   5.3
task_type:                   end-to-end-tests
node_role:                   stage_9
coverage_status:             covered
pair_kind:                   corruption_case
corruption_kind:             knowledge-edge-suppressed
rejected_type:               corruption_case
overlap_ratio:               0.31  (chosen KNOWLEDGE_EDGE w/ six metrics vs corrupted PASS w/ empty metrics; verdict-tokens fully disjoint)
```

---

## Stage 9 summary

```
Pairs:                  3 — exactly one of each required corruption kind
External authorities:   RFC 7519 + microsoft-testfx (COR-01),
                        OpenAPI 3.1 + RFC 6749 (COR-02),
                        MITRE CWE-287 (COR-03)
Internal contracts:     nine-predicate lexicon (COR-01),
                        70/30 scoring + SubPhaseRoutingDecision (COR-02),
                        KNOWLEDGE_EDGE six-metric shape (COR-03)
External-source-only:   true for source_id fields — XIIGen first-party
                        content is NOT used. Internal compiler contracts
                        are referenced but not claimed as external sources.
overlap_ratio gate:     all three pairs < 0.60
```

This satisfies the v8 acceptance checklist line: "Batch contains at least
one of each Stage 9 corruption kind."

---

## Last Updated

2026-05-08.
