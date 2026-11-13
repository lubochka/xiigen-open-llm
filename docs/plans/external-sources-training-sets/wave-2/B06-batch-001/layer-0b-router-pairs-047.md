# Layer 0-B — Router Pairs (047)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## Method note

File 047 covers multi-domain routing for the IMPLEMENT+DOCUMENT combo scenarios in files 037-048.

---

## B06-0B-047-001 — Multi-domain DOCUMENT sweep → 2.1

```
source_id: xiigen-b06-0b-047-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define and specify all requirements across customer segmentation, dynamic pricing, geofenced pricing, volume discounts, membership tiers, coupon stacking, free shipping, minimum order, backorders, and preorders."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: Domain breadth does not change sub-phase. DOCUMENT → 2.1 regardless of domain count.

---

## B06-0B-047-002 — Multi-domain DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-047-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the requirements for all Round 3 domains and build all corresponding services."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-047-003 — Routing decision count table for files 037-048

```
source_id: xiigen-b06-0b-047-003
source_license_state: first_party
pair_kind: router
```

Routing decision count by file and scenario:

  File | Scenario                    | DecisionCount | Sub-phases
  -----|-----------------------------|--------------|-----------
  037  | CustomerSegmentation        | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  038  | DynamicPricing              | 1, 2, or 3   | 2.1 / 2.1+4.1 / 2.1+3.1+4.1
  039  | GeofencedPricing            | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  040  | VolumeDiscount              | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  041  | MembershipTiers             | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  042  | CouponStackingRules         | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  043  | FreeShippingThreshold       | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  044  | MinimumOrderValue           | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  045  | BackorderManagement         | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  046  | PreorderRules               | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  047  | Multi-domain sweep          | 1 or 2       | 2.1 / 2.1+4.1
  048  | TransitionType reference    | 1             | 2.1

  Note: No 2.4 cases in Round 3 (no "stakeholder" signal in these domains).

---

## B06-0B-047-004 — Routing anti-patterns for Round 3 domains

```
source_id: xiigen-b06-0b-047-004
source_license_state: first_party
pair_kind: router
```

Routing anti-patterns for Round 3 domains:

  AP-R13: Routing customer segmentation to a "CRM sub-phase."
    Correct: No CRM sub-phase exists. DOCUMENT → 2.1. IMPLEMENT → 4.1.

  AP-R14: Routing dynamic pricing to "pricing sub-phase."
    Correct: No pricing sub-phase. DOCUMENT → 2.1. IMPLEMENT → 4.1.

  AP-R15: Routing membership tiers to "loyalty sub-phase."
    Correct: No loyalty sub-phase. DOCUMENT → 2.1. IMPLEMENT → 4.1.

  AP-R16: Adding 2.4 for coupon stacking because "rules" sounds like "stakeholder criteria."
    Correct: "rules" is not a stakeholder trigger. 2.4 requires explicit "stakeholder" term.

  AP-R17: Adding a separate sub-phase for backorder or preorder fulfillment.
    Correct: No fulfillment sub-phase. IMPLEMENT → 4.1 only.
