# Layer 0-B — Router Pairs
# Batch: B12-batch-001
# Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Contract: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B12B1-WDA-router-001 — [API_CONTRACT, DOCUMENT, SECURITY] → three decisions

```
source_id: xiigen-b12-router-001
pair_kind: router
```

A:
  DocumentId: "B12-compiler-001-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B12-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B12-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B12-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["SECURITY"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Routing rationale: API_CONTRACT → 3.3 (API Contract Design). DOCUMENT with "requirements"
→ 2.4 (External Stakeholder Requirements, because authentication requirements is an external
interface concern). SECURITY → 3.4 (Security and Compliance Design).

---

## Pair B12B1-WDA-router-002 — [API_CONTRACT, IMPLEMENT, DOCUMENT] → three decisions

```
source_id: xiigen-b12-router-002
pair_kind: router
```

A:
  DocumentId: "B12-compiler-002-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B12-compiler-002-prompt"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B12-compiler-002-prompt"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B12-compiler-002-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Note: IMPLEMENT → 4.2 (Integration and External Services) because "write" + "integration"
+ "payment gateway" signals integration work, not pure backend (4.1). 4.1 is nearest wrong.

---

## Pair B12B1-WDA-router-003 — [API_CONTRACT, ARCHITECTURE, DOCUMENT] → three decisions

```
source_id: xiigen-b12-router-003
pair_kind: router
```

A:
  DocumentId: "B12-compiler-003-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B12-compiler-003-prompt"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B12-compiler-003-prompt"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B12-compiler-003-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3
