# Layer 0-B — Router Pairs (023)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-023-001 — "retrospective" alone → knowledge-edge (not QA)

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-023-001-prompt"
  CompiledText: "Run the sprint retrospective and capture what went well."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-023-001-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1

BOUNDARY TEACHING: "retrospective" is NOT in the QA lexicon. No predicate fires.
Knowledge-edge is the only valid output for a retrospective-only sentence.

---

## B36-0B-023-002 — "review" in planning context → routes to 6.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-023-002-prompt"
  CompiledText: "Review the sprint outcomes and assess team performance."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-023-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

BOUNDARY TEACHING: "review" fires QA and routes to Phase 6 even in sprint context.

---

## B36-0B-023-003 — "retrospective" + "review" → QA fires, retrospective is gap

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-023-003-prompt"
  CompiledText: "In the retrospective, review the code quality from the last sprint."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-023-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

BOUNDARY TEACHING: "review" fires QA; "retrospective" in the same sentence is gap.
Only "review" drives the routing.

---

## B36-0B-023-004 — "sprint review" → QA + IMPLEMENT + DOCUMENT fires

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-023-004-prompt"
  CompiledText: "Run the sprint review meeting and demonstrate the completed features to stakeholders."
  CompiledPredicateIds: ["QA", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-023-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-023-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.91
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-023-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.86
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 3

---

## B36-0B-023-005 — Retrospective-only zero predicates → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-023-005-prompt"
  CompiledText: "Facilitate the team retrospective and identify process improvements for the next sprint."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-023-005-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1
