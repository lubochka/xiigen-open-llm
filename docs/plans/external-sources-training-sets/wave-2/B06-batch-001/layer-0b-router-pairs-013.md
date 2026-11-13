# Layer 0-B — Router Pairs (013)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 013 covers ShippingRequirements routing — four pairs showing DOCUMENT routing
to 2.1 and DOCUMENT+IMPLEMENT routing to 2.1+4.1.

---

## B06-0B-013-001 — ShippingRequirements: DOCUMENT alone → 2.1

```
source_id: xiigen-b06-0b-013-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the shipping requirements for the checkout flow."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: DOCUMENT alone routes to 2.1 (Business Requirements and Use Cases).
    No stakeholder signal present — 2.4 not triggered.

---

## B06-0B-013-002 — ShippingRequirements: DOCUMENT+IMPLEMENT → 2.1+4.1

```
source_id: xiigen-b06-0b-013-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the shipping specification for the carrier API integration."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2
  Routing rationale: DOCUMENT → 2.1; IMPLEMENT → 4.1. Two independent decisions.

---

## B06-0B-013-003 — ShippingRequirements: DOCUMENT strong signal → 2.1

```
source_id: xiigen-b06-0b-013-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Describe the use cases for shipping address validation and carrier selection."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: "describe" + "use cases" → strong 2.1 signal. NearestWrongSubPhaseScore: 0.38.

---

## B06-0B-013-004 — ShippingRequirements: DOCUMENT with specification → 2.1

```
source_id: xiigen-b06-0b-013-004
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Specify the functional criteria for shipping cost calculation."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: "specify" + "functional" + "criteria" → 2.1 primary. No IMPLEMENT verb.
