# Layer 0-B — Router Pairs (026)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-026-001 — ReferralProgram: DOCUMENT + stakeholder → 2.1 + 2.4

```
source_id: xiigen-b06-0b-026-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Write a feature-brief for the referral program for stakeholder review."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.4"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.88; KnowledgeEdge: false
  DecisionCount: 2
  Note: "stakeholder" triggers 2.4 in addition to 2.1. Still one predicate (DOCUMENT).

---

## B06-0B-026-002 — ReferralProgram: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-026-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the requirements for the referral program and develop the reward crediting module."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-026-003 — ReferralProgram: IMPLEMENT → 4.1 only

```
source_id: xiigen-b06-0b-026-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT"]
  CompiledText: "Build the referral tracking service with invite link generation."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.93; KnowledgeEdge: false
  DecisionCount: 1
  Note: IMPLEMENT alone routes to 4.1 only. No 2.1 without DOCUMENT.

---

## B06-0B-026-004 — ReferralProgram: routing anti-pattern

```
source_id: xiigen-b06-0b-026-004
source_license_state: first_party
pair_kind: router
```

Anti-pattern AP-R6: Routing "stakeholder" mention to 2.4 as the sole routing decision.
  Correct: "stakeholder" always adds 2.4 IN ADDITION to 2.1. Never 2.4 alone.
  Input: CompiledPredicateIds: ["DOCUMENT"], "stakeholder" present
  Correct: DecisionCount: 2 → [2.1, 2.4]
  Corrupt: DecisionCount: 1 → [2.4 only]

Anti-pattern AP-R7: Adding 2.4 when "stakeholder" is absent.
  Correct: 2.4 is only added when the word "stakeholder" appears in the request.
  "reward accrual for partners" — no "stakeholder" word → 2.4 NOT triggered.
