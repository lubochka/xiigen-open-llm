# Layer 0-B — Router Pairs (044)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-044-001 — MinimumOrderValue: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-044-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Describe the minimum order value requirements including per-channel thresholds and cart rejection behavior."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-044-002 — MinimumOrderValue: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-044-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the minimum order value specification and build the cart validation service."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-044-003 — MinimumOrderValue: DOCUMENT + ARCHITECTURE → 2.1 + 3.1

```
source_id: xiigen-b06-0b-044-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  CompiledText: "Define the minimum order value requirements and design the cart validation service architecture."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "3"; PredictedSubPhaseId: "3.1"
      EvidencePredicates: ["ARCHITECTURE"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-044-004 — MinimumOrderValue: knowledge-edge routing

```
source_id: xiigen-b06-0b-044-004
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: []
  CompiledText: "Block checkout when the cart total is below the minimum."
  KnowledgeEdge: true

B:
  RoutingDecisions: []
  DecisionCount: 0
  KnowledgeEdge: true
  Reason: No predicate fires. Request is escalated to knowledge-edge handler.
