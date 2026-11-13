# Held-Out and Corruption Pairs (015)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (015)

---

### Held-Out Pair B01-HO-015-001 — SettingsDashboard rephrased routes to 1.1

```
source_id: xiigen-b01-held-out-015-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-015-001"
  user_request: "We need a full layout diagram of all the settings panels in the application, covering every form input and spacing arrangement."
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
  "layout" fires UI. "form" fires UI. "input" fires UI. "spacing" fires UI.
  Four independent UI lexicon hits. Model must route to 1.1.
  "settings", "panels", "application" do not independently fire.

---

### Held-Out Pair B01-HO-015-002 — "user experience" longform does NOT fire UI

```
source_id: xiigen-b01-held-out-015-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-015-002"
  user_request: "Improve the overall user experience of the settings dashboard to reduce friction."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.65
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "user experience" longform does NOT fire UI — only "ux" fires UI.
  "improve", "friction", "reduce" — not in any predicate lexicon.
  Empty predicates → knowledge-edge. Model must not infer UI from "user experience".

---

## Section 2 — Corruption Pairs (015)

---

### Corruption Pair B01-COR-015-001 — v6-03: three-predicate merged into one routing decision

```
source_id: xiigen-b01-corruption-015-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-03-multi-branch-result-collapse
```

A:
  CompiledText: "Build and document the settings dashboard layout with full specification."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 3

B-corrupt:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI", "IMPLEMENT", "DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 1

Corruption type: Corruption-v6-03 — three predicates collapsed into one routing decision. Correct behavior: one decision per predicate, never merged.
