# Held-Out and Corruption Pairs (017)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (017)

---

### Held-Out Pair B01-HO-017-001 — CartView rephrased routes to 1.1 + IMPLEMENT

```
source_id: xiigen-b01-held-out-017-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-017-001"
  user_request: "Develop the cart screen with input fields for quantity and coupon code, and a responsive color theme."
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
  "screen" fires UI. "input" fires UI. "responsive" fires UI. "color" fires UI.
  "Develop" fires IMPLEMENT.
  Strong UI + IMPLEMENT signal → two routing decisions. TransitionType: feature_addition.

---

### Held-Out Pair B01-HO-017-002 — cart abandonment email → knowledge-edge

```
source_id: xiigen-b01-held-out-017-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-017-002"
  user_request: "Set up an automated email to recover users who abandon their cart at checkout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.60
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "email", "cart", "checkout", "abandon", "automated", "recover" — no predicate terms.
  Despite clear product intent, no lexicon matches. Empty predicates → knowledge-edge.

---

## Section 2 — Corruption Pairs (017)

---

### Corruption Pair B01-COR-017-001 — v6-02: probationary_accepted FallbackUsed incorrectly set true

```
source_id: xiigen-b01-corruption-017-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-02-flow-factory-fallback-suppression
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "CartAndCheckoutFlow11"
    trust_state: "probationary"
    gate_b_passed: true

B-correct:
  SelectedFlowId: "CartAndCheckoutFlow11"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

B-corrupt:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

Corruption type: Corruption-v6-02 inverse — probationary_accepted flow was rejected incorrectly. When gate_b_passed is true, the candidate flow must be accepted; fallback must NOT fire.
