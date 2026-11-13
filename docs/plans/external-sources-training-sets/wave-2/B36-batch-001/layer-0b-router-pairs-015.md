# Layer 0-B — Router Pairs (015)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-015-001 — Release-planning zero predicates → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-015-001-prompt"
  CompiledText: "Create the release plan for Q4 and map each milestone to a sprint delivery date."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-015-001-prompt"
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

## B36-0B-015-002 — Release-planning with QA+DOCUMENT → routes to 6.1 and 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-015-002-prompt"
  CompiledText: "Review the release plan and confirm the milestone dates with the stakeholders."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-015-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-015-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.86
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 2

---

## B36-0B-015-003 — Release-planning with IMPLEMENT → routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-015-003-prompt"
  CompiledText: "Release planning complete — now build the authentication module and implement the login flow."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-015-003-prompt"
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

## B36-0B-015-004 — Release-planning with DOCUMENT (release notes) → routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-015-004-prompt"
  CompiledText: "Create the release notes document and define the scope for the next release cycle."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-015-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-015-005 — Release-planning five gap terms → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-015-005-prompt"
  CompiledText: "Finalize the release plan timeline, assign milestones to epics, and track sprint velocity against the target."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-015-005-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1
