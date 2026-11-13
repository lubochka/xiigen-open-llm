# Layer 0-B — Multi-Sub-Phase Router Pairs

2026-05-11 routing-flow correction: this file is a diagnostic station pair set,
not a complete routing training surface. These pairs teach router behavior
inside routing, but accepted routing must be measured through:

```csharp
OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>
```

or equivalent `AdaptiveLogicalOrchestrator` behavior. If a batch has this file
but no flow-level Layer 0-F pairs, its routing status is
`PAIRSET_READY_ROUTING_FLOW_PENDING`, not `ACCEPT_BATCH`.

```
Contract:
  MultiSubPhaseRouterUnit
      : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   MultiSubPhaseRouterUnit
node_role:            layer_0_b
pair_kind:            adaptive_routing_multi_phase
coverage_status:      covered
attribution_required: false   (overridden to true on ES-S8 pair only)
```

Per-pair labels carry: `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `rejected_type`, `overlap_ratio`.

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60.

Eight pairs. Every A side is the chosen B output of the corresponding
Layer 0-A pair (no invented A shapes).

Every B side uses only actual `SubPhaseRoutingDecision` fields:

```
DocumentId, SourceStatus, PredictedPhaseId, PredictedSubPhaseId,
Confidence, NearestWrongSubPhaseScore, EvidencePredicates,
KnowledgeEdge, FailureClassification
```

No `dependency_order`, `parallel_group`, `domain_label`, or `is_primary`
fields appear in any B. Dependency ordering is read from
`ILogicalTopologyRegistry.ListTransitions()` at runtime.

---

## Pair[ID] ES-2026-05-08-3.1-L0B-01 — ES-S1 routes to 3.1 only

A:
```
DocumentId:           "ES-S1-router-001-prompt"
CompiledText:         "We need to define the component boundaries between the order service and the inventory service. The order service must own the order lifecycle and the inventory service must own stock state — neither service may read the other's storage directly."
CompiledPredicateIds: ["ARCHITECTURE"]
SourceStatus:         "ready"
```

B:
```
routing_decisions:
  - DocumentId:                "ES-S1-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.1"
    Confidence:                0.86
    NearestWrongSubPhaseScore: 0.41
    EvidencePredicates:        ["ARCHITECTURE"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: with only the ARCHITECTURE predicate, scoring against the registered
sub-phase set produces 3.1 (Architecture and Component Topology) at the top.
The text terms "boundaries", "service", "ownership" align with the 3.1
Name+Purpose vocabulary. The rejected routes to 4.1 instead — but 4.1's
Purpose is "backend service implementation, provider connectors, integration
code, and application service changes". Implementation is the wrong
responsibility for a prompt asking to define ownership rules. This is the
3.1 vs 4.1 confusion documented in the ARCH confusion table.]
```

Phrasing-B (Type A — 3.1 confused with 4.1):
```
routing_decisions:
  - DocumentId:                "ES-S1-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "4"
    PredictedSubPhaseId:       "4.1"
    Confidence:                0.78
    NearestWrongSubPhaseScore: 0.55
    EvidencePredicates:        ["ARCHITECTURE"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:       arc42-template, ADR-template
source_title:    arc42 building-block view + ADR Decision section
transition_type: feature_addition
phase:           3
sub_phase:       3.1
task_type:       architecture-topology
rejected_type:   A
overlap_ratio:   0.26   (Type A: chosen 3.1 vs rejected 4.1; sub-phase id tokens disjoint, evidence shared)
```

---

## Pair[ID] ES-2026-05-08-3.5-L0B-02 — ES-S2 routes to 3.5

A:
```
DocumentId:           "ES-S2-router-001-prompt"
CompiledText:         "Design what to measure and alert on for our payment processor service. We need to know which workflows are running, which retries are accumulating, and where compensation activities fire."
CompiledPredicateIds: ["ARCHITECTURE"]
SourceStatus:         "ready"
```

B:
```
routing_decisions:
  - DocumentId:                "ES-S2-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.5"
    Confidence:                0.83
    NearestWrongSubPhaseScore: 0.62
    EvidencePredicates:        ["ARCHITECTURE"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: 3.5 Observability and Operational Design owns "what to measure,
alert thresholds, and what not to log". The text terms "measure", "alert",
"workflows running", "compensation activities fire" all map to the 3.5
Purpose vocabulary. The rejected routes to 7.3 Monitoring and Feedback Loop.
7.3 owns "runtime observation and operational loop closure" — that is
configuring the dashboard once the signals exist. Designing what to capture
must happen first. This is the 3.5 vs 7.3 confusion documented in the ARCH
confusion table.]
```

Phrasing-B (Type A — 3.5 confused with 7.3):
```
routing_decisions:
  - DocumentId:                "ES-S2-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "7"
    PredictedSubPhaseId:       "7.3"
    Confidence:                0.74
    NearestWrongSubPhaseScore: 0.62
    EvidencePredicates:        ["ARCHITECTURE"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:       temporal-observability-docs, dapr-observability-docs
source_title:    Temporal observability + Dapr observability
transition_type: feature_addition
phase:           3
sub_phase:       3.5
task_type:       observability-design
rejected_type:   A
overlap_ratio:   0.28   (Type A: chosen 3.5 vs rejected 7.3; sub-phase id tokens disjoint)
```

---

## Pair[ID] ES-2026-05-08-5.2-L0B-03 — ES-S3 routes to 5.2

A:
```
DocumentId:           "ES-S3-router-001-prompt"
CompiledText:         "Write integration tests that verify postgres tenant isolation across the orders module. The tests must spin up a real postgres container and assert that tenant A cannot read tenant B's order rows."
CompiledPredicateIds: ["TEST", "AUTHORIZATION"]
SourceStatus:         "ready"
```

B:
```
routing_decisions:
  - DocumentId:                "ES-S3-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "5"
    PredictedSubPhaseId:       "5.2"
    Confidence:                0.88
    NearestWrongSubPhaseScore: 0.69
    EvidencePredicates:        ["TEST"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: 5.2 Integration Tests owns "cross-component validation, database
integration tests, and fixture construction". The Testcontainers documentation
authority defines real-container tests as the integration-test boundary —
mock isolation is sufficient for 5.1 Unit Tests but not for tenant-isolation
verification. The rejected routes to 5.1 Unit Tests, which would be satisfied
by mocking the database. Mocked rows cannot prove that tenant B's rows are
inaccessible because the mock is the entire data source. This is the 5.1 vs
5.2 confusion documented in the testing confusion table.]
```

Phrasing-B (Type A — 5.2 confused with 5.1):
```
routing_decisions:
  - DocumentId:                "ES-S3-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "5"
    PredictedSubPhaseId:       "5.1"
    Confidence:                0.72
    NearestWrongSubPhaseScore: 0.69
    EvidencePredicates:        ["TEST"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:       testcontainers-dotnet, microsoft-testfx
source_title:    Testcontainers PostgreSQL module + testfx fixture pattern
transition_type: feature_addition
phase:           5
sub_phase:       5.2
task_type:       integration-tests
rejected_type:   A
overlap_ratio:   0.27   (Type A: chosen 5.2 vs rejected 5.1; phase shared, sub-phase tail differs)
```

---

## Pair[ID] ES-2026-05-08-3.3+5.2-L0B-04 — ES-S4 fans out to 3.3 + 5.2

A:
```
DocumentId:           "ES-S4-router-001-prompt"
CompiledText:         "Add an Idempotency-Key header to the POST /payments endpoint per RFC 8470 and write the integration tests for the duplicate-request handling."
CompiledPredicateIds: ["API_CONTRACT", "TEST"]
SourceStatus:         "ready"
```

B:
```
routing_decisions:
  - DocumentId:                "ES-S4-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.3"
    Confidence:                0.85
    NearestWrongSubPhaseScore: 0.51
    EvidencePredicates:        ["API_CONTRACT"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
  - DocumentId:                "ES-S4-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "5"
    PredictedSubPhaseId:       "5.2"
    Confidence:                0.81
    NearestWrongSubPhaseScore: 0.66
    EvidencePredicates:        ["TEST"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: API_CONTRACT predicate (3.3 endpoint contract change) and TEST
predicate (5.2 integration test) each independently support a routing
decision. The router must emit BOTH decisions. The rejected emits only the
3.3 decision and drops 5.2 — the ambiguous-predicate-case corruption.
Downstream this would suppress the testing branch entirely and deny the
operator visibility into the test work that the prompt explicitly requested.]
```

Phrasing-B (Type A — ambiguous-predicate-case corruption):
```
routing_decisions:
  - DocumentId:                "ES-S4-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.3"
    Confidence:                0.85
    NearestWrongSubPhaseScore: 0.51
    EvidencePredicates:        ["API_CONTRACT"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:       openapi-specification, rfc-8470, rfc-9110
source_title:    OpenAPI 3.1 + RFC 8470 idempotency
transition_type: feature_addition
phase:           3
sub_phase:       3.3+5.2
task_type:       multi-route-contract-and-tests
rejected_type:   A
overlap_ratio:   0.36   (Type A ambiguous-predicate-case: chosen 2 decisions vs rejected 1; one decision shared)
```

---

## Pair[ID] ES-2026-05-08-3.4-L0B-05 — ES-S5 multi-route 3.4 + 3.3 + 5.2

A:
```
DocumentId:           "ES-S5-router-001-prompt"
CompiledText:         "Add JWT bearer authentication on the API endpoint per RFC 7519 and write integration tests for tenant isolation across the authentication and API layers."
CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
SourceStatus:         "ready"
```

B:
```
routing_decisions:
  - DocumentId:                "ES-S5-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.4"
    Confidence:                0.91
    NearestWrongSubPhaseScore: 0.52
    EvidencePredicates:        ["AUTHORIZATION", "SECURITY"]
    KnowledgeEdge:             false
    FailureClassification:     "none"
  - DocumentId:                "ES-S5-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.3"
    Confidence:                0.78
    NearestWrongSubPhaseScore: 0.41
    EvidencePredicates:        ["API_CONTRACT"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
  - DocumentId:                "ES-S5-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "5"
    PredictedSubPhaseId:       "5.2"
    Confidence:                0.84
    NearestWrongSubPhaseScore: 0.48
    EvidencePredicates:        ["TEST"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: ES-S5 is the canonical multi-route example — three predicates each
support a different sub-phase. 3.4 is registered (KnowledgeEdge: false) and
will produce a real flow result. 3.3 and 5.2 are not registered and must
emit KnowledgeEdge: true. The rejected merges all three predicates into a
single 3.4 decision and drops 3.3 and 5.2. This is the canonical
ambiguous-predicate-case for adaptive routing. The downstream knowledge-edge
detection layer would never fire for 3.3 or 5.2 because the router never
emitted them.]
```

Phrasing-B (Type A — ambiguous-predicate-case):
```
routing_decisions:
  - DocumentId:                "ES-S5-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.4"
    Confidence:                0.91
    NearestWrongSubPhaseScore: 0.52
    EvidencePredicates:        ["AUTHORIZATION", "SECURITY", "API_CONTRACT", "TEST"]
    KnowledgeEdge:             false
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:       rfc-7519, rfc-6749
source_title:    IETF RFC 7519 JWT + RFC 6749 OAuth 2.0
transition_type: feature_addition
phase:           3
sub_phase:       3.4+3.3+5.2
task_type:       multi-route-jwt-contract-tests
rejected_type:   A
overlap_ratio:   0.41   (Type A ambiguous-predicate-case: chosen 3 decisions vs rejected 1; predicate set merged into single decision)
```

---

## Pair[ID] ES-2026-05-08-6.3-L0B-06 — ES-S6 routes to 6.3

A:
```
DocumentId:           "ES-S6-router-001-prompt"
CompiledText:         "Review whether the admin endpoints enforce role-based access control as required by NIST SP 800-53 AC-3 — there is a finding that an authenticated regular user can call the admin user-list endpoint."
CompiledPredicateIds: ["QA", "AUTHORIZATION", "API_CONTRACT"]
SourceStatus:         "ready"
```

B:
```
routing_decisions:
  - DocumentId:                "ES-S6-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "6"
    PredictedSubPhaseId:       "6.3"
    Confidence:                0.87
    NearestWrongSubPhaseScore: 0.71
    EvidencePredicates:        ["QA", "AUTHORIZATION"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: 6.3 Security and Compliance Review owns "implemented-security review,
compliance evidence, vulnerability checks, and control validation". The
prompt names a finding: an authenticated regular user can call an admin
endpoint. NIST AC-3 cited in the prompt is the control authority. The
rejected routes to 3.4 Security and Authorization Model. 3.4 owns "designing"
the model — but the prompt does not ask to design. The model exists; the
implementation has a finding. Routing to 3.4 would produce a redesign
instead of a review verdict. This is the 6.3 vs 3.4 confusion documented in
the ARCH confusion table.]
```

Phrasing-B (Type A — 6.3 confused with 3.4):
```
routing_decisions:
  - DocumentId:                "ES-S6-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.4"
    Confidence:                0.79
    NearestWrongSubPhaseScore: 0.71
    EvidencePredicates:        ["AUTHORIZATION"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:       cwe-285, nist-sp-800-53-ac-3
source_title:    CWE-285 + NIST AC-3
transition_type: repair
phase:           6
sub_phase:       6.3
task_type:       security-compliance-review
rejected_type:   A
overlap_ratio:   0.31   (Type A: chosen 6.3 vs rejected 3.4; cross-phase routing, evidence partially shared)
```

---

## Pair[ID] ES-2026-05-08-3.5-L0B-07 — ES-S7 routes to 3.5 + 4.1

A:
```
DocumentId:           "ES-S7-router-001-prompt"
CompiledText:         "Implement the saga compensation logic for the order placement workflow before connecting it to the inventory service. The saga must compensate the payment if inventory reservation fails, and the inventory must be reserved before the payment is captured."
CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
SourceStatus:         "ready"
```

B:
```
routing_decisions:
  - DocumentId:                "ES-S7-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.5"
    Confidence:                0.82
    NearestWrongSubPhaseScore: 0.55
    EvidencePredicates:        ["ARCHITECTURE"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
  - DocumentId:                "ES-S7-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "4"
    PredictedSubPhaseId:       "4.1"
    Confidence:                0.79
    NearestWrongSubPhaseScore: 0.51
    EvidencePredicates:        ["IMPLEMENT"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: ES-S7 has two distinct work items — the saga design (workflow
orchestration → 3.5) and the implementation (4.1). Both predicates fire and
both decisions must be emitted. The rejected emits only 4.1, dropping the
3.5 design responsibility. Temporal saga documentation explicitly separates
the workflow declaration (the orchestration design, 3.5) from the activity
implementation (4.1). Implementing without the orchestration design first
is exactly the anti-pattern the rejected encodes.]
```

Phrasing-B (Type A — ambiguous-predicate-case):
```
routing_decisions:
  - DocumentId:                "ES-S7-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "4"
    PredictedSubPhaseId:       "4.1"
    Confidence:                0.79
    NearestWrongSubPhaseScore: 0.51
    EvidencePredicates:        ["IMPLEMENT"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:       temporal-saga, airflow-dag-tutorial
source_title:    Temporal saga + Airflow DAG tutorial
transition_type: feature_addition
phase:           3
sub_phase:       3.5+4.1
task_type:       multi-route-design-and-implement
rejected_type:   A
overlap_ratio:   0.34   (Type A ambiguous-predicate-case: chosen 2 decisions vs rejected 1)
```

---

## Pair[ID] ES-2026-05-08-3.3-L0B-08 — ES-S8 routes to 3.3 (CODE_Q intent)

A:
```
DocumentId:           "ES-S8-router-001-prompt"
CompiledText:         "How does breaking change versioning work in OpenAPI 3.1?"
CompiledPredicateIds: ["API_CONTRACT"]
SourceStatus:         "ready"
```

B:
```
routing_decisions:
  - DocumentId:                "ES-S8-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "3"
    PredictedSubPhaseId:       "3.3"
    Confidence:                0.74
    NearestWrongSubPhaseScore: 0.49
    EvidencePredicates:        ["API_CONTRACT"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Reason:
```
[WRONG: API_CONTRACT predicate routes to 3.3 — the sub-phase that owns
"endpoint shape, request and response contracts, schema, and compatibility
rules". OpenAPI 3.1 versioning is a 3.3 explanatory question; the downstream
domain will be CODE_Q because the verb is "How does". The rejected routes
to 4.5 Integration and Provider Connectors — but 4.5 owns "external provider
integration and adapters", not contract semantics. AIP-180 is a contract
authority, not a connector authority. Routing to 4.5 would produce a
provider-integration plan when an explanation was asked.]
```

Phrasing-B (Type C — wrong domain, contract → integration):
```
routing_decisions:
  - DocumentId:                "ES-S8-router-001-prompt"
    SourceStatus:              "predicted"
    PredictedPhaseId:          "4"
    PredictedSubPhaseId:       "4.5"
    Confidence:                0.66
    NearestWrongSubPhaseScore: 0.49
    EvidencePredicates:        ["API_CONTRACT"]
    KnowledgeEdge:             true
    FailureClassification:     "none"
```

Coverage labels:
```
source_id:            openapi-specification, aip-180
source_title:         OpenAPI 3.1 + Google AIP-180
attribution_required: true
attribution_text:     "© Google LLC, CC BY 4.0"
transition_type:      explain
phase:                3
sub_phase:            3.3
task_type:            api-contract-explanation
rejected_type:        C
overlap_ratio:        0.23   (Type C: chosen 3.3 vs rejected 4.5; cross-phase, predicate shared)
```

---

## Layer 0-B summary

```
Pairs:                  8
Type A rejected:        7
Type C rejected:        1
ambiguous-predicate-case (corruption shape): 4 (ES-S4, ES-S5, ES-S7, ES-S8 weak)
KnowledgeEdge true:     8 of 9 routed branches (only 3.4 in ES-S5 is registered)
```

Every B field is from the actual `SubPhaseRoutingDecision` schema.
