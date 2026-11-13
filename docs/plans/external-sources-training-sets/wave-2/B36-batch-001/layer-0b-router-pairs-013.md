# Layer 0-B — Router Pairs (013)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-013-001 — Planning-poker zero predicates → knowledge-edge routing

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-013-001-prompt"
  CompiledText: "Let's run a planning-poker session to estimate the backlog items for the next sprint."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-013-001-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1

Routing rationale:
  CompiledPredicateIds is empty. Knowledge-edge path selected. Planning-poker vocabulary
  (planning-poker, estimate, backlog, sprint) has no registered predicate. No routing
  to any sub-phase is possible.

---

## B36-0B-013-002 — Planning-poker with DOCUMENT → routes to 2.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-013-002-prompt"
  CompiledText: "Run planning poker and assign story point estimates for each backlog item."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-013-002-prompt"
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
  DOCUMENT fires on "story". Routes to Phase 2 (Requirements). 2.1 (Business Requirements
  and Use Cases) is selected because "story" in the DOCUMENT lexicon primarily maps to
  user stories. Planning-poker and backlog gap warnings do not alter routing.

---

## B36-0B-013-003 — Planning-poker with QA → routes to Phase 6

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-013-003-prompt"
  CompiledText: "Review the planning poker results and finalize the sprint estimate."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-013-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Routing rationale:
  QA fires on "review". Routes to Phase 6. The planning-poker context does not prevent
  routing. Sprint gap warning emitted but routing proceeds normally.

---

## B36-0B-013-004 — Planning-poker with IMPLEMENT → routes to 4.1

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-013-004-prompt"
  CompiledText: "After planning poker, implement the top-priority backlog features for the sprint."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-013-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Routing rationale:
  IMPLEMENT fires on "implement" and "features". Routes to 4.1 (Backend Services and
  Business Logic). Planning-poker, backlog, and sprint gap warnings emitted but routing
  is driven by IMPLEMENT predicate alone.

---

## B36-0B-013-005 — Planning-poker five gap terms → knowledge-edge

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-0A-013-005-prompt"
  CompiledText: "Facilitate a planning poker session to groom and estimate the product backlog for the next iteration."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-0A-013-005-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1
