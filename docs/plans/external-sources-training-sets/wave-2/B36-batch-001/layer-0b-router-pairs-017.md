# Layer 0-B — Router Pairs (017)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-017-001 — Grooming zero predicates → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-017-001-prompt"
  CompiledText: "Groom the product backlog before the next sprint planning session."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-017-001-prompt"
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

## B36-0B-017-002 — Refinement zero predicates → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-017-002-prompt"
  CompiledText: "Run the backlog refinement meeting and reprioritize the tickets for the upcoming iteration."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-017-002-prompt"
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

## B36-0B-017-003 — Grooming with DOCUMENT (story+criteria) → routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-017-003-prompt"
  CompiledText: "Groom the backlog and refine the user story acceptance criteria for each ticket."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-017-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.89
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-017-004 — Grooming with IMPLEMENT → routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-017-004-prompt"
  CompiledText: "After grooming, implement the highest-priority backlog features in the current sprint."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-017-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-017-005 — Grooming with DOCUMENT (story fires) → routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-017-005-prompt"
  CompiledText: "Facilitate the backlog grooming session, triage the epic tickets, and estimate story-points before the sprint."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-017-005-prompt"
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
  DOCUMENT fires on "story" (false positive via "story-points"). Routes to 2.1.
  Grooming, backlog, epic, ticket, sprint gap warnings emitted but routing proceeds
  based on the DOCUMENT predicate alone.
