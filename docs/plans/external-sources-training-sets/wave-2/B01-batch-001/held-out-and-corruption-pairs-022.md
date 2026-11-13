# Held-Out and Corruption Pairs (022)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (022)

---

### Held-Out Pair B01-HO-022-001 — IMPLEMENT+UI: "create" + "screen"

```
source_id: xiigen-b01-held-out-022-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-022-001"
  user_request: "Create the product detail page with an add-to-cart button and responsive image gallery component."
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
  "page" fires UI. "button" fires UI. "responsive" fires UI. "component" fires UI.
  "Create" fires IMPLEMENT.
  Two predicates → two routing decisions. TransitionType: feature_addition.

---

### Held-Out Pair B01-HO-022-002 — IMPLEMENT-only: "develop" without UI terms

```
source_id: xiigen-b01-held-out-022-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-022-002"
  user_request: "Develop the backend service to calculate tax on checkout totals."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["IMPLEMENT"]
  Confidence: 0.90
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "Develop" fires IMPLEMENT. No UI terms present.
  Single IMPLEMENT predicate → single routing decision to 3.1.
  TransitionType: feature_addition. "backend", "service", "tax", "totals" do not fire independently.

---

## Section 2 — Corruption Pairs (022)

---

### Corruption Pair B01-COR-022-001 — v6-01: IMPLEMENT+UI with UI predicate dropped

```
source_id: xiigen-b01-corruption-022-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  CompiledText: "Build a modal component with responsive typography."
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

Corruption type: Corruption-v6-01 — "modal", "component", "responsive", "typography" are all UI lexicon terms. Dropping UI predicate loses the 1.1 routing decision.
