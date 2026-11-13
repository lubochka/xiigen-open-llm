# Layer 0-B — Router Pairs
# Batch: B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Contract: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B36B1-WDA-router-001 — zero predicates → knowledge-edge routing

```
source_id: xiigen-b36-router-001
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: unknown
coverage_status: knowledge_edge
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from B36B1-WDA-compiler-001 B output]
  DocumentId: "B36-compiler-001-prompt"
  CompiledText: "We need to plan the next sprint and update the roadmap with the new milestones for Q3."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B36-compiler-001-prompt"
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
  CompiledPredicateIds is empty. The router cannot select any registered sub-phase.
  KnowledgeEdge: true is the mandatory output. The FailureClassification must be
  "no_predicate_match_capability_gap" (not just "no_predicate_match") because the
  input contains known capability gap vocabulary (sprint, roadmap, milestone).
  NearestWrongSubPhaseScore: 0.0 — no sub-phase is even close to matching.

---

## Pair B36B1-WDA-router-002 — DOCUMENT from mixed signal → routes to 2.4

```
source_id: xiigen-b36-router-002
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from B36B1-WDA-compiler-002 B output]
  DocumentId: "B36-compiler-002-prompt"
  CompiledText: "Define the quarterly roadmap and document the stakeholder requirements for each milestone in the product delivery plan."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  DOCUMENT predicate fires → routes to Phase 2 requirements sub-phases.
  "stakeholder requirements" makes 2.4 (External Stakeholder Requirements) the
  primary owner over 2.1 (Business Requirements and Use Cases). 2.1 is the nearest
  wrong sub-phase (NearestWrongSubPhaseScore 0.44). The capability gap vocabulary
  (roadmap, milestone) is not captured by the routing — it will be flagged in Layer 0-C.

---

## Pair B36B1-WDA-router-003 — IMPLEMENT from mixed signal → routes to 4.1

```
source_id: xiigen-b36-router-003
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from B36B1-WDA-compiler-003 B output]
  DocumentId: "B36-compiler-003-prompt"
  CompiledText: "Plan the sprint to implement the checkout feature and build the payment integration module."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B36-compiler-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  IMPLEMENT predicate (implement, build, feature) routes to 4.1 (Backend Services
  and Business Logic). The project planning vocabulary ("sprint", "plan") is a
  capability gap that does not alter the routing — the routing is driven entirely
  by the compiled predicates, not by gap vocabulary.
