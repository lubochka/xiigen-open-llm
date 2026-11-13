# Layer 0-B — Router Pairs (014)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 014 covers InventoryManagement routing — four pairs.

---

## B06-0B-014-001 — InventoryManagement: DOCUMENT alone → 2.1

```
source_id: xiigen-b06-0b-014-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the inventory management requirements for the checkout flow."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: DOCUMENT alone → 2.1. No competing predicate terms.

---

## B06-0B-014-002 — InventoryManagement: DOCUMENT+IMPLEMENT → 2.1+4.1

```
source_id: xiigen-b06-0b-014-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the inventory reservation spec."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-014-003 — InventoryManagement: DOCUMENT high-signal → 2.1

```
source_id: xiigen-b06-0b-014-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Describe the functional behavior of inventory reservation during checkout."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: "describe" + "functional" + "behavior" — three DOCUMENT terms, high signal density → 2.1.

---

## B06-0B-014-004 — InventoryManagement: DOCUMENT epics+capability → 2.1

```
source_id: xiigen-b06-0b-014-004
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Specify the epics and capability requirements for real-time inventory tracking."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: "epics" + "capability" + "requirements" → strong 2.1 signal.
    "epics" does not route to a separate phase — it is a DOCUMENT term routing to 2.1.
