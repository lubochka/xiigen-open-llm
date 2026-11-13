# Held-Out and Corruption Pairs (023)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (023) — Advanced Boundary Cases

---

### Held-Out Pair B01-HO-023-001 — "write user stories" → DOCUMENT+IMPLEMENT

```
source_id: xiigen-b01-held-out-023-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-023-001"
  user_request: "Write user stories for the checkout flow covering all screen transitions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  Confidence: 0.91
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "screen" fires UI.
  "stories" fires DOCUMENT (via "story" root).
  "write" fires IMPLEMENT (code context: "write user stories" is a special rule where both DOCUMENT and IMPLEMENT fire).
  Three predicates → three independent routing decisions.

---

### Held-Out Pair B01-HO-023-002 — "prototype" fires UI even in testing context

```
source_id: xiigen-b01-held-out-023-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-023-002"
  user_request: "Build a clickable prototype to test user flows before implementing the full checkout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "prototype" fires UI — regardless of "testing" context. "interaction" context does not override lexicon.
  "Build" fires IMPLEMENT. "implementing" fires IMPLEMENT.
  Two predicates → two routing decisions. TransitionType: feature_addition.

---

## Section 2 — Corruption Pairs (023) — Advanced Corruption Cases

---

### Corruption Pair B01-COR-023-001 — v6-01: "prototype" missed as UI, routed as gap

```
source_id: xiigen-b01-corruption-023-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  CompiledText: "Create an interactive prototype for the user onboarding screens."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

B-corrupt:
  CompiledPredicateIds: []
  RoutingDecisions:
    - KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Corruption type: Corruption-v6-01 — "prototype", "screens", "interactive" are all in the UI lexicon. Falsely routing to knowledge-edge suppresses both UI and IMPLEMENT routing.
