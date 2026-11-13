# Layer 0-B — Router Pairs (024)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 024 is the complete routing rule reference for files 013-024,
covering all new domains and routing scenarios introduced in sub-phase 2.1.

---

## B06-0B-024-001 — Multi-domain DOCUMENT sweep → 2.1

```
source_id: xiigen-b06-0b-024-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define, describe, and specify all requirements across shipping, inventory, loyalty, return and refund, tax, multi-currency, bundle, subscription, fraud, and data privacy."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: Domain breadth does not change sub-phase. DOCUMENT → 2.1 regardless of domain scope.

---

## B06-0B-024-002 — Routing decision count table for files 013-024

```
source_id: xiigen-b06-0b-024-002
source_license_state: first_party
pair_kind: router
```

Routing decision count by file and scenario:

  File | Scenario                    | DecisionCount | Sub-phases
  -----|-----------------------------|--------------|-----------
  013  | ShippingRequirements        | 1 or 2        | 2.1 / 2.1+4.1
  014  | InventoryManagement         | 1 or 2        | 2.1 / 2.1+4.1
  015  | LoyaltyProgram              | 1, 2, or 2    | 2.1 / 2.1+2.4 / 2.1+4.1
  016  | ReturnAndRefund             | 1 or 2        | 2.1 / 2.1+4.1
  017  | TaxCalculation              | 1 or 2        | 2.1 / 2.1+4.1
  018  | MultiCurrency               | 1 or 2        | 2.1 / 2.1+4.1
  019  | BundleAndDiscount           | 1 or 2        | 2.1 / 2.1+4.1
  020  | SubscriptionBilling         | 1 or 2        | 2.1 / 2.1+4.1
  021  | FraudDetection              | 1 or 2        | 2.1 / 2.1+4.1
  022  | DataPrivacy                 | 1, 2, or 2    | 2.1 / 2.1+2.4 / 2.1+4.1
  023  | DOCUMENT+IMPLEMENT sweep    | 2             | 2.1+4.1 (all pairs)
  024  | Multi-domain sweep          | 1 or 2        | varies

  Key rule: "stakeholder" signal → 2.4 added to routing decisions.
  Key rule: IMPLEMENT verb → 4.1 added to routing decisions.

---

## B06-0B-024-003 — Routing anti-pattern reference for files 013-024

```
source_id: xiigen-b06-0b-024-003
source_license_state: first_party
pair_kind: router
```

Routing anti-patterns for new domains:

  AP-R1: Routing shipping requirements to 4.1 without IMPLEMENT predicate.
    Correct: DOCUMENT alone → 2.1 only.

  AP-R2: Routing fraud detection requirements to a security sub-phase.
    Correct: No security sub-phase exists in 2.1 flow. DOCUMENT → 2.1 only.
    Note: Security concerns route to 2.1 as non-functional or business requirements.

  AP-R3: Routing data privacy with "stakeholder" to 2.1 only.
    Correct: "stakeholder" → 2.1 AND 2.4. Both decisions required.

  AP-R4: Routing subscription billing epics to a separate sub-phase.
    Correct: "epics" is a DOCUMENT term → 2.1 only.

  AP-R5: Producing DecisionCount: 0 for any DOCUMENT predicate request.
    Correct: DOCUMENT always routes to at least 2.1. KnowledgeEdge: false for
    all requests with at least one DOCUMENT term.

---

## B06-0B-024-004 — 2.1 vs 2.4 disambiguation for new domains

```
source_id: xiigen-b06-0b-024-004
source_license_state: first_party
pair_kind: router
```

2.1 vs 2.4 disambiguation rule for new domains:

  2.1 (Business Requirements and Use Cases):
    - All DOCUMENT requests route here first
    - Shipping requirements → 2.1
    - Inventory requirements → 2.1
    - Tax requirements → 2.1
    - Fraud detection requirements → 2.1
    - Data privacy requirements (without stakeholder) → 2.1

  2.4 (External Stakeholder Requirements):
    - Added only when "stakeholder" term is present in request
    - Loyalty stakeholder behavior → 2.1 + 2.4
    - Data privacy stakeholder requirements → 2.1 + 2.4
    - Shipping carrier stakeholder requirements → 2.1 + 2.4 (if "stakeholder" present)

  Decision rule: 2.4 is never the sole routing decision. It is always added to 2.1.
  One predicate → multiple routing decisions is valid — this is not corruption.
