# Layer 0-B — Router Pairs (019)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-019-001 — "story" false positive routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-019-001-prompt"
  CompiledText: "Estimate the story points for each backlog item before the sprint starts."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-019-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Routing rationale:
  DOCUMENT fires on "story" (false positive via "story points"). Routes to 2.1.
  Effort estimation intent is not routed. Backlog and sprint gap warnings emitted.

---

## B36-0B-019-002 — "user story" routes to 2.1 (correct in intent and lexicon)

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-019-002-prompt"
  CompiledText: "Write up the user story for the checkout flow and assign story points."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-019-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.90
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Routing rationale:
  DOCUMENT fires on "story" (true positive: "user story" authoring). Routes to 2.1.
  In this case the routing matches intent: user story writing is Phase 2.1 work.

---

## B36-0B-019-003 — "stories" plural routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-019-003-prompt"
  CompiledText: "Assign story points to all backlog stories before the sprint planning meeting."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-019-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-019-004 — "story" + "feature" routes to 2.1 and 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-019-004-prompt"
  CompiledText: "Create a user story for the notification feature."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-019-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.89
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-019-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.91
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 2

---

## B36-0B-019-005 — "story" + retrospective + sprint routes to 6.1 and 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-019-005-prompt"
  CompiledText: "In the retrospective, review the story points assigned last sprint and update velocity."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-019-005-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-019-005-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.87
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 2

Routing rationale:
  QA fires on "review". DOCUMENT fires on "story". Retrospective, sprint, velocity are
  gap warnings. Two routing decisions produced. KnowledgeEdge: false for both.
