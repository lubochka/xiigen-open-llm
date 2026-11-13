# Layer 0-B — Router Pairs
# Batch: B38-batch-001
# Scenario: B38 — Project delivery — release gates and go-live checklists
# Contract: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B38B1-WDA-router-001 — [QA, IMPLEMENT] → two decisions: 7.1 + 4.1

```
source_id: xiigen-b38-router-001
transition_type: feature_addition
coverage_status: covered
pair_kind: router
```

A:
  DocumentId: "B38-compiler-001-prompt"
  CompiledText: "Review the release checklist and approve the go-live for the payment service upgrade."
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B38-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B38-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  QA predicate ("review", "approve") → Phase 7 → 7.1 (Release and Deployment Automation).
  IMPLEMENT predicate ("service") → Phase 4 → 4.1 (Backend Services and Business Logic).
  Two independent routing decisions. The router does not collapse multi-predicate
  results into a single decision (anti-pattern COR-003 in B01 and B36).

---

## Pair B38B1-WDA-router-002 — [QA, DOCUMENT] → two decisions: 7.1 + 2.1

```
source_id: xiigen-b38-router-002
transition_type: feature_addition
coverage_status: covered
pair_kind: router
```

A:
  DocumentId: "B38-compiler-002-prompt"
  CompiledText: "Validate all release criteria and confirm the deployment is ready for the production cutover."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B38-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B38-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  QA ("validate") → 7.1. DOCUMENT ("criteria") → 2.1 (Business Requirements and
  Use Cases). "Criteria" is more aligned with 2.1 than 2.4 because it's about
  release criteria (exit conditions) rather than external stakeholder requirements.

---

## Pair B38B1-WDA-router-003 — zero predicates → knowledge-edge routing

```
source_id: xiigen-b38-router-003
transition_type: unknown
coverage_status: knowledge_edge
pair_kind: router
```

A:
  DocumentId: "B38-compiler-003-prompt"
  CompiledText: "Execute the production rollout and monitor the go-live deployment for any issues."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - DocumentId: "B38-compiler-003-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"

  DecisionCount: 1
