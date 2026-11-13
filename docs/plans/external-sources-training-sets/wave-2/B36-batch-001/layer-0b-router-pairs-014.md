# Layer 0-B — Router Pairs (014)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-014-001 — Effort-estimate zero predicates → knowledge-edge routing

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-014-001-prompt"
  CompiledText: "Provide an effort estimate for each item in the backlog before we commit to the sprint."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-014-001-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1

---

## B36-0B-014-002 — Effort-estimate with DOCUMENT (story fires) → routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-014-002-prompt"
  CompiledText: "Estimate the effort for each user story in the backlog and record the story point values."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-014-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Routing rationale:
  DOCUMENT fires on "story" (false positive: "user story" and "story point"). Routes to
  2.1 (Business Requirements/Use Cases). Effort estimation intent is not captured.

---

## B36-0B-014-003 — Effort-estimate with IMPLEMENT → routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-014-003-prompt"
  CompiledText: "Estimate the effort required to implement the payment gateway feature before the sprint starts."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-014-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-014-004 — Effort-estimate with DOCUMENT (document+define+criteria) → routes to 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-014-004-prompt"
  CompiledText: "Document the effort estimates and define acceptance criteria for all backlog items."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-014-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.90
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Routing rationale:
  DOCUMENT fires on "document", "define", "acceptance criteria". Routes to 2.4
  (External Stakeholder Requirements) because "acceptance criteria" maps to
  stakeholder-facing requirements. NearestWrongSubPhaseScore 0.44 reflects 2.1 proximity.

---

## B36-0B-014-005 — Effort-estimate five gap terms → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-014-005-prompt"
  CompiledText: "Run a team effort-estimate session for the epic backlog and align on sprint capacity before grooming."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-014-005-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1
