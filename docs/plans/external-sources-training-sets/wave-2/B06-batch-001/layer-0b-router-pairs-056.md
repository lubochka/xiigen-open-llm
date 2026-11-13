# Layer 0-B — Router Pairs (056)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-056-001 — PriceMatchPolicy: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-056-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the price match policy requirements including eligible competitor sources and verification window."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-056-002 — PriceMatchPolicy: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-056-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Specify the price match policy specification and build the price adjustment service."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-056-003 — PriceMatchPolicy: DOCUMENT + ARCHITECTURE → 2.1 + 3.1

```
source_id: xiigen-b06-0b-056-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  CompiledText: "Describe the price match policy requirements and design the competitor price verification architecture."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "3"; PredictedSubPhaseId: "3.1"
      EvidencePredicates: ["ARCHITECTURE"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-056-004 — PriceMatchPolicy: knowledge-edge routing

```
source_id: xiigen-b06-0b-056-004
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: []
  CompiledText: "Match the competitor price when the customer submits a valid price match request."
  KnowledgeEdge: true

B:
  RoutingDecisions: []
  DecisionCount: 0
  KnowledgeEdge: true
  Reason: No predicate fires. Request is escalated to knowledge-edge handler.
