# Held-Out and Corruption Pairs (018)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (018)

---

### Held-Out Pair B01-HO-018-001 — WishlistPage rephrased routes to 1.1

```
source_id: xiigen-b01-held-out-018-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-018-001"
  user_request: "Map all the visual states of the wishlist page including the empty state, populated list, and move-to-cart modal component."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI"]
  Confidence: 0.95
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false

Generalization test:
  "visual" fires UI. "page" fires UI. "modal" fires UI. "component" fires UI.
  Four UI hits. "wishlist", "empty state", "populated list" do not fire independently.
  Strong UI signal routes to 1.1.

---

### Held-Out Pair B01-HO-018-002 — wishlist sharing → knowledge-edge

```
source_id: xiigen-b01-held-out-018-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-018-002"
  user_request: "Allow users to share their wishlist via a unique link to friends and family."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.63
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "share", "link", "unique", "friends", "family" — not in any predicate lexicon.
  "wishlist" alone does not fire. Empty predicates → knowledge-edge.

---

## Section 2 — Corruption Pairs (018)

---

### Corruption Pair B01-COR-018-001 — v6-01: "accessibility" missed, falsely to knowledge-edge

```
source_id: xiigen-b01-corruption-018-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  CompiledText: "Document the accessibility requirements for the wishlist page."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Corruption type: Corruption-v6-01 — "accessibility" is in the UI lexicon; dropping the UI predicate silently omits the 1.1 routing decision.
