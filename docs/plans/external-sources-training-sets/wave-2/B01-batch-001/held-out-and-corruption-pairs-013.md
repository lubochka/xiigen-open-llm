# Held-Out and Corruption Pairs (013)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (013)

---

### Held-Out Pair B01-HO-013-001 — UserProfilePage rephrased routes to 1.1

```
source_id: xiigen-b01-held-out-013-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-013-001"
  user_request: "We need a complete visual inventory of every user profile page and account settings screen in our app."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI"]
  Confidence: 0.93
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false

Generalization test:
  "visual" fires UI. "page" fires UI. "screen" fires UI. Multiple UI hits confirm the predicate.
  "account settings" does not independently fire — the UI terms carry the routing.
  Model must route to 1.1 via UI predicate; "settings" alone is not a UI term.

---

### Held-Out Pair B01-HO-013-002 — navigation request → knowledge-edge

```
source_id: xiigen-b01-held-out-013-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-013-002"
  user_request: "Map all the navigation paths between sections in our onboarding experience."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.68
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "navigation" — critical boundary. "navigation" is NOT in the UI lexicon.
  "paths", "sections", "experience" — no predicate terms.
  Empty CompiledPredicateIds → knowledge-edge path. Model must NOT infer UI from "navigation".

---

## Section 2 — Corruption Pairs (013)

---

### Corruption Pair B01-COR-013-001 — v6-01: "visual" missed, gap assigned

```
source_id: xiigen-b01-corruption-013-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  CompiledText: "Create a visual map of all user profile screens."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false

B-corrupt:
  CompiledPredicateIds: []
  RoutingDecisions:
    - KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Corruption type: Corruption-v6-01 — "visual" is in the UI lexicon; falsely routing to knowledge-edge suppresses valid UI routing.
