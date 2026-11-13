# Layer 0-B — Router Pairs
# Batch: B15-batch-001
# Scenario: B15 — Development from scratch — backend order processing service
# Contract: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B15B1-WDA-router-001 — [IMPLEMENT] → 4.1

```
source_id: xiigen-b15-router-001
pair_kind: router
```

A:
  DocumentId: "B15-compiler-001-prompt"
  CompiledText: "Build an order processing service from scratch including order creation, status tracking, and fulfillment logic."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B15-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Routing rationale: IMPLEMENT → Phase 4. "service" + "logic" + "build" point to 4.1
(Backend Services and Business Logic). 4.2 (Integration) is nearest wrong (NearestWrongSubPhaseScore 0.40).

---

## Pair B15B1-WDA-router-002 — [IMPLEMENT, DOCUMENT, API_CONTRACT, ARCHITECTURE] → four decisions

```
source_id: xiigen-b15-router-002
pair_kind: router
```

A:
  DocumentId: "B15-compiler-002-prompt"
  CompiledText: "Write the order service from scratch: implement the domain model, define the data contracts, and code the business rules."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "API_CONTRACT", "ARCHITECTURE"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B15-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B15-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B15-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B15-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 4

Teaching point: Four predicates → four independent routing decisions. The router
does not select the "most important" one — it produces one decision per predicate.

---

## Pair B15B1-WDA-router-003 — [IMPLEMENT] high-density → 4.1 with high confidence

```
source_id: xiigen-b15-router-003
pair_kind: router
```

A:
  DocumentId: "B15-compiler-003-prompt"
  CompiledText: "Create the OrderRepository class and implement the CreateOrder, UpdateOrderStatus, and CancelOrder methods."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B15-compiler-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.95
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Note: Confidence 0.95 (vs standard 0.92) reflects high-density IMPLEMENT signal:
create, class, implement, method — all four terms fire IMPLEMENT.
