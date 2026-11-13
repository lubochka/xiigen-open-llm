# Held-Out and Corruption Pairs (019)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (019)

---

### Held-Out Pair B01-HO-019-001 — OrderHistoryPage rephrased routes to 1.1 + DOCUMENT

```
source_id: xiigen-b01-held-out-019-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-019-001"
  user_request: "Document all the screen states in the order history and tracking view, with accessibility requirements for each."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.93
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Generalization test:
  "screen" fires UI. "accessibility" fires UI.
  "Document" fires DOCUMENT. "requirements" fires DOCUMENT.
  Two predicates → two routing decisions. TransitionType: documentation.

---

### Held-Out Pair B01-HO-019-002 — "delivery ETA" → knowledge-edge

```
source_id: xiigen-b01-held-out-019-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-019-002"
  user_request: "Show the estimated delivery time for each order in the tracking view."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.61
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "show", "estimated", "time", "tracking" — not in any predicate lexicon.
  Clear product feature intent but no lexicon matches. Empty predicates → knowledge-edge.

---

## Section 2 — Corruption Pairs (019)

---

### Corruption Pair B01-COR-019-001 — v6-03: DOCUMENT-only collapsed into 1.1

```
source_id: xiigen-b01-corruption-019-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-03-multi-branch-result-collapse
```

A:
  CompiledText: "Document the functional requirements for the order history screen."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI", "DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 1

Corruption type: Corruption-v6-03 — DOCUMENT predicate merged into 1.1. Sub-phase 2.1 is never selected; the documentation work is lost.
