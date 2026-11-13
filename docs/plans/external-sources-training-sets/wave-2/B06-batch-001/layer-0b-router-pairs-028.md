# Layer 0-B — Router Pairs (028)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-028-001 — BundlePricing: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-028-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the bundle pricing rules including fixed-price bundles and buy-one-get-one scenarios."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-028-002 — BundlePricing: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-028-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Document the bundle pricing specification and develop the discount computation engine."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-028-003 — BundlePricing: "narrative" + "Write" → DOCUMENT + IMPLEMENT routing

```
source_id: xiigen-b06-0b-028-003
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write a narrative description of bundle pricing rules for the product team."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2
  Teaching: "narrative" fires DOCUMENT (Rule 13); "Write" fires IMPLEMENT simultaneously. Both route.

---

## B06-0B-028-004 — BundlePricing: IMPLEMENT alone → 4.1 only

```
source_id: xiigen-b06-0b-028-004
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT"]
  CompiledText: "Create the bundle price calculator with nested discount support."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.93; KnowledgeEdge: false
  DecisionCount: 1
  Teaching: IMPLEMENT alone → 4.1 only. No 2.1 without DOCUMENT predicate.
