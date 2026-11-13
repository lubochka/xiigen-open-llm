# Held-Out and Corruption Pairs (021)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (021)

---

### Held-Out Pair B01-HO-021-001 — DOCUMENT+UI: spec for modal dialog

```
source_id: xiigen-b01-held-out-021-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-021-001"
  user_request: "Define the functional behavior of the delete confirmation modal and describe its visual hierarchy."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Generalization test:
  "modal" fires UI. "visual" fires UI.
  "Define", "functional", "behavior", "describe" fire DOCUMENT.
  Two predicates → two routing decisions. TransitionType: documentation (no IMPLEMENT).

---

### Held-Out Pair B01-HO-021-002 — DOCUMENT-only: "describe" without UI terms

```
source_id: xiigen-b01-held-out-021-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-021-002"
  user_request: "Describe the acceptance criteria for the wishlist sharing feature."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.89
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Generalization test:
  "Describe" fires DOCUMENT. "criteria" fires DOCUMENT.
  No UI terms present. Single DOCUMENT predicate → single routing decision to 2.1.
  TransitionType: documentation.

---

## Section 2 — Corruption Pairs (021)

---

### Corruption Pair B01-COR-021-001 — v6-03: DOCUMENT+UI dual routed to 1.1 only

```
source_id: xiigen-b01-corruption-021-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-03-multi-branch-result-collapse
```

A:
  CompiledText: "Describe the layout of the product modal with functional specifications."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1

Corruption type: Corruption-v6-03 — DOCUMENT predicate silently dropped; 2.1 never selected. Functional specification work is lost.
