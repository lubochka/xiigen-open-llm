# Layer 0-B — Router Pairs (020)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-020-001 — Sprint + IMPLEMENT routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-020-001-prompt"
  CompiledText: "Plan the sprint and build the authentication features scheduled for this iteration."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-020-001-prompt"
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

## B36-0B-020-002 — Roadmap + IMPLEMENT routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-020-002-prompt"
  CompiledText: "Implement the first quarter roadmap items: build the API service and create the database module."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-020-002-prompt"
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

## B36-0B-020-003 — Backlog + IMPLEMENT routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-020-003-prompt"
  CompiledText: "Write the code for the top five backlog items and deploy the updated service."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-020-003-prompt"
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

## B36-0B-020-004 — Epic + IMPLEMENT routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-020-004-prompt"
  CompiledText: "In this sprint, implement the user authentication epic: build the login module and create the token service."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-020-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.93
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B36-0B-020-005 — Sprint + IMPLEMENT + DOCUMENT routes to 4.1 and 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-020-005-prompt"
  CompiledText: "For this sprint: build the notification service, implement the email feature, and document the API requirements."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-020-005-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-020-005-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.90
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 2
