# Layer 0-B — Router Pairs (021)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-021-001 — Sprint + QA routes to 6.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-021-001-prompt"
  CompiledText: "Review the sprint deliverables and approve the features before releasing to production."
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-021-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-021-001-prompt"
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

## B36-0B-021-002 — Velocity + QA routes to 6.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-021-002-prompt"
  CompiledText: "Review the team velocity report from the last sprint and identify any testing gaps."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-021-002-prompt"
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

## B36-0B-021-003 — Milestone + QA + DOCUMENT routes to 6.1 and 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-021-003-prompt"
  CompiledText: "Review the milestone completion criteria and approve the Q3 deliverables for sign-off."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-021-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-021-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.89
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 2

---

## B36-0B-021-004 — Roadmap + QA + TEST routes to 6.1 and 5.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-021-004-prompt"
  CompiledText: "Review the product roadmap items and test whether the current sprint covers the Q3 goals."
  CompiledPredicateIds: ["QA", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-021-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-021-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 2

---

## B36-0B-021-005 — Backlog + QA + DOCUMENT routes to 6.1 and 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-021-005-prompt"
  CompiledText: "Review the prioritized backlog with the stakeholders and get approval on the sprint scope."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-021-005-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
    - DocumentId: "B36-0A-021-005-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.86
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 2
