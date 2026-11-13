# Layer 0-B — Router Pairs (016)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-016-001 — Product-roadmap zero predicates → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-016-001-prompt"
  CompiledText: "Present the product roadmap to the board and walk them through the Q3 and Q4 milestones."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-016-001-prompt"
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

## B36-0B-016-002 — Product-roadmap with DOCUMENT → routes to 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-016-002-prompt"
  CompiledText: "Document the product roadmap and specify the requirements for each milestone deliverable."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-016-002-prompt"
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

## B36-0B-016-003 — Product-roadmap with ARCHITECTURE → routes to 3.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-016-003-prompt"
  CompiledText: "Design the product roadmap structure and architect the delivery phases for each epic."
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-016-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      Confidence: 0.91
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-016-004 — Product-roadmap with QA+DOCUMENT → routes to 6.1 and 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-016-004-prompt"
  CompiledText: "Review the product roadmap with stakeholders and approve the milestone targets."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-016-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-016-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.86
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 2

---

## B36-0B-016-005 — Product-roadmap five gap terms → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-016-005-prompt"
  CompiledText: "Align the product roadmap with sprint velocity, epic priorities, and burndown projections for Q3."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-016-005-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1
