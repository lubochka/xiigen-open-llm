# Held-Out and Corruption Pairs (002)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO "component library" → UI on "component"; COR: component suppressed

---

## Section 1 — Held-Out Pair HO-002-A

```
source_id: xiigen-b01-held-out-002-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-002-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory and document the component library we use in the onboarding screens, with usage guidelines for each component."
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
  "component" fires UI (term 9 in the 22-term UI lexicon).
  "screens" fires UI (term 4).
  "document" fires DOCUMENT (term 1).
  "usage guidelines" — "guidelines" is not a DOCUMENT term; fires via "document" verb.
  Two routing decisions required. The model must recognize "component" as a full UI
  lexicon member and produce 1.1 routing, not treat "component library" as architecture.

---

## Section 1 — Held-Out Pair HO-002-B

```
source_id: xiigen-b01-held-out-002-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-002-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map out the component states for buttons, inputs, and modals across the onboarding flow."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false

Generalization test:
  "component" → UI. "buttons" — adjacent to "button" (UI term 16). "inputs" → UI (term 17).
  "modals" → UI (term 6).
  Multiple UI terms reinforce the predicate. "map out" does not fire any predicate via verb.
  "onboarding flow" — context only; no predicate.
  Result: UI alone → 1.1 routing.

---

## Section 2 — Corruption Pair COR-002

```
source_id: xiigen-b01-corruption-002
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-UI-predicate-missed-component-suppressed
```

A:
  DocumentId: "B01-002-comp-prompt"
  CompiledText: "Inventory the component library used across the onboarding and product screens."
  CompiledPredicateIds_correct: ["UI"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1"
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
  DecisionCount: 1

B-corrupt:
  CompiledPredicateIds: []
  Confidence: 0.70
  RoutingDecisions:
    - PredictedSubPhaseId: null
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"
  DecisionCount: 0

Corruption type: Corruption-v6-01 — UI predicate missed (component term suppressed).
  "component" is term 9 in the UI lexicon. The corrupted output fails to recognize it,
  producing an empty PredicateIds and routing to the knowledge-edge path.

Why this is harmful:
  The screen inventory for the component library is never executed. The correct
  behavior is to recognize "component" and "screens" as UI lexicon members and
  route to ScreenInventoryFlow11 at sub-phase 1.1. The knowledge-edge path produces
  a capability-learning signal instead of actual component inventory output.
