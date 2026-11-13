# Layer 0-B — Multi Sub-Phase Router Pairs
# Batch: O1-batch-001
# Scenario: O1 — multi-tenant JWT authentication
# Contract: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: adaptive_routing_router

---

## Method note

The A side of every router pair is taken verbatim from the B side of the
corresponding compiler pair (layer-0a-compiler-pairs.md). Router A inputs
are never invented independently — they are the compiled document output
that the compiler unit already produced.

---

## Coverage map

| Pair ID | Input predicates | Routing decisions | Knowledge edge |
|---|---|---|---|
| O1B1-WDA-router-001 | SECURITY, AUTHORIZATION, API_CONTRACT, TEST | 3.4, 3.3, 5.1 | false |
| O1B1-WDA-router-002 | (none) | — | true |
| O1B1-WDA-router-003 | DOCUMENT, API_CONTRACT | 3.3, 2.1 | false |

---

## Pair O1B1-WDA-router-001 — multi-predicate fan-out routing

```
source_id: xiigen-o1-router-001
source_title: XIIGen O1 scenario router — multi-predicate fan-out
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: whole_development_area
sub_phase: whole_development_area
task_type: adaptive_routing_multi_router
node_role: multi-sub-phase-router
coverage_status: covered
pair_kind: adaptive_routing_router
```

Contract:
  MultiSubPhaseRouterUnit
      : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>

A:
  DocumentId: "O1-router-001-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: "runtime_prompt_compiled"

B:
  [unit result envelope]
    UnitId: "unit-multi-sub-phase-router-v1"
    UnitRole: "multi-sub-phase-router"
    DomainTag: "whole_development_area"
    Confidence: 0.92
    PredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
    Warnings: []

  RoutingDecisions:
    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.65
      EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.68
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.70
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: null

Routing rationale:
  SECURITY + AUTHORIZATION: only sub-phase 3.4 carries both predicates. Score gap
    (0.92 − 0.65 = 0.27) reflects that 3.3 is the nearest wrong candidate
    (it shares phase 3 but lacks AUTHORIZATION).
  API_CONTRACT: 3.3 is the canonical API contract sub-phase. 4.5 also carries
    API_CONTRACT but its primary predicate is IMPLEMENT; nearest-wrong score 0.68.
  TEST: 5.1 is the primary test sub-phase for isolated unit-level TEST signals.
    5.2 also carries TEST but requires API_CONTRACT as the discriminating signal;
    because API_CONTRACT is already routed to 3.3, TEST alone routes to 5.1.
    Nearest-wrong score 0.70 reflects 5.2's partial overlap.

EM-9 guard: RoutingDecisions contains no dependency_order or parallel_group fields.
  Execution sequencing is an orchestrator concern, not a router output.

---

## Pair O1B1-WDA-router-002 — no-predicate knowledge-edge result

```
source_id: xiigen-o1-router-002
source_title: XIIGen O1 scenario router — no-predicate knowledge edge
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: unknown
phase: whole_development_area
sub_phase: whole_development_area
task_type: adaptive_routing_multi_router
node_role: multi-sub-phase-router
coverage_status: covered
pair_kind: adaptive_routing_router
```

Contract:
  MultiSubPhaseRouterUnit
      : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>

A:
  DocumentId: "O1-router-002-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "The thing on the login page is broken again."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "runtime_prompt_compiled"

B:
  [unit result envelope]
    UnitId: "unit-multi-sub-phase-router-v1"
    UnitRole: "multi-sub-phase-router"
    DomainTag: "whole_development_area"
    Confidence: 0.0
    PredicateIds: []
    Warnings: ["no_routing_predicate_matched"]

  RoutingDecisions:
    - DocumentId: "O1-router-002-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.0
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

Routing rationale:
  CompiledPredicateIds is empty. No sub-phase in the topology registry
  carries a zero-predicate match rule. The router produces exactly one
  decision record with KnowledgeEdge: true and FailureClassification:
  "no_predicate_match". The orchestrator reads this single-entry array
  and routes to the knowledge-edge synthetic flow path.

---

## Pair O1B1-WDA-router-003 — dual-predicate DOCUMENT + API_CONTRACT routing

```
source_id: xiigen-o1-router-003
source_title: XIIGen O1 scenario router — DOCUMENT + API_CONTRACT dual routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: whole_development_area
sub_phase: whole_development_area
task_type: adaptive_routing_multi_router
node_role: multi-sub-phase-router
coverage_status: covered
pair_kind: adaptive_routing_router
```

Contract:
  MultiSubPhaseRouterUnit
      : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>

A:
  DocumentId: "O1-router-003-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "We need to document the API endpoints for our external partners, including request and response schema."
  CompiledPredicateIds: ["DOCUMENT", "API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: "runtime_prompt_compiled"

B:
  [unit result envelope]
    UnitId: "unit-multi-sub-phase-router-v1"
    UnitRole: "multi-sub-phase-router"
    DomainTag: "whole_development_area"
    Confidence: 0.92
    PredicateIds: ["DOCUMENT", "API_CONTRACT"]
    Warnings: []

  RoutingDecisions:
    - DocumentId: "O1-router-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.66
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "O1-router-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.71
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

Routing rationale:
  API_CONTRACT: 3.3 is the canonical api-contract sub-phase. Nearest wrong is 4.5
    (provider-connectors, also carries API_CONTRACT but primary is IMPLEMENT);
    score gap 0.92 − 0.66 = 0.26.
  DOCUMENT: 2.1 is the primary single-predicate DOCUMENT sub-phase (business-capability).
    2.4 (edge-cases) also carries DOCUMENT but is the nearest wrong candidate;
    score gap 0.92 − 0.71 = 0.21 reflects the closer predicate overlap among
    DOCUMENT-carrying phase-2 sub-phases.
  Note: 2.3 carries both API_CONTRACT and DOCUMENT, but its routing confidence
    is subordinated to the more specific 3.3 (API_CONTRACT primary) and 2.1
    (DOCUMENT primary) decisions. 2.3 does not appear because the router assigns
    each predicate signal to its primary sub-phase first.
