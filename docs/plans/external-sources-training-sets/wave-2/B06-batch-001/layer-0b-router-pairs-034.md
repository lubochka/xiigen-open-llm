# Layer 0-B — Router Pairs (034)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-034-001 — SeasonalPromotion: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-034-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Describe the seasonal promotion rules including campaign scheduling."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-034-002 — SeasonalPromotion: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-034-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Define the seasonal promotion requirements and build the campaign scheduling engine."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-034-003 — SeasonalPromotion: knowledge-edge

```
source_id: xiigen-b06-0b-034-003
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: []
  CompiledText: "Activate the Black Friday campaign discounts across all product categories."
  KnowledgeEdge: true

B:
  RoutingDecisions: []
  DecisionCount: 0
  KnowledgeEdge: true

---

## B06-0B-034-004 — SeasonalPromotion: DOCUMENT routing — domain breadth does not affect sub-phase

```
source_id: xiigen-b06-0b-034-004
source_license_state: first_party
pair_kind: router
```

Routing rationale for multi-domain DOCUMENT requests:
  "Define, describe, and specify requirements across gift card, referral, flash sale, bundle pricing, wholesale discounts, dropshipping, abandoned cart, recommendations, cross-sell, seasonal promotions, and digital delivery."
  → Still routes to 2.1 only. Domain breadth (11 domains) does not change sub-phase.
  → DOCUMENT → 2.1 regardless of how many domains are in scope.
  This mirrors the rule established in file 024 for the original 10 new domains.
