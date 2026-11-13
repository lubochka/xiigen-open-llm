# Layer 0-B — Router Pairs (060)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## Method note

File 060 is the complete routing rule reference for files 049-060 (Round 4 domains).

---

## B06-0B-060-001 — Multi-domain DOCUMENT sweep → 2.1

```
source_id: xiigen-b06-0b-060-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define and specify all requirements across return window policy, warranty tracking, split payment, installment plans, store credit, wishlist sharing, product bundle creator, price match, stock alerts, auto-replenishment, and VIP access."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: Domain breadth does not change sub-phase. DOCUMENT → 2.1 regardless of domain count.

---

## B06-0B-060-002 — Routing decision count table for files 049-060

```
source_id: xiigen-b06-0b-060-002
source_license_state: first_party
pair_kind: router
```

Routing decision count by file and scenario:

  File | Scenario                    | DecisionCount | Sub-phases
  -----|-----------------------------|--------------|-----------
  049  | ReturnWindowPolicy          | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  050  | WarrantyTracking            | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  051  | SplitPayment                | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  052  | InstallmentPlan             | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  053  | StoreCreditManagement       | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  054  | WishlistSharing             | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  055  | ProductBundleCreator        | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  056  | PriceMatchPolicy            | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  057  | StockAlertSystem            | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  058  | AutoReplenishment           | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  059  | VIPAccess                   | 1, 2, or 2   | 2.1 / 2.1+4.1 / 2.1+3.1
  060  | Multi-domain sweep          | 1             | 2.1

  Note: No 2.4 cases in Round 4 (no "stakeholder" signal in these domains).

---

## B06-0B-060-003 — Complete routing anti-patterns for Round 4 domains

```
source_id: xiigen-b06-0b-060-003
source_license_state: first_party
pair_kind: router
```

Routing anti-patterns for Round 4 domains:

  AP-R18: Routing return window to "returns sub-phase."
    Correct: No returns sub-phase. DOCUMENT → 2.1. IMPLEMENT → 4.1.

  AP-R19: Routing warranty tracking to "warranty sub-phase."
    Correct: No warranty sub-phase. DOCUMENT → 2.1 for requirements.

  AP-R20: Routing split payment or installment to "payment sub-phase."
    Correct: No payment sub-phase exists in the sub-phase map.

  AP-R21: Routing store credit issuance to "credit sub-phase."
    Correct: No credit sub-phase. DOCUMENT → 2.1.

  AP-R22: Routing VIP access to "access control sub-phase" or "security sub-phase."
    Correct: No such sub-phase. DOCUMENT → 2.1. IMPLEMENT → 4.1.

---

## B06-0B-060-004 — Full batch routing invariant confirmation

```
source_id: xiigen-b06-0b-060-004
source_license_state: first_party
pair_kind: router
```

Routing invariants confirmed for all of B06-batch-001 (files 002-060):

  Sub-phase map is fixed: 2.1, 2.4, 3.1, 4.1, 1.1 (unchanged across all domains and rounds).
  DOCUMENT → 2.1 (required for all domains).
  IMPLEMENT → 4.1 (required for all domains).
  ARCHITECTURE → 3.1 (required for all domains).
  UI → 1.1 (required when present).
  "stakeholder" → adds 2.4 to 2.1 (for the 4 domains with stakeholder signals).
  DomainId: "software-development" confirmed for all 35+ domains.
  No new sub-phases have been added or will be added regardless of domain.
