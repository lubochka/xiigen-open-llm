# Layer 0-B — Router Pairs (027)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-027-001 — FlashSaleEngine: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-027-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Describe the requirements for a flash sale engine."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-027-002 — FlashSaleEngine: DOCUMENT + ARCHITECTURE + IMPLEMENT → 2.1 + 3.1 + 4.1

```
source_id: xiigen-b06-0b-027-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  CompiledText: "Write the flash sale specification, design the scheduling architecture, and build the inventory hold service."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "3"; PredictedSubPhaseId: "3.1"
      EvidencePredicates: ["ARCHITECTURE"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 3
  CanonicalOrdering: 2.1 → 3.1 → 4.1

---

## B06-0B-027-003 — FlashSaleEngine: knowledge-edge → no routing

```
source_id: xiigen-b06-0b-027-003
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: []
  CompiledText: "Run a flash sale for electronics at 50% off."
  KnowledgeEdge: true

B:
  RoutingDecisions: []
  DecisionCount: 0
  KnowledgeEdge: true

---

## B06-0B-027-004 — FlashSaleEngine: canonical ordering enforcement

```
source_id: xiigen-b06-0b-027-004
source_license_state: first_party
pair_kind: router
```

Canonical ordering rule for three-predicate routing decisions:
  Priority: 2.1 (highest) → 3.1 → 4.1 (lowest)
  Correct: [2.1, 3.1, 4.1]
  Corrupt (v6-03): [4.1, 2.1, 3.1] or [3.1, 4.1, 2.1]

  This ordering applies regardless of the order predicates appear in the request.
  "build the inventory hold service and write the specification and design the architecture"
  → routing is still [2.1, 3.1, 4.1] in canonical order.
