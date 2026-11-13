# Held-Out and Corruption Pairs (016)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (016)

---

### Held-Out Pair B01-HO-016-001 — SearchResults rephrased routes to 1.1 + IMPLEMENT

```
source_id: xiigen-b01-held-out-016-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-016-001"
  user_request: "Build the search form and implement the filter panel component with responsive layout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.94
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "form" fires UI. "component" fires UI. "responsive" fires UI.
  "Build" fires IMPLEMENT. "implement" fires IMPLEMENT.
  Two predicates → two routing decisions. TransitionType: feature_addition.

---

### Held-Out Pair B01-HO-016-002 — "card sorting" → knowledge-edge

```
source_id: xiigen-b01-held-out-016-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-016-002"
  user_request: "Run a card sorting exercise to determine the best categorization for our search results."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.62
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "card sorting" — UX research methodology; not in any predicate lexicon.
  "categorization", "results" — no predicate terms.
  Empty predicates → knowledge-edge. Model must not infer UI from UX research context.

---

## Section 2 — Corruption Pairs (016)

---

### Corruption Pair B01-COR-016-001 — v6-01: "form" missed, falsely routed to knowledge-edge

```
source_id: xiigen-b01-corruption-016-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  CompiledText: "Design a search form with filter panel component."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false

B-corrupt:
  CompiledPredicateIds: []
  RoutingDecisions:
    - KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Corruption type: Corruption-v6-01 — "form" and "component" are UI lexicon terms; routing to knowledge-edge when UI is present is invalid.
