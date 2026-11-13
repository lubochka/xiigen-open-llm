# Layer 0-B — Router Pairs
# Batch: B32-batch-001
# Scenario: B32 — Vibe coding gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B32B1-WDA-router-001 — zero predicates → knowledge-edge routing

```
source_id: xiigen-b32-router-001
pair_kind: router
```

A:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  RoutingDecisions:
    - SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1

---

## Pair B32B1-WDA-router-002 — [UI] → 1.1

```
source_id: xiigen-b32-router-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["UI"]
  CompiledText: "Vibe code a quick screen for the user profile — make the interface feel intuitive."

B:
  RoutingDecisions:
    - PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
  DecisionCount: 1

---

## Pair B32B1-WDA-router-003 — [IMPLEMENT] → 4.1

```
source_id: xiigen-b32-router-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT"]
  CompiledText: "Exploratory prototype — just build something quick for the order summary."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
  DecisionCount: 1
