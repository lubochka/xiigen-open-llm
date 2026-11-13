# Layer 0-B — Router Pairs (024)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-024-001 — Complete routing decision table: zero-predicate gap cases

A: [Reference: what does the router produce for every zero-predicate B36 gap input?]

B:
  Rule: "Zero predicates → always knowledge-edge routing with PredictedPhaseId: null, PredictedSubPhaseId: null, KnowledgeEdge: true."
  ZeroPredicateExamples:
    - input: "Plan the sprint" → knowledge_edge
    - input: "Groom the backlog" → knowledge_edge
    - input: "Align the product roadmap" → knowledge_edge
    - input: "Run standups" → knowledge_edge
    - input: "Facilitate retrospective" → knowledge_edge
    - input: "View burndown chart" → knowledge_edge
    - input: "Plan team capacity" → knowledge_edge
    - input: "Triage tickets" → knowledge_edge
    - input: "Prioritize epic backlog" → knowledge_edge
  NearestWrongSubPhaseScore for all: 0.0
  DomainId for all: "project-management"

---

## B36-0B-024-002 — Complete routing decision table: single-predicate gap cases

A: [Reference: what does the router produce for single-predicate B36 mixed inputs?]

B:
  SinglePredicateExamples:
    - predicates: ["QA"] → Phase 6, SubPhase 6.1, KnowledgeEdge: false
    - predicates: ["DOCUMENT"] + "story" → Phase 2, SubPhase 2.1, KnowledgeEdge: false
    - predicates: ["DOCUMENT"] + "requirements"/"criteria" → Phase 2, SubPhase 2.4
    - predicates: ["IMPLEMENT"] → Phase 4, SubPhase 4.1, KnowledgeEdge: false
    - predicates: ["ARCHITECTURE"] → Phase 3, SubPhase 3.4, KnowledgeEdge: false
    - predicates: ["TEST"] → Phase 5, SubPhase 5.1, KnowledgeEdge: false
  Rule: Gap vocabulary present in the sentence does not appear in EvidencePredicates.
  Rule: KnowledgeEdge: false for all single-predicate cases.
  Rule: DomainId: "software-development" for all single-predicate cases.

---

## B36-0B-024-003 — Multi-predicate routing: IMPLEMENT + DOCUMENT + QA

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-024-003-prompt"
  CompiledText: "This sprint: build the feature, document the requirements, and review the output."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  DecisionCount: 3
  KnowledgeEdge: false for all
  DomainId: "software-development"
  Note: QA is the last execution order (QA-last structural constraint).

---

## B36-0B-024-004 — Router decision DomainId rule summary

A: [Reference: what determines DomainId in router output?]

B:
  DomainIdRule:
    condition: "CompiledPredicateIds: []"
    result: "DomainId: 'project-management'"
    note: "Only when zero predicates; gap vocabulary signals project-management domain"

    condition: "Any predicate in CompiledPredicateIds"
    result: "DomainId: 'software-development'"
    note: "One predicate is sufficient to force software-development regardless of gap count"

  GapVocabularyEffect: "Gap vocabulary changes DomainId only when ALL predicates are absent.
    When any predicate fires, DomainId reverts to software-development unconditionally."

---

## B36-0B-024-005 — FailureClassification distinction: gap vs no-predicate

A: [Reference: when is FailureClassification 'no_predicate_match_capability_gap' vs 'no_predicate_match'?]

B:
  DistinctionTable:
    no_predicate_match_capability_gap:
      when: "CompiledPredicateIds is empty AND known gap vocabulary is present in the request"
      examples:
        - "Plan the sprint" (sprint is known gap vocabulary)
        - "Groom the backlog" (backlog is known gap vocabulary)
        - "Run the retrospective" (retrospective is known gap vocabulary)
      effect: "SCHEDULE_CAPABILITY_LEARNING emitted; gap IDs logged"

    no_predicate_match:
      when: "CompiledPredicateIds is empty AND no known gap vocabulary — completely unrecognized input"
      examples:
        - "Can you help me?" (no gap vocabulary, no predicate)
      effect: "Generic knowledge-edge; no SCHEDULE_CAPABILITY_LEARNING; no gap IDs"

  Rule: B36 requests always use 'no_predicate_match_capability_gap' because B36
    vocabulary is known project-planning gap vocabulary, not unrecognized input.
