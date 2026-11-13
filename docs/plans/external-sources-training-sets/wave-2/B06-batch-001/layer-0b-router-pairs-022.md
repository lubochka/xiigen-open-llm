# Layer 0-B — Router Pairs (022)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 022 covers DataPrivacy routing — four pairs, including the stakeholder
dual-route case (DOCUMENT → 2.1 + 2.4).

---

## B06-0B-022-001 — DataPrivacy: DOCUMENT alone → 2.1

```
source_id: xiigen-b06-0b-022-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the data privacy requirements for the checkout flow."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-022-002 — DataPrivacy: DOCUMENT+stakeholder → 2.1+2.4

```
source_id: xiigen-b06-0b-022-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Specify the stakeholder requirements for data privacy in checkout."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.4"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.87; KnowledgeEdge: false
  DecisionCount: 2
  Routing rationale: "stakeholder" in DOCUMENT predicate triggers both 2.1 and 2.4.
    Both routing decisions must be produced. v6-03 corruption collapses to one — must not happen.

---

## B06-0B-022-003 — DataPrivacy: DOCUMENT+IMPLEMENT → 2.1+4.1

```
source_id: xiigen-b06-0b-022-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the data privacy specification for checkout event logging."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-022-004 — DataPrivacy: DOCUMENT use-case → 2.1

```
source_id: xiigen-b06-0b-022-004
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Describe the use cases for privacy consent collection at checkout."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
