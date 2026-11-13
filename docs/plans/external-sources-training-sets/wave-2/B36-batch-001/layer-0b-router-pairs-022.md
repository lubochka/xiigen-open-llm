# Layer 0-B — Router Pairs (022)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-022-001 — Roadmap + DOCUMENT routes to 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-022-001-prompt"
  CompiledText: "Document the roadmap items and specify the requirements for each quarterly milestone."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-022-001-prompt"
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

## B36-0B-022-002 — Sprint + DOCUMENT (criteria) routes to 2.4

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-022-002-prompt"
  CompiledText: "Write the acceptance criteria for each sprint ticket and describe the expected behavior."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-022-002-prompt"
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

## B36-0B-022-003 — Backlog + DOCUMENT (functional+spec) routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-022-003-prompt"
  CompiledText: "Define the backlog requirements and document the functional specifications for each epic."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-022-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.90
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Routing rationale:
  DOCUMENT fires on "define", "requirements", "document", "functional", "specifications".
  Routes to 2.1 (Business Requirements and Use Cases) because "functional specifications"
  is primarily a business requirements artifact. NearestWrongSubPhaseScore 0.43 reflects
  proximity to 2.4 (stakeholder requirements).

---

## B36-0B-022-004 — Velocity + DOCUMENT routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-022-004-prompt"
  CompiledText: "Document the team velocity metrics and specify how they relate to the sprint capacity."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-022-004-prompt"
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

## B36-0B-022-005 — Milestone + DOCUMENT (use-case) routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-022-005-prompt"
  CompiledText: "Describe the deliverables for each roadmap milestone and define the use-case for each major epic."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-022-005-prompt"
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
  DOCUMENT fires on "describe", "define", "use-case". Routes to 2.1 (Business Requirements
  and Use Cases) — "use-case" is the primary signal for 2.1. Milestone/roadmap/epic are
  gap warnings but do not alter routing.
