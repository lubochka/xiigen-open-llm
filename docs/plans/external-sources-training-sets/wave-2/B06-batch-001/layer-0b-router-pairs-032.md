# Layer 0-B — Router Pairs (032)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-032-001 — ProductRecommendation: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-032-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the requirements for the product recommendation engine."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-032-002 — ProductRecommendation: DOCUMENT + stakeholder → 2.1 + 2.4

```
source_id: xiigen-b06-0b-032-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Write a feature-brief for the product recommendation engine for stakeholder review."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.4"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.88; KnowledgeEdge: false
  DecisionCount: 2
  Note: "stakeholder" signals 2.4. "Write" + "feature-brief" fire DOCUMENT + IMPLEMENT but "Write" alone adds IMPLEMENT.
  Correction: "Write a feature-brief" — "Write" fires IMPLEMENT. Full predicates: ["DOCUMENT", "IMPLEMENT"].
  RoutingDecisions add 4.1 as well. DecisionCount: 3 → [2.1, 2.4, 4.1]

---

## B06-0B-032-003 — ProductRecommendation: DOCUMENT + ARCHITECTURE → 2.1 + 3.1

```
source_id: xiigen-b06-0b-032-003
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  CompiledText: "Describe the recommendation system requirements and design the ML pipeline architecture."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "3"; PredictedSubPhaseId: "3.1"
      EvidencePredicates: ["ARCHITECTURE"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-032-004 — ProductRecommendation: knowledge-edge

```
source_id: xiigen-b06-0b-032-004
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: []
  CompiledText: "Show related products based on the current item category."
  KnowledgeEdge: true

B:
  RoutingDecisions: []
  DecisionCount: 0
  KnowledgeEdge: true
