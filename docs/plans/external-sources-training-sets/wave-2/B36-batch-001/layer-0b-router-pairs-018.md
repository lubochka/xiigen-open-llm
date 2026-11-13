# Layer 0-B — Router Pairs (018)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-018-001 — Prioritize/triage zero predicates → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-018-001-prompt"
  CompiledText: "Prioritize the backlog items and triage the incoming tickets for the upcoming sprint."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-018-001-prompt"
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

## B36-0B-018-002 — Triage with DOCUMENT (define+criteria) → routes to 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-018-002-prompt"
  CompiledText: "Triage the incoming backlog and define the priority criteria for each ticket type."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-018-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.90
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-018-003 — Prioritize with QA (review+approve) → routes to 6.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-018-003-prompt"
  CompiledText: "Review the prioritized backlog and approve the sprint scope for the team."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-018-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-018-004 — Triage five gap terms → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-018-004-prompt"
  CompiledText: "Triage the epic backlog, prioritize the sprint tickets, and set velocity targets for the next iteration."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-018-004-prompt"
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

## B36-0B-018-005 — Prioritize with IMPLEMENT (build top features) → routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-018-005-prompt"
  CompiledText: "Prioritize the backlog and build the top three features in this sprint."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-018-005-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1
