# Layer 0-B — Router Pairs (030)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-030-001 — DropshippingRules: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-030-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the dropshipping fulfillment rules including supplier routing and split shipment policies."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-030-002 — DropshippingRules: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-030-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the dropshipping rules specification and create the supplier selection module."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-030-003 — DropshippingRules: IMPLEMENT → 4.1

```
source_id: xiigen-b06-0b-030-003
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT"]
  CompiledText: "Develop the supplier order routing service with automatic failover."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.93; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-030-004 — DropshippingRules: knowledge-edge routing

```
source_id: xiigen-b06-0b-030-004
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: []
  CompiledText: "Route orders to the nearest available dropship supplier."
  KnowledgeEdge: true

B:
  RoutingDecisions: []
  DecisionCount: 0
  KnowledgeEdge: true
  Teaching: "Route" is operational verb. No predicate fires. No routing decision produced.
