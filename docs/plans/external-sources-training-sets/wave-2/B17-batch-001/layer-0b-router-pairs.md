# Layer 0-B — Router Pairs
# Batch: B17-batch-001
# Scenario: B17 — Development — integration — third-party payment connector
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B17B1-WDA-router-001 — [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b17-router-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  CompiledText: "Build the Stripe payment integration connector — implement the charge, refund, and webhook handlers."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false

    - PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false

  DecisionCount: 2

Note: IMPLEMENT → 4.3 (Integration, not 4.1) because "integration connector" + "webhook"
signals an external service integration, not a pure backend service. 4.1 is nearest wrong.

---

## Pair B17B1-WDA-router-002 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b17-router-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  CompiledText: "Integrate the SendGrid email API and document the request interface for sending transactional emails."

B:
  RoutingDecisions:
    - PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false

    - PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false

  DecisionCount: 2

---

## Pair B17B1-WDA-router-003 — [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] → four decisions

```
source_id: xiigen-b17-router-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      EvidencePredicates: ["IMPLEMENT"]
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      KnowledgeEdge: false

    - PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      EvidencePredicates: ["API_CONTRACT"]
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      KnowledgeEdge: false

    - PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      EvidencePredicates: ["SECURITY"]
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      KnowledgeEdge: false

    - PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      EvidencePredicates: ["AUTHORIZATION"]
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      KnowledgeEdge: false

  DecisionCount: 4

Note: Both SECURITY and AUTHORIZATION route to 3.4 (Security and Compliance Design).
Two distinct predicates can route to the same sub-phase — this is valid. DecisionCount: 4
because four predicates fired, even if two sub-phases overlap.
