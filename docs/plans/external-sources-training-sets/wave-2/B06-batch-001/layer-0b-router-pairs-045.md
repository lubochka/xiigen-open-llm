# Layer 0-B — Router Pairs (045)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-045-001 — BackorderManagement: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-045-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the backorder management requirements including customer notification rules and automatic fulfillment on restock."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-045-002 — BackorderManagement: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-045-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Specify the backorder management specification and build the fulfillment queue service."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-045-003 — BackorderManagement: DOCUMENT + ARCHITECTURE → 2.1 + 3.1

```
source_id: xiigen-b06-0b-045-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  CompiledText: "Describe the backorder management requirements and design the fulfillment queue architecture."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "3"; PredictedSubPhaseId: "3.1"
      EvidencePredicates: ["ARCHITECTURE"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-045-004 — BackorderManagement: knowledge-edge routing

```
source_id: xiigen-b06-0b-045-004
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: []
  CompiledText: "Queue the customer order when inventory is exhausted."
  KnowledgeEdge: true

B:
  RoutingDecisions: []
  DecisionCount: 0
  KnowledgeEdge: true
  Reason: No predicate fires. Request is escalated to knowledge-edge handler.
