# Layer 0-B — Router Pairs (031)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-031-001 — AbandonedCartRecovery: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-031-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Specify the requirements for abandoned cart recovery including email timing windows."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-031-002 — AbandonedCartRecovery: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-031-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the functional requirements for abandoned cart recovery and implement the notification scheduler."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-031-003 — AbandonedCartRecovery: "story" + "Write" → DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-031-003
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write a user story for abandoned cart recovery from the perspective of a returning customer."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 2
  Teaching: "story" fires DOCUMENT; "Write" fires IMPLEMENT → two routing decisions.

---

## B06-0B-031-004 — AbandonedCartRecovery: IMPLEMENT → 4.1

```
source_id: xiigen-b06-0b-031-004
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT"]
  CompiledText: "Build the abandoned cart email trigger with configurable delay intervals."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.93; KnowledgeEdge: false
  DecisionCount: 1
