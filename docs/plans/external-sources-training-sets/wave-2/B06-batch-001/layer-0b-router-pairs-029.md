# Layer 0-B — Router Pairs (029)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-029-001 — WholesaleDiscount: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-029-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Describe the wholesale discount tiers and account-level pricing agreements."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-029-002 — WholesaleDiscount: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-029-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Specify wholesale discount requirements and build the tiered pricing engine."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-029-003 — WholesaleDiscount: ARCHITECTURE → 3.1

```
source_id: xiigen-b06-0b-029-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE"]
  CompiledText: "Design the pricing service architecture for wholesale account segmentation."

B:
  RoutingDecisions:
    - PredictedPhaseId: "3"; PredictedSubPhaseId: "3.1"
      EvidencePredicates: ["ARCHITECTURE"]; Confidence: 0.93; KnowledgeEdge: false
  DecisionCount: 1
  Teaching: ARCHITECTURE alone → 3.1 only. No 2.1 without DOCUMENT. No 4.1 without IMPLEMENT.

---

## B06-0B-029-004 — WholesaleDiscount: routing count table for ARCHITECTURE combinations

```
source_id: xiigen-b06-0b-029-004
source_license_state: first_party
pair_kind: router
```

ARCHITECTURE routing decision count reference:
  ARCHITECTURE alone → DecisionCount: 1 → [3.1]
  DOCUMENT + ARCHITECTURE → DecisionCount: 2 → [2.1, 3.1]
  ARCHITECTURE + IMPLEMENT → DecisionCount: 2 → [3.1, 4.1]
  DOCUMENT + ARCHITECTURE + IMPLEMENT → DecisionCount: 3 → [2.1, 3.1, 4.1]

  Anti-pattern: treating "architecture" domain noun as ARCHITECTURE predicate.
  "design the pricing architecture" → ARCHITECTURE fires (design + architecture).
  "pricing architecture document" → only DOCUMENT fires (document fires DOCUMENT, "architecture" is context noun here only if "design" absent).
  Note: "architecture" as standalone noun fires ARCHITECTURE via Rule A-02.
