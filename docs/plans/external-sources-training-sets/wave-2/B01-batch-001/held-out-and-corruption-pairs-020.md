# Held-Out and Corruption Pairs (020)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (020)

---

### Held-Out Pair B01-HO-020-001 — AddressBook rephrased routes to 1.1 + IMPLEMENT

```
source_id: xiigen-b01-held-out-020-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-020-001"
  user_request: "Build the address book screen with a form for adding new addresses and implement the validation logic."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "screen" fires UI. "form" fires UI.
  "Build" fires IMPLEMENT. "implement" fires IMPLEMENT. "logic" fires IMPLEMENT.
  Two predicates → two routing decisions. TransitionType: feature_addition.

---

### Held-Out Pair B01-HO-020-002 — "checkout flow" → knowledge-edge

```
source_id: xiigen-b01-held-out-020-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-020-002"
  user_request: "Streamline the checkout flow to reduce the number of steps from address entry to payment."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.64
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "flow", "steps", "streamline", "checkout", "address", "payment" — not in any predicate lexicon.
  Empty predicates → knowledge-edge. "flow" and "steps" are not predicate terms.

---

## Section 2 — Corruption Pairs (020)

---

### Corruption Pair B01-COR-020-001 — v6-01: "layout" missed, predicate dropped

```
source_id: xiigen-b01-corruption-020-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  CompiledText: "Create a responsive layout for the address book and delivery options screens."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

B-corrupt:
  CompiledPredicateIds: ["IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Corruption type: Corruption-v6-01 — "layout", "responsive", "screens" are all UI lexicon terms. Dropping the UI predicate silently omits the 1.1 routing decision.
