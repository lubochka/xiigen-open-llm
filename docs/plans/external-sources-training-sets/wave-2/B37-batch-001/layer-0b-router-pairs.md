# Layer 0-B — Router Pairs
# Batch: B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Contract: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B37B1-WDA-router-001 — DOCUMENT from stakeholder mention → routes to 2.4

```
source_id: xiigen-b37-router-001
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: unknown (routing proceeds on DOCUMENT; gap dominates intent)
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from B37B1-WDA-compiler-001 corrected B output]
  DocumentId: "B37-compiler-001-prompt"
  CompiledText: "Track progress across all active projects and provide a status report to stakeholders by Friday."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B37-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  DOCUMENT fires on "stakeholder" → routes to Phase 2. "stakeholder" specifically
  points to 2.4 (External Stakeholder Requirements) over 2.1 (Business Requirements
  and Use Cases). 2.1 is the nearest wrong sub-phase (NearestWrongSubPhaseScore 0.41).
  The gap vocabulary (track, progress, status report) does not alter the routing —
  gap vocabulary produces CapabilityGapWarnings only.

---

## Pair B37B1-WDA-router-002 — zero predicates → knowledge-edge routing

```
source_id: xiigen-b37-router-002
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: unknown
coverage_status: knowledge_edge
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from B37B1-WDA-compiler-002 B output]
  DocumentId: "B37-compiler-002-prompt"
  CompiledText: "Coordinate the cross-team dependencies and resolve the blockers preventing the release."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B37-compiler-002-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"

  DecisionCount: 1

Routing rationale:
  CompiledPredicateIds is empty. "dependencies" does NOT fire API_CONTRACT because
  the request has no API-domain co-occurrence (no: api, endpoint, contract, interface,
  swagger, openapi, rest, graphql). The router cannot select any registered sub-phase.
  KnowledgeEdge: true is mandatory. FailureClassification is
  "no_predicate_match_capability_gap" (not "no_predicate_match") because the input
  contains known capability gap vocabulary (coordinate, blockers, cross-team
  dependencies in a project managing context).

---

## Pair B37B1-WDA-router-003 — IMPLEMENT from mixed signal → routes to 4.1

```
source_id: xiigen-b37-router-003
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from B37B1-WDA-compiler-003 B output]
  DocumentId: "B37-compiler-003-prompt"
  CompiledText: "Manage the delivery of the authentication module — build the service, track progress, and escalate blockers."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B37-compiler-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  IMPLEMENT fires on "build", "service", "module" → routes to 4.1 (Backend Services
  and Business Logic). The project managing vocabulary ("manage", "track progress",
  "escalate blockers") is gap vocabulary — it does not alter the routing decision.
  4.2 (Integration and External Services) is the nearest wrong sub-phase
  (NearestWrongSubPhaseScore 0.38) because "authentication" could be read as an
  external service concern, but "build the service" clearly targets 4.1.
