# Layer 0-B — Router Pairs (015)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 015 covers LoyaltyProgram routing — four pairs, including the stakeholder
dual-route case (DOCUMENT → 2.1 + 2.4).

---

## B06-0B-015-001 — LoyaltyProgram: DOCUMENT alone → 2.1

```
source_id: xiigen-b06-0b-015-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the loyalty program requirements for checkout."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-015-002 — LoyaltyProgram: DOCUMENT+stakeholder → 2.1+2.4

```
source_id: xiigen-b06-0b-015-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Describe the stakeholder behavior expectations for loyalty program checkout."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.4"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.87; KnowledgeEdge: false
  DecisionCount: 2
  Routing rationale: "stakeholder" signal in DOCUMENT predicate triggers 2.4 in addition to 2.1.
    One predicate → two routing decisions. Corruption-v6-03: router must NOT collapse to one.

---

## B06-0B-015-003 — LoyaltyProgram: DOCUMENT+IMPLEMENT → 2.1+4.1

```
source_id: xiigen-b06-0b-015-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the user stories for loyalty program integration."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-015-004 — LoyaltyProgram: DOCUMENT narrative+feature-brief → 2.1

```
source_id: xiigen-b06-0b-015-004
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Specify the feature-brief for loyalty rewards at checkout, including acceptance criteria."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: "feature-brief" + "criteria" → 2.1. No stakeholder signal → 2.4 not triggered.
